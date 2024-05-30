using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectBooks.Data;
using ProjectBooks.Models;
using ProjectBooks.Dtos;
using static System.Reflection.Metadata.BlobBuilder;

namespace ProjectBooks.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly BookContext _bookContext;
        public BookController(BookService bookService, BookContext bookContext)
        {
            _bookService = bookService;
            _bookContext = bookContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> Get()
        {
            var books = await _bookService.GetBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _bookContext.Books.FindAsync(id);
            if(book == null)
            {
                return NotFound();
            }
            return book;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var books = await _bookContext.Books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(books);
        }

        [HttpGet("specific-condition")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBySpecificCondition([FromQuery] string author, [FromQuery] string title, [FromQuery] string isbn)
        {
            var books = await _bookContext.Books.ToListAsync();

            if (!string.IsNullOrEmpty(author))
            {
                books = books.Where(b => b.Authors.Contains(author)).ToList();
            }
            if (!string.IsNullOrEmpty(title))
            {
                books = books.Where(b => b.Title.Contains(title)).ToList();
            }
            if (!string.IsNullOrEmpty(isbn))
            {
                books = books.Where(b => b.Isbn.Contains(isbn)).ToList();
            }

            return Ok(books);
        }

        [HttpGet("author/{author}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetByAuthor(string author, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number must be greater than 0.");
            }

            var books = await _bookContext.Books.ToListAsync();
            var filteredBooks = books.Where(b => b.Authors.Contains(author));

            var totalBooks = filteredBooks.Count();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var paginatedBooks = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalBooks = totalBooks,
                Books = paginatedBooks
            };

            return Ok(response);
        }


        [HttpGet("title/{title}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetByTitle(string title, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number must be greater than 0.");
            }    
            var books = await _bookContext.Books.ToListAsync();
            var filteredBooks = books.Where(b => b.Title.Contains(title));

            var totalBooks = filteredBooks.Count();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var paginatedBooks = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Page = page,
                PageSize = pageSize,
                TotalsPages = totalPages,
                Book = paginatedBooks
            };   

            return Ok(filteredBooks);
        }

        [HttpGet("isbn/{isbn}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetByIsbn(string isbn, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number must be greater than 0.");
            }    
            var books = await _bookContext.Books.ToListAsync();
            var filteredBooks = books.Where(b => b.Isbn.Contains(isbn));

            var totalBooks = filteredBooks.Count();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var paginatedBooks = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Page = page,
                PageSize = pageSize,
                TotalsPages = totalPages,
                Book = paginatedBooks
            };

            return Ok(filteredBooks);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Book>>> GetByKeyword([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Keyword is required.");
            }

            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest("Page number must be greater than 0.");
            }

            var books = await _bookContext.Books
                .Where(b => b.Title.Contains(keyword) || b.Authors.Any(a => a.Contains(keyword)) || b.Isbn == keyword)
                .ToListAsync();
            //            var books = await _bookContext.Books.ToListAsync();
            /*var books = await _bookContext.Books
                .Where(b => b.Title.Contains(keyword) || b.Authors.Any(a => a.Contains(keyword)) || b.Isbn == keyword)
                .ToListAsync();
            var books = await query.ToListAsync();
            books = books.Where(b => b.Authors.Any(a => a.Contains(keyword))).ToList();*/

            if (books.Count == 0)
            {
                return NotFound("No books found matching the search criteria.");
            }
            var totalBooks = books.Count();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var paginatedBooks = books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalBooks = totalBooks,
                Books = paginatedBooks
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<Book>> CreateBook([FromBody] Book book)
        {
            if (book == null)
            {
                return BadRequest("Book is null.");
            }

            var existingBook = await _bookContext.Books.FindAsync(book.Id);
            if (existingBook != null)
            {
                return Conflict($"A book with ID {book.Id} already exists.");
            }

            _bookContext.Books.Add(book);
            await _bookContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id)
            {
                return BadRequest("Not Found.");
            }

            var existingBook = await _bookContext.Books.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            existingBook.Title = book.Title;
            existingBook.Isbn = book.Isbn;
            existingBook.PageCount = book.PageCount;
            existingBook.Authors = book.Authors;

            _bookContext.Entry(existingBook).State = EntityState.Modified;
            await _bookContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookContext.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            _bookContext.Books.Remove(book);
            await _bookContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("all")]
        public async Task<IActionResult> DeleteAll()
        {
            var book = await _bookContext.Books.ToListAsync();
            if (book.Count == 0 )
            {
                return NotFound();
            }
            _bookContext.Books.RemoveRange(book);
            await _bookContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
