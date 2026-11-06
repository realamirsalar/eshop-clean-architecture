using E_Shop.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;

namespace E_Shop.Api.Tests.Intagration
{
    public class ApplicationApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("mastershop_test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        private DbConnection _dbConnection = default!;
        private Respawner _respawner = default!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.RemoveAll(typeof(ApplicationDbContext));

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });
            });
        }

        public async ValueTask InitializeAsync()
        {
            await _dbContainer.StartAsync();

            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
            await _dbConnection.OpenAsync();

            _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" } 
            });
        }

      
        public async Task ResetDatabaseAsync()
        {
            if (_respawner != null && _dbConnection != null)
            {
                await _respawner.ResetAsync(_dbConnection);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            if (_dbConnection != null)
            {
                await _dbConnection.CloseAsync();
                await _dbConnection.DisposeAsync();
            }

            NpgsqlConnection.ClearAllPools();

            await _dbContainer.DisposeAsync();
        }
    }
}
