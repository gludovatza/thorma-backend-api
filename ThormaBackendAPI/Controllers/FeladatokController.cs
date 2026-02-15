using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThormaBackendAPI.Data;

namespace ThormaBackendAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeladatokController : ControllerBase
    {
        private readonly AppDbContext _db;
        public FeladatokController(AppDbContext db) => _db = db;

        [HttpGet("2haboru")]
        public async Task<IEnumerable<string>> Eltek1914ben()
            => await _db.Festok
                .Where(f => f.Szuletett <= 1914 && f.Meghalt >= 1914)
                .OrderBy(f => f.Nev)
                .Select(f => f.Nev)
                .ToListAsync();

        [HttpGet("3anyag")]
        public async Task<IEnumerable<string>> OlajAnyagok()
            => await _db.Kepek
                .Where(k => k.Technika == "olaj")
                .Select(k => k.Anyag)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

        [HttpGet("4domb")]
        public async Task<IEnumerable<object>> DombosKepek()
            => await _db.Kepek
                .Where(k => k.Cim.Contains("domb"))
                .Select(k => new {
                    k.Cim,
                    k.Szeles,
                    k.Magas,
                    FestoNev = k.Festo!.Nev
                })
                .ToListAsync();

        [HttpGet("5magas")]
        public async Task<object?> Legmagasabb()
            => await _db.Kepek
                .OrderByDescending(k => k.Magas)
                .Select(k => new { k.Cim, FestoNev = k.Festo!.Nev })
                .FirstOrDefaultAsync();

        [HttpGet("6evek")]
        public async Task<IEnumerable<object>> EvekDarabszam()
            => await _db.Kepek
                .GroupBy(k => k.Keszult)
                .Select(g => new { Keszult = g.Key, Darab = g.Count() })
                .OrderByDescending(x => x.Darab)
                .ThenBy(x => x.Keszult)
                .ToListAsync();
    }
}
