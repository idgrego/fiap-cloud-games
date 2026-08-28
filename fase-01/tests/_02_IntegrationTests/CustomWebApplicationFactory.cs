using fase_01.infrastructure.data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace fase_01.tests.IntegrationTestes
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // ✅ AQUI: O nome do banco é gerado APENAS UMA VEZ por instância de teste
        private readonly string _dbName = "IntegrationTestsDb_" + Guid.NewGuid();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1. Remove TODAS as registros do AppDbContext e opções adicionados pelo Program.cs (SqlServer)
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(AppDbContext) ||
                    d.ServiceType.Name.Contains("IDbContextOptionsConfiguration")).ToList();

                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                // 2. Cria um ServiceProvider interno isolado para o EF Core em memória
                var internalServiceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // 3. Adiciona o AppDbContext utilizando o banco em memória com o ServiceProvider isolado
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                    options.UseInternalServiceProvider(internalServiceProvider);
                });

                // 4. garante a criação do banco em memória limpo
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            });

            base.ConfigureWebHost(builder);
        }
    }
}