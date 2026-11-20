using System.Net;
using System.Net.Http.Json;
using ProjectAssetAPI.DTOs;
using Xunit;

namespace ProjectAssetAPI.Tests
{
    public sealed class VisualSecurityApiTests
    {
        [Fact]
        public async Task CreateSecurityEvent_ReturnsNotFound_WhenAssetDoesNotExist()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var response = await client.PostAsJsonAsync("/api/VisualSecurity", new CreateSecurityEventRequest
            {
                AssetId = 9999,
                EventType = "PortScan",
                Severity = "High",
                Description = "Escaneo detectado en equipo inexistente."
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SecurityEvents_Lifecycle_AndCascadeDelete_WorkCorrectly()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var asset = await (await client.PostAsJsonAsync("/api/VisualLabs/register", new RegisterAssetRequest
            {
                Hostname = "SEC-NODE-01",
                IPAddress = "10.0.1.15",
                OperatingSystem = "Windows Server 2022"
            })).Content.ReadFromJsonAsync<VisualLabResponse>();

            Assert.NotNull(asset);

            var createEventRes = await client.PostAsJsonAsync("/api/VisualSecurity", new CreateSecurityEventRequest
            {
                AssetId = asset.Id,
                EventType = "FailedLogin",
                Severity = "Critical",
                Description = "Múltiples intentos fallidos por RDP."
            });

            Assert.Equal(HttpStatusCode.Created, createEventRes.StatusCode);
            var createdEvent = await createEventRes.Content.ReadFromJsonAsync<VisualSecurityResponse>();
            Assert.NotNull(createdEvent);
            Assert.Equal("SEC-NODE-01", createdEvent.Hostname);
            Assert.Equal("Critical", createdEvent.Severity);

            // Verifica conteo reflejado en el dispositivo
            var fetchedAsset = await client.GetFromJsonAsync<VisualLabResponse>($"/api/VisualLabs/{asset.Id}");
            Assert.NotNull(fetchedAsset);
            Assert.Equal(1, fetchedAsset.SecurityEventCount);

            // Filtro por assetId y severity
            var filtered = await client.GetFromJsonAsync<List<VisualSecurityResponse>>(
                $"/api/VisualSecurity?assetId={asset.Id}&severity=Critical");
            Assert.NotNull(filtered);
            Assert.Single(filtered);

            // Al eliminar el dispositivo, los eventos asociados se eliminan en cascada
            var deleteAssetRes = await client.DeleteAsync($"/api/VisualLabs/{asset.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteAssetRes.StatusCode);

            var remainingEvents = await client.GetFromJsonAsync<List<VisualSecurityResponse>>("/api/VisualSecurity");
            Assert.NotNull(remainingEvents);
            Assert.Empty(remainingEvents);
        }

        [Fact]
        public async Task CreateSecurityEvent_RejectsInvalidSeverity()
        {
            await using var factory = new TestWebApplicationFactory();
            using var client = factory.CreateAuthorizedClient();

            var response = await client.PostAsJsonAsync("/api/VisualSecurity", new
            {
                assetId = 1,
                eventType = "Audit",
                severity = "InvalidLevel",
                description = "Prueba de validación"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
