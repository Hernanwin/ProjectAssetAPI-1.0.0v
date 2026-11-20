using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using ProjectAssetAPI.Context;
using ProjectAssetAPI.DTOs;
using ProjectAssetAgent.Services;
using Xunit;

namespace ProjectAssetAPI.Tests
{
    public sealed class VisualLabsApiTests
    {
        [Fact]
        public async Task HealthEndpoint_AllowsAnonymousAccess()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/health");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task VisualLabsEndpoint_RequiresValidApiKey()
        {
            await using var factory = new TestWebApplicationFactory();
            using var anonClient = factory.CreateClient();

            var missingKeyResponse = await anonClient.GetAsync("/api/VisualLabs");
            Assert.Equal(HttpStatusCode.Unauthorized, missingKeyResponse.StatusCode);

            using var wrongKeyRequest = new HttpRequestMessage(HttpMethod.Get, "/api/VisualLabs");
            wrongKeyRequest.Headers.Add("X-Api-Key", "wrong-key");
            var wrongKeyResponse = await anonClient.SendAsync(wrongKeyRequest);
            Assert.Equal(HttpStatusCode.Unauthorized, wrongKeyResponse.StatusCode);

            using var authClient = factory.CreateAuthorizedClient();
            var okResponse = await authClient.GetAsync("/api/VisualLabs");
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        }

        [Fact]
        public async Task RegisterAsset_PerformsUpsertByHostname_WithoutDuplicates()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var firstPayload = new RegisterAssetRequest
            {
                Hostname = "LAB-PC-01",
                IPAddress = "192.168.0.10",
                OperatingSystem = "Windows 11"
            };

            var createResponse = await client.PostAsJsonAsync("/api/VisualLabs/register", firstPayload);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<VisualLabResponse>();
            Assert.NotNull(created);
            Assert.Equal("Online", created.Status);

            var secondPayload = new RegisterAssetRequest
            {
                Hostname = "lab-pc-01",
                IPAddress = "192.168.0.99",
                OperatingSystem = "Windows 11 Pro 24H2"
            };

            var updateResponse = await client.PostAsJsonAsync("/api/VisualLabs/register", secondPayload);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updated = await updateResponse.Content.ReadFromJsonAsync<VisualLabResponse>();
            Assert.NotNull(updated);
            Assert.Equal(created.Id, updated.Id);
            Assert.Equal("192.168.0.99", updated.IPAddress);
            Assert.Equal("Windows 11 Pro 24H2", updated.OperatingSystem);

            var list = await client.GetFromJsonAsync<List<VisualLabResponse>>("/api/VisualLabs");
            Assert.NotNull(list);
            Assert.Single(list);
        }

        [Fact]
        public async Task GetVisualLab_ComputesOfflineStatus_WhenHeartbeatExpired()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var createResponse = await client.PostAsJsonAsync("/api/VisualLabs/register", new RegisterAssetRequest
            {
                Hostname = "STALE-NODE",
                IPAddress = "10.0.0.5",
                OperatingSystem = "Linux"
            });
            var created = await createResponse.Content.ReadFromJsonAsync<VisualLabResponse>();
            Assert.NotNull(created);

            using (var scope = factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<VisualDBContext>();
                var entity = await db.VisualLabs.FindAsync(created.Id);
                Assert.NotNull(entity);
                entity.LastSeenUtc = DateTime.UtcNow.AddMinutes(-10);
                await db.SaveChangesAsync();
            }

            var fetched = await client.GetFromJsonAsync<VisualLabResponse>($"/api/VisualLabs/{created.Id}");
            Assert.NotNull(fetched);
            Assert.Equal("Offline", fetched.Status);
        }

        [Fact]
        public async Task PutVisualLab_RejectsDuplicateHostnameWithConflict()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var first = await (await client.PostAsJsonAsync("/api/VisualLabs/register", new RegisterAssetRequest
            {
                Hostname = "HOST-A",
                IPAddress = "10.0.0.1",
                OperatingSystem = "Windows 11"
            })).Content.ReadFromJsonAsync<VisualLabResponse>();

            var second = await (await client.PostAsJsonAsync("/api/VisualLabs/register", new RegisterAssetRequest
            {
                Hostname = "HOST-B",
                IPAddress = "10.0.0.2",
                OperatingSystem = "Windows 11"
            })).Content.ReadFromJsonAsync<VisualLabResponse>();

            Assert.NotNull(first);
            Assert.NotNull(second);

            var conflictResponse = await client.PutAsJsonAsync($"/api/VisualLabs/{second.Id}", new UpdateAssetRequest
            {
                Hostname = "host-a",
                IPAddress = "10.0.0.2",
                OperatingSystem = "Windows 11",
                Status = "Online"
            });

            Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        }

        [Fact]
        public async Task RegisterAsset_ReturnsBadRequest_OnInvalidPayload()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var response = await client.PostAsJsonAsync("/api/VisualLabs/register", new
            {
                hostname = "",
                ipAddress = "",
                operatingSystem = ""
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public void SystemCollector_ReturnsNonEmptyMachineMetadata()
        {
            Assert.False(string.IsNullOrWhiteSpace(SystemCollector.GetHostname()));
            Assert.False(string.IsNullOrWhiteSpace(SystemCollector.GetIPAddress()));
            Assert.False(string.IsNullOrWhiteSpace(SystemCollector.GetOperatingSystem()));
        }
    }
}
