namespace BookApp.Models
{
    public class BookListRow
    {
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public int? PublishedYear { get; set; }

        public string? Genre { get; set; }

        public string? Isbn { get; set; }
    }
}
