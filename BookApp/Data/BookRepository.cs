using System.Data;
using BookApp.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace BookApp.Data
{
    public class BookRepository : IBookRepository
    {
        private const string ListCursor = "book_list_cursor";
        private const string CardCursor = "book_card_cursor";

        private readonly LibraryDbContext _context;

        static BookRepository()
        {
            DefaultTypeMap.MatchNamesWithUnderscores = true;
            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
        }

        public BookRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public Task<long> InsertAsync(BookRecord book)
        {
            var parameters = BookParameters(book);
            parameters.Add("p_id", null, DbType.Int64, ParameterDirection.InputOutput);
            return QuerySingleAsync<long>(
                """
                CALL book_insert(
                    p_title => @p_title,
                    p_author => @p_author,
                    p_published_year => @p_published_year,
                    p_isbn => @p_isbn,
                    p_publisher => @p_publisher,
                    p_genre => @p_genre,
                    p_language => @p_language,
                    p_description => @p_description,
                    p_page_count => @p_page_count,
                    p_contents => @p_contents,
                    p_contents_text => @p_contents_text,
                    p_id => @p_id
                )
                """,
                parameters);
        }

        public Task<int> UpdateAsync(BookRecord book)
        {
            var parameters = BookParameters(book);
            parameters.Add("p_id", book.Id, DbType.Int64);
            parameters.Add("p_affected", null, DbType.Int32, ParameterDirection.InputOutput);
            return QuerySingleAsync<int>(
                """
                CALL book_update(
                    p_id => @p_id,
                    p_title => @p_title,
                    p_author => @p_author,
                    p_published_year => @p_published_year,
                    p_isbn => @p_isbn,
                    p_publisher => @p_publisher,
                    p_genre => @p_genre,
                    p_language => @p_language,
                    p_description => @p_description,
                    p_page_count => @p_page_count,
                    p_contents => @p_contents,
                    p_contents_text => @p_contents_text,
                    p_affected => @p_affected
                )
                """,
                parameters);
        }

        public Task<int> DeleteAsync(long id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_id", id, DbType.Int64);
            parameters.Add("p_affected", null, DbType.Int32, ParameterDirection.InputOutput);
            return QuerySingleAsync<int>(
                """
                CALL book_delete(p_id => @p_id, p_affected => @p_affected)
                """,
                parameters);
        }

        public async Task<BookRecord?> SelectByIdAsync(long id)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_id", id, DbType.Int64);
            parameters.Add("p_cursor", new RefcursorParameter(CardCursor));
            var rows = await QueryCursorAsync<BookRecord>(
                """
                CALL book_select_by_id(p_id => @p_id, p_cursor => @p_cursor)
                """,
                parameters,
                CardCursor);
            return rows.SingleOrDefault();
        }

        public Task<IReadOnlyList<BookListRow>> SelectAllAsync()
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_cursor", new RefcursorParameter(ListCursor));
            return QueryCursorAsync<BookListRow>(
                """
                CALL book_select_all(p_cursor => @p_cursor)
                """,
                parameters,
                ListCursor);
        }

        public Task<IReadOnlyList<BookListRow>> SearchAsync(string query)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_query", query, DbType.String);
            parameters.Add("p_cursor", new RefcursorParameter(ListCursor));
            return QueryCursorAsync<BookListRow>(
                """
                CALL book_search(p_query => @p_query, p_cursor => @p_cursor)
                """,
                parameters,
                ListCursor);
        }

        private static DynamicParameters BookParameters(BookRecord book)
        {
            var parameters = new DynamicParameters();
            parameters.Add("p_title", book.Title, DbType.String);
            parameters.Add("p_author", book.Author, DbType.String);
            parameters.Add("p_published_year", book.PublishedYear, DbType.Int32);
            parameters.Add("p_isbn", book.Isbn, DbType.String);
            parameters.Add("p_publisher", book.Publisher, DbType.String);
            parameters.Add("p_genre", book.Genre, DbType.String);
            parameters.Add("p_language", book.Language, DbType.String);
            parameters.Add("p_description", book.Description, DbType.String);
            parameters.Add("p_page_count", book.PageCount, DbType.Int32);
            parameters.Add("p_contents", book.Contents, DbType.Xml);
            parameters.Add("p_contents_text", book.ContentsText, DbType.String);
            return parameters;
        }

        private async Task<T> QuerySingleAsync<T>(string sql, object parameters)
        {
            await _context.Database.OpenConnectionAsync();
            try
            {
                var connection = _context.Database.GetDbConnection();
                return await connection.QuerySingleAsync<T>(sql, parameters);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private async Task<IReadOnlyList<T>> QueryCursorAsync<T>(string sql, object parameters, string cursorName)
        {
            await _context.Database.OpenConnectionAsync();
            try
            {
                var connection = _context.Database.GetDbConnection();
                await using var transaction = await connection.BeginTransactionAsync();
                await connection.ExecuteAsync(sql, parameters, transaction);
                var rows = (await connection.QueryAsync<T>(
                    "FETCH ALL FROM " + cursorName,
                    transaction: transaction)).ToList();
                await transaction.CommitAsync();
                return rows;
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        private sealed class RefcursorParameter : SqlMapper.ICustomQueryParameter
        {
            private readonly string _cursorName;

            public RefcursorParameter(string cursorName)
            {
                _cursorName = cursorName;
            }

            public void AddParameter(IDbCommand command, string name)
            {
                command.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Refcursor)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = _cursorName
                });
            }
        }

        private sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
        {
            public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
            {
                parameter.Value = value;
            }

            public override DateTimeOffset Parse(object value)
            {
                return value switch
                {
                    DateTimeOffset dateTimeOffset => dateTimeOffset,
                    DateTime dateTime => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
                    _ => throw new DataException($"Не удалось прочитать дату типа {value.GetType().Name}.")
                };
            }
        }
    }
}
