namespace BookApp.Data
{
    public static class LibraryProcedures
    {
        public const string CreateAll = """
            CREATE PROCEDURE book_insert(
                IN p_title text,
                IN p_author text,
                IN p_published_year integer,
                IN p_isbn text,
                IN p_publisher text,
                IN p_genre text,
                IN p_language text,
                IN p_description text,
                IN p_page_count integer,
                IN p_contents xml,
                IN p_contents_text text,
                OUT p_id bigint
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                INSERT INTO books (
                    title,
                    author,
                    published_year,
                    isbn,
                    publisher,
                    genre,
                    language,
                    description,
                    page_count,
                    contents,
                    contents_text,
                    created_at,
                    updated_at
                )
                VALUES (
                    p_title,
                    p_author,
                    p_published_year,
                    p_isbn,
                    p_publisher,
                    p_genre,
                    p_language,
                    p_description,
                    p_page_count,
                    p_contents,
                    p_contents_text,
                    now(),
                    now()
                )
                RETURNING id INTO p_id;
            END;
            $$;

            CREATE PROCEDURE book_update(
                IN p_id bigint,
                IN p_title text,
                IN p_author text,
                IN p_published_year integer,
                IN p_isbn text,
                IN p_publisher text,
                IN p_genre text,
                IN p_language text,
                IN p_description text,
                IN p_page_count integer,
                IN p_contents xml,
                IN p_contents_text text,
                OUT p_affected integer
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                UPDATE books
                SET
                    title = p_title,
                    author = p_author,
                    published_year = p_published_year,
                    isbn = p_isbn,
                    publisher = p_publisher,
                    genre = p_genre,
                    language = p_language,
                    description = p_description,
                    page_count = p_page_count,
                    contents = p_contents,
                    contents_text = p_contents_text,
                    updated_at = now()
                WHERE id = p_id;

                GET DIAGNOSTICS p_affected = ROW_COUNT;
            END;
            $$;

            CREATE PROCEDURE book_delete(
                IN p_id bigint,
                OUT p_affected integer
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                DELETE FROM books
                WHERE id = p_id;

                GET DIAGNOSTICS p_affected = ROW_COUNT;
            END;
            $$;

            CREATE PROCEDURE book_select_by_id(
                IN p_id bigint,
                INOUT p_cursor refcursor
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_cursor FOR
                    SELECT
                        id,
                        title,
                        author,
                        published_year,
                        isbn,
                        publisher,
                        genre,
                        language,
                        description,
                        page_count,
                        contents,
                        contents_text,
                        created_at,
                        updated_at
                    FROM books
                    WHERE id = p_id;
            END;
            $$;

            CREATE PROCEDURE book_select_all(
                INOUT p_cursor refcursor
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_cursor FOR
                    SELECT
                        id,
                        title,
                        author,
                        published_year,
                        genre,
                        isbn
                    FROM books
                    ORDER BY title, id;
            END;
            $$;

            CREATE PROCEDURE book_search(
                IN p_query text,
                INOUT p_cursor refcursor
            )
            LANGUAGE plpgsql
            AS $$
            BEGIN
                OPEN p_cursor FOR
                    SELECT
                        id,
                        title,
                        author,
                        published_year,
                        genre,
                        isbn
                    FROM books
                    WHERE title ILIKE '%' || p_query || '%'
                       OR author ILIKE '%' || p_query || '%'
                       OR contents_text ILIKE '%' || p_query || '%'
                    ORDER BY title, id;
            END;
            $$;
            """;

        public const string DropAll = """
            DROP PROCEDURE IF EXISTS book_search(text, refcursor);
            DROP PROCEDURE IF EXISTS book_select_all(refcursor);
            DROP PROCEDURE IF EXISTS book_select_by_id(bigint, refcursor);
            DROP PROCEDURE IF EXISTS book_delete(bigint);
            DROP PROCEDURE IF EXISTS book_update(bigint, text, text, integer, text, text, text, text, text, integer, xml, text);
            DROP PROCEDURE IF EXISTS book_insert(text, text, integer, text, text, text, text, text, integer, xml, text);
            """;
    }
}
