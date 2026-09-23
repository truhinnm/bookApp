using BookApp.Models;

namespace BookApp.Data
{
    public interface IBookRepository
    {
        Task<long> InsertAsync(BookRecord book);

        Task<int> UpdateAsync(BookRecord book);

        Task<int> DeleteAsync(long id);

        Task<BookRecord?> SelectByIdAsync(long id);

        Task<IReadOnlyList<BookListRow>> SelectAllAsync();

        Task<IReadOnlyList<BookListRow>> SearchAsync(string query);
    }
}
