using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProjectBooks.Data;
using ProjectBooks.Dtos;
using ProjectBooks.Models;

public class BookService
{
    private readonly HttpClient _httpClient;
    private readonly BookContext _bookContext;

    public BookService(HttpClient httpClient, BookContext bookContext)
    {
        _httpClient = httpClient;
        _bookContext = bookContext;
    }
    
    public async Task<IEnumerable<BookDto>> GetBooksAsync()
    {
        var response = await _httpClient.GetAsync("https://softwium.com/api/books");
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadAsStringAsync();
        var booksFromAPI = JsonConvert.DeserializeObject<List<Book>>(responseData);
            
        foreach (var book in booksFromAPI)
        {
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            if(book.Isbn == null)
            {
                book.Isbn = "Null";
            }    
            if (!_bookContext.Books.Any(b => b.Id == book.Id))
            {
                _bookContext.Books.Add(book);
            }
        }

        await _bookContext.SaveChangesAsync();

        return booksFromAPI.Select(book => new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Isbn = book.Isbn,
            PageCount = book.PageCount,
            Authors = string.Join(", ", book.Authors),
        }).ToList();
    }
}
