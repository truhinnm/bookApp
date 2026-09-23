namespace BookApp.Models
{
    public class BookRecord
    {
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public int? PublishedYear { get; set; }

        public string? Isbn { get; set; }

        public string? Publisher { get; set; }

        public string? Genre { get; set; }

        public string? Language { get; set; }

        public string? Description { get; set; }

        public int? PageCount { get; set; }

        public string? Contents { get; set; }

        public string? ContentsText { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
