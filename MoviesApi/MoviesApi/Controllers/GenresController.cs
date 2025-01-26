using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApi.Dtos;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public GenresController(ApplicationDbContext context)
        {
            _context = context;

        }
        [HttpGet]
        public async Task<ActionResult> GetAllAsync()
        {
            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            return Ok(genres);
        }
        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromBody] CreateGenreDto dto)
        {
            var genre = new Genre { Name = dto.Name };
            await _context.AddAsync(genre);
            _context.SaveChanges();

            return Ok(genre);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(int id,[FromBody] CreateGenreDto dto)
        {
            var genre= await _context.Genres.SingleOrDefaultAsync(g => g.Id == id);
            if (genre == null)
            {
                return NotFound($"No genre was found with iD: {id}");
            }

            genre.Name = dto.Name;
           
            _context.SaveChanges();

            return Ok(genre);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var genre = await _context.Genres.SingleOrDefaultAsync(g => g.Id == id);
            if (genre == null)
            {
                return NotFound($"No genre was found with iD: {id}");
            }
            _context.Remove(genre);
            _context.SaveChanges();
            return Ok(genre);
        }
    }
}