using BookApp.Models;
using BookApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookApp.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _books;

        public BooksController(IBookService books)
        {
            _books = books;
        }

        public async Task<IActionResult> Index(string? query)
        {
            var items = await _books.GetListAsync(query);
            return View(new BookListViewModel
            {
                Query = query,
                Items = items
            });
        }

        public async Task<IActionResult> Details(long id)
        {
            var book = await _books.GetAsync(id);
            if (book is null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new BookForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookForm form)
        {
            try
            {
                var id = await _books.CreateAsync(form);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (BookValidationException exception)
            {
                AddValidationErrors(exception);
                return View(form);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var book = await _books.GetAsync(id);
            if (book is null)
            {
                return NotFound();
            }

            return View(ToForm(book));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookForm form)
        {
            try
            {
                var updated = await _books.UpdateAsync(form);
                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Details), new { id = form.Id });
            }
            catch (BookValidationException exception)
            {
                AddValidationErrors(exception);
                return View(form);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            await _books.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private void AddValidationErrors(BookValidationException exception)
        {
            foreach (var error in exception.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private static BookForm ToForm(BookDetails book)
        {
            return new BookForm
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
                ContentsHtml = book.ContentsHtml
            };
        }
    }
}
