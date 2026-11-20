using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectAssetAPI.Context;
using ProjectAssetAPI.DTOs;
using ProjectAssetAPI.Models;
using ProjectAssetAPI.Options;

namespace ProjectAssetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisualLabsController : ControllerBase
    {
        private readonly VisualDBContext _context;
        private readonly IOptionsMonitor<SecurityOptions> _securityOptions;

        public VisualLabsController(
            VisualDBContext context,
            IOptionsMonitor<SecurityOptions> securityOptions)
        {
            _context = context;
            _securityOptions = securityOptions;
        }

        private TimeSpan HeartbeatTimeout =>
            TimeSpan.FromSeconds(Math.Max(1, _securityOptions.CurrentValue.HeartbeatTimeoutSeconds));

        // GET: api/VisualLabs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisualLabResponse>>> GetVisualLabs(CancellationToken cancellationToken)
        {
            var timeout = HeartbeatTimeout;
            var labs = await _context.VisualLabs
                .AsNoTracking()
                .OrderByDescending(l => l.LastSeenUtc)
                .Select(l => new
                {
                    Lab = l,
                    EventCount = l.SecurityEvents.Count
                })
                .ToListAsync(cancellationToken);

            var response = labs.Select(x => VisualLabResponse.FromEntity(x.Lab, timeout, x.EventCount));
            return Ok(response);
        }

        // GET: api/VisualLabs/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VisualLabResponse>> GetVisualLab(int id, CancellationToken cancellationToken)
        {
            var item = await _context.VisualLabs
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new
                {
                    Lab = l,
                    EventCount = l.SecurityEvents.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
            {
                return NotFound();
            }

            return Ok(VisualLabResponse.FromEntity(item.Lab, HeartbeatTimeout, item.EventCount));
        }

        // POST: api/VisualLabs/register (Upsert por Hostname + Heartbeat)
        [HttpPost("register")]
        public async Task<ActionResult<VisualLabResponse>> RegisterAsset(
            [FromBody] RegisterAssetRequest request,
            CancellationToken cancellationToken)
        {
            var normalizedHostname = request.Hostname.Trim();
            var normalizedIp = request.IPAddress.Trim();
            var normalizedOs = request.OperatingSystem.Trim();
            var now = DateTime.UtcNow;

            var existing = await _context.VisualLabs
                .FirstOrDefaultAsync(
                    l => l.Hostname.ToLower() == normalizedHostname.ToLower(),
                    cancellationToken);

            if (existing is not null)
            {
                existing.Hostname = normalizedHostname;
                existing.IPAddress = normalizedIp;
                existing.OperatingSystem = normalizedOs;
                existing.Status = string.IsNullOrWhiteSpace(request.Status) ? "Online" : request.Status;
                existing.LastSeenUtc = now;

                await _context.SaveChangesAsync(cancellationToken);

                var eventCount = await _context.VisualSecurities
                    .CountAsync(s => s.AssetId == existing.Id, cancellationToken);

                return Ok(VisualLabResponse.FromEntity(existing, HeartbeatTimeout, eventCount));
            }

            var created = new VisualLab
            {
                Hostname = normalizedHostname,
                IPAddress = normalizedIp,
                OperatingSystem = normalizedOs,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Online" : request.Status,
                CreatedAtUtc = now,
                LastSeenUtc = now
            };

            _context.VisualLabs.Add(created);
            await _context.SaveChangesAsync(cancellationToken);

            var response = VisualLabResponse.FromEntity(created, HeartbeatTimeout, 0);
            return CreatedAtAction(nameof(GetVisualLab), new { id = created.Id }, response);
        }

        // PUT: api/VisualLabs/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutVisualLab(
            int id,
            [FromBody] UpdateAssetRequest request,
            CancellationToken cancellationToken)
        {
            var existing = await _context.VisualLabs.FindAsync([id], cancellationToken);
            if (existing is null)
            {
                return NotFound();
            }

            var normalizedHostname = request.Hostname.Trim();
            var duplicateExists = await _context.VisualLabs
                .AnyAsync(
                    l => l.Id != id && l.Hostname.ToLower() == normalizedHostname.ToLower(),
                    cancellationToken);

            if (duplicateExists)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Hostname duplicado",
                    Detail = $"Ya existe otro dispositivo registrado con el hostname '{normalizedHostname}'.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            existing.Hostname = normalizedHostname;
            existing.IPAddress = request.IPAddress.Trim();
            existing.OperatingSystem = request.OperatingSystem.Trim();
            existing.Status = request.Status;
            if (string.Equals(request.Status, "Online", StringComparison.OrdinalIgnoreCase))
            {
                existing.LastSeenUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return NoContent();
        }

        // DELETE: api/VisualLabs/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteVisualLab(int id, CancellationToken cancellationToken)
        {
            var visualLab = await _context.VisualLabs.FindAsync([id], cancellationToken);
            if (visualLab is null)
            {
                return NotFound();
            }

            _context.VisualLabs.Remove(visualLab);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
