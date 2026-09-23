using BookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApp.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("books", table =>
                {
                    table.HasCheckConstraint(
                        "ck_books_page_count",
                        "page_count IS NULL OR page_count >= 0");
                });

                entity.HasKey(book => book.Id);

                entity.Property(book => book.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(book => book.Title)
                    .HasColumnName("title")
                    .IsRequired();

                entity.Property(book => book.Author)
                    .HasColumnName("author")
                    .IsRequired();

                entity.Property(book => book.PublishedYear)
                    .HasColumnName("published_year");

                entity.Property(book => book.Isbn)
                    .HasColumnName("isbn");

                entity.Property(book => book.Publisher)
                    .HasColumnName("publisher");

                entity.Property(book => book.Genre)
                    .HasColumnName("genre");

                entity.Property(book => book.Language)
                    .HasColumnName("language");

                entity.Property(book => book.Description)
                    .HasColumnName("description");

                entity.Property(book => book.PageCount)
                    .HasColumnName("page_count");

                entity.Property(book => book.Contents)
                    .HasColumnName("contents")
                    .HasColumnType("xml");

                entity.Property(book => book.ContentsText)
                    .HasColumnName("contents_text");

                entity.Property(book => book.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");

                entity.Property(book => book.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("now()");
            });
        }
    }
}
