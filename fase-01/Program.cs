using System.Text;
using fase_01.application.dtos;
using fase_01.application.interfaces;
using fase_01.application.services;
using fase_01.domain.interfaces;
using fase_01.infrastructure.data;
using fase_01.infrastructure.repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar o EF Core com a Connection String do Azure SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                  // Número máximo de tentativas
            maxRetryDelay: TimeSpan.FromSeconds(10), // Intervalo entre tentativas
            errorNumbersToAdd: null);
        #region explicação do errorNumbersToAdd
        // A propriedade errorNumbersToAdd serve para registrar 
        // números de erros específicos do SQL Server aos quais 
        // a política de tentativa automática (retry policy) 
        // do Entity Framework Core deve reagir.

        // Por padrão, quando você chama .EnableRetryOnFailure(), 
        // o EF Core já possui uma lista interna pré-definida de códigos 
        // de erro transitórios conhecidos do SQL Server 
        // (por exemplo, erros de timeout de conexão, servidor ocupado, 
        // perda momentânea de pacote, etc.).

        // se a sua aplicação ou o seu banco no Azure SQL gerar um código 
        // de erro customizado ou um erro específico que o EF Core não 
        // trata por padrão como "transitório", você pode passá-lo na 
        // propriedade errorNumbersToAdd.

        // errorNumbersToAdd: new[] { 40613, 10928 } // Adiciona retentativas para erros específicos do Azure
        // * 40613: Database is not currently available (quando o banco Azure SQL está em pausa/acordando).
        // * 10928: Resource limit reached (limite de conexões ou DTUs atingido temporariamente no Azure).
        // Se você não tiver erros específicos para adicionar, pode deixar como null
        #endregion
    })
);

// Configurar o serviço de JWT
// 1) busca as informações de configuração do JWT no appsettings.json
builder.Services.Configure<JwtSettingsDto>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettingsDto = builder.Configuration.GetSection("JwtSettings").Get<JwtSettingsDto>();
if (jwtSettingsDto == null) throw new InvalidOperationException("JWT settings are not configured properly.");
var secretKey = Encoding.UTF8.GetBytes(jwtSettingsDto.SecretKey);

