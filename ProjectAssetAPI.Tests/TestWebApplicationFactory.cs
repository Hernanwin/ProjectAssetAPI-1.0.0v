using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectAssetAPI.Context;
using ProjectAssetAPI.Security;

namespace ProjectAssetAPI.Tests
{
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        public const string TestApiKey = "test-secret-api-key";
        private readonly string _databaseName = $"AssetTestDb-{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Security:ApiKey"] = TestApiKey,
                    ["Security:HeartbeatTimeoutSeconds"] = "60"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<VisualDBContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<VisualDBContext>>();

                services.AddDbContext<VisualDBContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }

        public HttpClient CreateAuthorizedClient()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Add(ApiKeyAuthenticationHandler.HeaderName, TestApiKey);
            return client;
        }
    }
}
