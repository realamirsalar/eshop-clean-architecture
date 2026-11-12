using E_Shop.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Respawn;
using StackExchange.Redis;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

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

        private readonly RedisContainer _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();

        private DbConnection _dbConnection = default!;
        private Respawner _respawner = default!;
        private IConnectionMultiplexer _redisConnection = default!;

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

                services.RemoveAll(typeof(IDistributedCache));

                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = _redisContainer.GetConnectionString();
                });
            });
        }

        public async ValueTask InitializeAsync()
        {
            await Task.WhenAll(
                _dbContainer.StartAsync(),
                _redisContainer.StartAsync()
            );

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

            var redisOptions = ConfigurationOptions.Parse(_redisContainer.GetConnectionString());
            redisOptions.AllowAdmin = true;
            _redisConnection = await ConnectionMultiplexer.ConnectAsync(redisOptions);
        }

        public async Task ResetDatabaseAsync()
        {
            if (_respawner != null && _dbConnection != null)
            {
                await _respawner.ResetAsync(_dbConnection);
            }

            if (_redisConnection != null)
            {
                var endpoints = _redisConnection.GetEndPoints();
                foreach (var endpoint in endpoints)
                {
                    var server = _redisConnection.GetServer(endpoint);
                    await server.FlushAllDatabasesAsync();
                }
            }
        }

        public new async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();

            if (_dbConnection != null)
            {
                await _dbConnection.CloseAsync();
                await _dbConnection.DisposeAsync();
            }

            if (_redisConnection != null)
            {
                await _redisConnection.CloseAsync();
                await _redisConnection.DisposeAsync();
            }

            NpgsqlConnection.ClearAllPools();

            await Task.WhenAll(
                _dbContainer.DisposeAsync().AsTask(),
                _redisContainer.DisposeAsync().AsTask()
            );
        }
    }
}
