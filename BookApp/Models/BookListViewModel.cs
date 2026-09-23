namespace BookApp.Models
{
    public class BookListViewModel
    {
        public string? Query { get; set; }

        public IReadOnlyList<BookListRow> Items { get; set; } = [];
    }
}
