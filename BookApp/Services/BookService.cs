using BookApp.Data;
using BookApp.Models;

namespace BookApp.Services
{
    public class BookService : IBookService
    {
        public const int MinPublishedYear = 1000;
        public const int MaxHtmlLength = 100_000;

        private readonly IBookRepository _books;
        private readonly IContentsXmlService _contents;

        public BookService(IBookRepository books, IContentsXmlService contents)
        {
            _books = books;
            _contents = contents;
        }

        public Task<IReadOnlyList<BookListRow>> GetListAsync(string? query)
        {
            var trimmed = query?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return _books.SelectAllAsync();
            }

            return _books.SearchAsync(trimmed);
        }

        public async Task<BookDetails?> GetAsync(long id)
        {
            var book = await _books.SelectByIdAsync(id);
            if (book is null)
            {
                return null;
            }

            return ToDetails(book);
        }

        public async Task<long> CreateAsync(BookForm form)
        {
            Validate(form);
            return await _books.InsertAsync(ToRecord(form));
        }

        public async Task<bool> UpdateAsync(BookForm form)
        {
            Validate(form);
            var affected = await _books.UpdateAsync(ToRecord(form));
            return affected > 0;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var affected = await _books.DeleteAsync(id);
            return affected > 0;
        }

        public void Validate(BookForm form)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(form.Title))
            {
                errors.Add("Укажите название.");
            }

            if (string.IsNullOrWhiteSpace(form.Author))
            {
                errors.Add("Укажите автора.");
            }

            var maxYear = DateTime.UtcNow.Year + 1;
            if (form.PublishedYear is int year && (year < MinPublishedYear || year > maxYear))
            {
                errors.Add($"Год издания должен быть от {MinPublishedYear} до {maxYear}.");
            }

            if (form.PageCount is < 0)
            {
                errors.Add("Число страниц не может быть отрицательным.");
            }

            if (form.ContentsHtml.Length > MaxHtmlLength)
            {
                errors.Add($"Оглавление не должно быть длиннее {MaxHtmlLength} символов.");
            }

            if (errors.Count > 0)
            {
                throw new BookValidationException(errors);
            }
        }

        private BookRecord ToRecord(BookForm form)
        {
            var html = form.ContentsHtml;
            return new BookRecord
            {
                Id = form.Id,
                Title = form.Title.Trim(),
                Author = form.Author.Trim(),
                PublishedYear = form.PublishedYear,
                Isbn = Normalize(form.Isbn),
                Publisher = Normalize(form.Publisher),
                Genre = Normalize(form.Genre),
                Language = Normalize(form.Language),
                Description = Normalize(form.Description),
                PageCount = form.PageCount,
                Contents = _contents.ToXml(html),
                ContentsText = _contents.ToPlainText(html)
            };
        }

        private BookDetails ToDetails(BookRecord book)
        {
            return new BookDetails
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                PublishedYear = book.PublishedYear,
                Isbn = book.Isbn,
                Publisher = book.Publisher,
                Genre = book.Genre,
                Language = book.Language,
                Description = book.Description,
                PageCount = book.PageCount,
                ContentsHtml = _contents.ToHtml(book.Contents ?? string.Empty),
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt
            };
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }
    }
}
