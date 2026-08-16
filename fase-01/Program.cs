using fase_01.application.interfaces;
using fase_01.application.services;
using fase_01.domain.interfaces;
using fase_01.infrastructure.data;
using fase_01.infrastructure.repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar o EF Core com a Connection String do Azure SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                  // Número máximo de tentativas
            maxRetryDelay: TimeSpan.FromSeconds(10), // Intervalo entre tentativas
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
            errorNumbersToAdd: null);
    })
);

// Add services to the container.
builder.Services.AddScoped<IGamePhotoRepository, GamePhotoRepository>();
builder.Services.AddScoped<IUserPhotoRepository, UserPhotoRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPhotoService, PhotoService>();


builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