// 2) configura a autenticação via JWT Bearer e Cookie no pipeline do ASP.NET Core
builder.Services.AddAuthentication(options =>
{
    /*
    O que é: Define qual esquema será usado para reconhecer/identificar quem é o usuário a cada requisição que chega.
    O que acontece ao definir como JwtBearerDefaults.AuthenticationScheme:
    O ASP.NET Core usará o manipulador de JWT Bearer para inspecionar a requisição (lendo o cabeçalho Authorization ou o Cookie jwt_token), validar a assinatura do token e preencher a propriedade HttpContext.User com as claims do usuário.
    Sem isso: Se você não definir um esquema padrão de autenticação, o ASP.NET Core não saberá qual middleware chamar para ler o token, e o HttpContext.User ficará anônimo.
    */
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    /*
    O que é: Define o que o ASP.NET Core deve fazer quando um usuário não autenticado (anônimo) tenta acessar uma rota protegida por [Authorize]. A isso chamamos de "Challenge" (Desafio).
    O que acontece ao definir como JwtBearerDefaults.AuthenticationScheme:
    Quando um usuário anônimo tentar acessar uma rota restrita, o framework invocará o manipulador de JWT Bearer, que responderá automaticamente com o código de status HTTP 401 Unauthorized (e o cabeçalho WWW-Authenticate: Bearer).
    Comparação prática:
    Em APIs / JWT: O Challenge padrão retorna HTTP 401 Unauthorized.
    Em Aplicações MVC puras com Cookie: O Challenge padrão geralmente redireciona o usuário para a tela de login (/Account/Login?ReturnUrl=...).
    */
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Exige (ou não) que a comunicação e a descoberta de metadados do provedor de autenticação aconteçam exclusivamente via HTTPS.
    options.RequireHttpsMetadata = false; // em desenvolvimento

    // Instrui o ASP.NET Core a salvar o token JWT que veio na requisição dentro do contexto da requisição HTTP (HttpContext)
    // Se em alguma Controller ou Service você precisar acessar o token em texto puro 
    // (por exemplo, para repassar a chamada para outra API interna), 
    // você consegue recuperar com: 
    // var token = await HttpContext.GetTokenAsync("access_token");
    options.SaveToken = true;

    // Este objeto define a "lista de verificação" (checklist) que o ASP.NET executará 
    // em todo e qualquer token recebido antes de permitir o acesso às suas Controllers.
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        /* -- Resumo do fluxo de validação -- 
        
        Requisição HTTP com Token
         │
         ▼
        1. A chave bate com a nossa SecretKey? (ValidateIssuerSigningKey / IssuerSigningKey)
         │ Sim
         ▼
        2. Foi o nosso sistema que emitiu? (ValidateIssuer / ValidIssuer)
         │ Sim
         ▼
        3. Foi emitido para esta aplicação? (ValidateAudience / ValidAudience)
         │ Sim
         ▼
        4. O token ainda está no prazo de validade sem tolerância extra? (ValidateLifetime / ClockSkew)
         │ Sim
         ▼
        ✅ USUÁRIO AUTENTICADO! (Preenche HttpContext.User com as Claims)
        
        */



        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secretKey),

        ValidateIssuer = true,
        ValidIssuer = jwtSettingsDto.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtSettingsDto.Audience,

        // Verifica se o token ainda está dentro do seu prazo de validade 
        // (comparando as claims nbf [Not Before] e exp [Expiration] com o horário atual 
        // DateTime.UtcNow). Se o token expirou, retorna 401 Unauthorized.
        ValidateLifetime = true,
        // Por padrão, o .NET adiciona uma tolerância de 5 minutos (ClockSkew = TimeSpan.FromMinutes(5)) para compensar possíveis desincronizações de relógio entre diferentes servidores.
        // Definindo TimeSpan.Zero, você remove essa tolerância. Se o token expirar às 14:00:00, às 14:00:01 ele já é considerado inválido instantaneamente.
        ClockSkew = TimeSpan.Zero // Remove o tempo de tolerância padrão de 5 minutos
    };

    // Extrai o token do cookie, se presente, para permitir autenticação via cookie
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        /*
        Explica como recuperar a informação do token JWT da requisição HTTP, 
        caso ele venha em um cookie (em vez do cabeçalho Authorization).
        
        1. Se for uma API REST pura (consumida via Angular, React, Postman, Mobile):
        - O cliente envia no Header: Authorization: Bearer <token>.
        - Você não precisa de OnMessageReceived.

        2. Se for uma Aplicação Web MVC com Views/HTML e Cookies:
        - O navegador guarda o token num Cookie.
        - Você obrigatoriamente precisa do OnMessageReceived para extrair do cookie e atribuir a context.Token (independente do nome que você der ao cookie).
        */
        OnMessageReceived = context =>
        {
            /* essa é uma forma de fazer
            var accessToken = context.Request.Cookies["jwt_token"];
            if (!string.IsNullOrEmpty(accessToken))
                context.Token = accessToken;
            */

            // essa é outra forma de fazer
            if (context.Request.Cookies.TryGetValue("jwt_token", out string? token))
                context.Token = token;

            return Task.CompletedTask;
        },

        // Executado imediatamente APÓS o token ser validado com sucesso
        OnTokenValidated = async context =>
        {
            // 1. Extrai a claim do ID do usuário do token
            var userIdClaim = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                context.Fail("Token não contém um ID de usuário válido.");
                return;
            }

            // 2. Resolve o repositório/DbContext através do Injeção de Dependências da requisição
            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

            // 3. Busca o usuário atualizado no banco de dados
            var user = await userRepository.GetByIdAsync(userId); // ou método de busca equivalente

            if (user == null)
            {
                // Se o usuário foi deletado ou bloqueado no banco, invalida a requisição
                context.Fail("Usuário não encontrado ou inativo no banco de dados.");
                return;
            }

            // 4. Cria uma nova identidade com os dados atualizados vindos do BANCO DE DADOS
            var claims = new List<System.Security.Claims.Claim>
            {
                new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(System.Security.Claims.ClaimTypes.Name, user.FullName),
                new(System.Security.Claims.ClaimTypes.Email, user.Email),
                new("nickname", user.NickName ?? string.Empty),
                new(System.Security.Claims.ClaimTypes.Role, user.Admin ? "Admin" : "User")
            };

            var appIdentity = new System.Security.Claims.ClaimsIdentity(claims, context.Scheme.Name);

            // Atualiza o Principal da requisição com as claims do banco
            context.Principal = new System.Security.Claims.ClaimsPrincipal(appIdentity);
        }
    };
});

// Add services to the container.
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IGamePhotoRepository, GamePhotoRepository>();
builder.Services.AddScoped<IUserPhotoRepository, UserPhotoRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

// Configurar Web API puro (Substituindo AddControllersWithViews por AddControllers)
builder.Services.AddControllers();

// 1. Configurar OpenAPI Nativo do .NET 10 com suporte a JWT
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "FIAP Cloud Games API",
            Version = "v1",
            Description = "API RESTful para gerenciamento de jogos e usuários"
        };

        // Adiciona a definição do Bearer Token
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Insira o token JWT gerado no endpoint de login"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes?.Add("Bearer", scheme);

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// 2. Habilitar o endpoint do OpenAPI e a Interface do Scalar no ambiente de Desenvolvimento
if (app.Environment.IsDevelopment())
{
    // Expõe a documentação JSON em /openapi/v1.json
    app.MapOpenApi();

    // Expõe a UI moderna do Scalar em /scalar/v1
    app.MapScalarApiReference(options =>
    {
        options.Title = "FIAP Cloud Games API - Documentação";
        options.Theme = ScalarTheme.Purple;
        options.DefaultHttpClient = new(ScalarTarget.Http, ScalarClient.Http11);
    });
}

app.UseHttpsRedirection();
app.UseRouting();


// Para JWT precisa habilitar Middlewares de Autenticação e Autorização na ordem correta
app.UseAuthentication(); // <- OBRIGATÓRIO estar antes do UseAuthorization
app.UseAuthorization();

// Mapear rotas de API Controllers
app.MapControllers();


app.Run();
