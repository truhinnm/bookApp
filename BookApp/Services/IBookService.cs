using BookApp.Models;

namespace BookApp.Services
{
    public interface IBookService
    {
        Task<IReadOnlyList<BookListRow>> GetListAsync(string? query);

        Task<BookDetails?> GetAsync(long id);

        Task<long> CreateAsync(BookForm form);

        Task<bool> UpdateAsync(BookForm form);

        Task<bool> DeleteAsync(long id);

        void Validate(BookForm form);
    }
}
