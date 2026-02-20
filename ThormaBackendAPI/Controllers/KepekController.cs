using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThormaBackendAPI.Data;
using ThormaBackendAPI.Models;

namespace ThormaBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class KepekController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KepekController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Kep
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kep>>> GetKepek()
        {
            return Ok(await _context.Kepek
                .Select(k => new
                {
                    k.Leltar,
                    k.Cim,
                    k.Keszult,
                    k.Anyag,
                    k.Technika,
                    k.Szeles,
                    k.Magas,
                    FestoNev = k.Festo != null ? k.Festo.Nev : null
                })
                .ToListAsync());
        }

        // GET: api/Kep/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Kep>> GetKep(string id)
        {
            var kep = await _context.Kepek.FindAsync(id);

            if (kep == null)
            {
                return NotFound();
            }

            return kep;
        }

        // PUT: api/Kep/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutKep(string id, Kep kep)
        {
            if (id != kep.Leltar)
            {
                return BadRequest();
            }

            _context.Entry(kep).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KepExists(id))
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

        // POST: api/Kep
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Kep>> PostKep(Kep kep)
        {
            _context.Kepek.Add(kep);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (KepExists(kep.Leltar))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetKep", new { id = kep.Leltar }, kep);
        }

        // DELETE: api/Kep/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteKep(string id)
        {
            var kep = await _context.Kepek.FindAsync(id);
            if (kep == null)
            {
                return NotFound();
            }

            _context.Kepek.Remove(kep);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool KepExists(string id)
        {
            return _context.Kepek.Any(e => e.Leltar == id);
        }
    }
}
