using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAssetAPI.Context;
using ProjectAssetAPI.Models;

namespace ProjectAssetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisualLabsController : ControllerBase
    {
        private readonly VisualDBContext _context;

        public VisualLabsController(VisualDBContext context)
        {
            _context = context;
        }

        // GET: api/VisualLabs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VisualLab>>> GetVisualLabs()
        {
            return await _context.VisualLabs.ToListAsync();
        }

        // GET: api/VisualLabs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VisualLab>> GetVisualLab(int id)
        {
            var visualLab = await _context.VisualLabs.FindAsync(id);

            if (visualLab == null)
            {
                return NotFound();
            }

            return visualLab;
        }

        // PUT: api/VisualLabs/5
        
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVisualLab(int id, VisualLab visualLab)
        {
            if (id != visualLab.Id)
            {
                return BadRequest();
            }

            _context.Entry(visualLab).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VisualLabExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/VisualLabs/register
       
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsset([FromBody] VisualLab asset)
        {
            asset.Status = "Online";

            _context.VisualLabs.Add(asset);

            await _context.SaveChangesAsync();

            return Ok(asset);
        }


        // DELETE: api/VisualLabs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisualLab(int id)
        {
            var visualLab = await _context.VisualLabs.FindAsync(id);
            if (visualLab == null)
            {
                return NotFound();
            }

            _context.VisualLabs.Remove(visualLab);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VisualLabExists(int id)
        {
            return _context.VisualLabs.Any(e => e.Id == id);
        }
    }
}
