using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThormaBackendAPI.Data;
using ThormaBackendAPI.Models;

namespace ThormaBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FestokController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FestokController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Festok
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Festo>>> GetFestok()
        {
            return await _context.Festok.ToListAsync();
        }

        // GET: api/Festok/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Festo>> GetFesto(int id)
        {
            var festo = await _context.Festok.FindAsync(id);

            if (festo == null)
            {
                return NotFound();
            }

            return festo;
        }

        // PUT: api/Festok/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFesto(int id, Festo festo)
        {
            if (id != festo.Azon)
            {
                return BadRequest();
            }

            _context.Entry(festo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FestoExists(id))
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

        // POST: api/Festok
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Festo>> PostFesto(Festo festo)
        {
            _context.Festok.Add(festo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFesto", new { id = festo.Azon }, festo);
        }

        // DELETE: api/Festok/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFesto(int id)
        {
            var festo = await _context.Festok.FindAsync(id);
            if (festo == null)
            {
                return NotFound();
            }

            _context.Festok.Remove(festo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FestoExists(int id)
        {
            return _context.Festok.Any(e => e.Azon == id);
        }
    }
}
