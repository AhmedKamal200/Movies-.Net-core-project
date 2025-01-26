using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using MoviesApi.Dtos;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;
        public MoviesController(ApplicationDbContext context)
        {
            _context=context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _context.Movies
                .OrderByDescending(x=>x.Rate)
                .Include(m => m.Genre)
                .Select(m => new MovieDetailsDto
            { 
                Id = m.Id,
                Poster = m.Poster, 
                Rate = m.Rate, 
                StoryLine = m.StoryLine, 
                Title = m.Title, 
                Year = m.Year, 
                GenreId = m.GenreId, 
                GenreName = m.Genre.Name 
            })
                .ToListAsync();
            return Ok(movies);
        }
        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreid)
        {
            var movies = await _context.Movies
                .Where(m => m.GenreId == genreid)
               .OrderByDescending(x => x.Rate)
               .Include(m => m.Genre)
               .Select(m => new MovieDetailsDto
               {
                   Id = m.Id,
                   Poster = m.Poster,
                   Rate = m.Rate,
                   StoryLine = m.StoryLine,
                   Title = m.Title,
                   Year = m.Year,
                   GenreId = m.GenreId,
                   GenreName = m.Genre.Name
               })
               .ToListAsync();
            return Ok(movies);



        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _context.Movies.Include(n => n.Genre).SingleOrDefaultAsync(m => m.Id == id);
            if (movie == null)
                return NotFound();
            var dto = new MovieDetailsDto
            {
                Id = movie.Id,
                GenreId = movie.GenreId,
                GenreName = movie.Genre?.Name,
                Poster = movie.Poster,
                Rate = movie.Rate,
                StoryLine = movie.StoryLine,
                Title = movie.Title,
                Year = movie.Year
            };
            return Ok(dto);
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync( [FromForm] MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is required!");

            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed!");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1MB!");

            var isValidGenre = await _context.Genres.AnyAsync(g => g.Id == (dto.GenreId));

            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");


            using var dataStream= new MemoryStream();
            await dto.Poster.CopyToAsync(dataStream);
            var movie = new Movie
            {
                GenreId = dto.GenreId,
                Title = dto.Title,
                Poster = dataStream.ToArray(),
                Rate = dto.Rate,
                StoryLine = dto.StoryLine,
                Year = dto.Year

            };

            await _context.Movies.AddAsync(movie);
             _context.SaveChanges();
            return Ok(movie);


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
                return NotFound($"No movie was found with ID {id}");

            var isValidGenre = await _context.Genres.AnyAsync(g=>g.Id == (dto.GenreId));

            if (!isValidGenre)
                return BadRequest("Invalid genere ID!");

            if (dto.Poster != null)
            {   
                if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed!");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster is 1MB!");

                using var dataStream = new MemoryStream();

                await dto.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }

            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.StoryLine = dto.StoryLine;
            movie.Rate = dto.Rate;

            _context.SaveChanges();

            return Ok(movie);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync (id);
            if (movie == null)
                return NotFound($"no movie was found with id: {id}");
            _context.Remove(movie);
            _context.SaveChanges();
            return Ok(movie);
        }


    }
}