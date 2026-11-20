using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAssetAPI.Context;
using ProjectAssetAPI.DTOs;
using ProjectAssetAPI.Models;

namespace ProjectAssetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VisualSecurityController : ControllerBase
    {
        private readonly VisualDBContext _context;

        public VisualSecurityController(VisualDBContext context)
        {
            _context = context;
        }

        // GET: api/VisualSecurity?assetId=1&severity=High
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisualSecurityResponse>>> GetSecurityEvents(
            [FromQuery] int? assetId,
            [FromQuery] string? severity,
            CancellationToken cancellationToken)
        {
            var query = _context.VisualSecurities
                .AsNoTracking()
                .Include(e => e.Asset)
                .AsQueryable();

            if (assetId.HasValue)
            {
                query = query.Where(e => e.AssetId == assetId.Value);
            }

            if (!string.IsNullOrWhiteSpace(severity))
            {
                var normalizedSeverity = severity.Trim().ToLower();
                query = query.Where(e => e.Severity.ToLower() == normalizedSeverity);
            }

            var events = await query
                .OrderByDescending(e => e.CreatedAtUtc)
                .ToListAsync(cancellationToken);

            return Ok(events.Select(VisualSecurityResponse.FromEntity));
        }

        // GET: api/VisualSecurity/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<VisualSecurityResponse>> GetSecurityEvent(int id, CancellationToken cancellationToken)
        {
            var securityEvent = await _context.VisualSecurities
                .AsNoTracking()
                .Include(e => e.Asset)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (securityEvent is null)
            {
                return NotFound();
            }

            return Ok(VisualSecurityResponse.FromEntity(securityEvent));
        }

        // POST: api/VisualSecurity
        [HttpPost]
        public async Task<ActionResult<VisualSecurityResponse>> CreateSecurityEvent(
            [FromBody] CreateSecurityEventRequest request,
            CancellationToken cancellationToken)
        {
            var asset = await _context.VisualLabs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

            if (asset is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Dispositivo no encontrado",
                    Detail = $"No existe ningún dispositivo registrado con Id={request.AssetId}.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            var securityEvent = new VisualSecurity
            {
                AssetId = request.AssetId,
                EventType = request.EventType.Trim(),
                Severity = request.Severity.Trim(),
                Description = request.Description.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.VisualSecurities.Add(securityEvent);
            await _context.SaveChangesAsync(cancellationToken);

            securityEvent.Asset = asset;
            var response = VisualSecurityResponse.FromEntity(securityEvent);
            return CreatedAtAction(nameof(GetSecurityEvent), new { id = securityEvent.Id }, response);
        }

        // DELETE: api/VisualSecurity/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSecurityEvent(int id, CancellationToken cancellationToken)
        {
            var securityEvent = await _context.VisualSecurities.FindAsync([id], cancellationToken);
            if (securityEvent is null)
            {
                return NotFound();
            }

            _context.VisualSecurities.Remove(securityEvent);
            await _context.SaveChangesAsync(cancellationToken);

            return NoContent();
        }
    }
}
