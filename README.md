# Домашняя библиотека

Каталог книг: список, карточка, создание, правка и удаление. Оглавление редактируется в Quill и хранится в Postgres как XML. Поиск идёт по названию, автору и тексту оглавления.

## Стек

- C#, ASP.NET Core MVC, .NET 10
- Entity Framework Core и Npgsql: модель таблицы, соединение, миграции
- Dapper: вызов хранимых процедур
- PostgreSQL
- Quill 2: HTML-редактор оглавления
- Bootstrap

Строка подключения: `ConnectionStrings:Library` в `appsettings.json`. При старте создаётся база `home_library`, если её ещё нет, и накатывается миграция с таблицей `books` и процедурами.

## Модули

- **BooksController** — страницы списка, карточки, создания и правки. Удаление только через POST.
- **BookService** — сценарии библиотеки. Проверяет название, автора, год, число страниц и размер оглавления. Пустой поиск возвращает весь список.
- **ContentsXmlService** — переводит HTML редактора в XML `<toc>`, обратно в HTML для карточки и в плоский текст для поиска. Убирает скрипты и обработчики событий.
- **BookRepository** — вызывает процедуры через Dapper. `book_insert`, `book_update` и `book_delete` возвращают выходной параметр. `book_select_by_id`, `book_select_all` и `book_search` отдают строки через `refcursor`.
- **LibraryDbContext** — описание таблицы `books`. Колонка `contents` имеет тип `xml`, `contents_text` используется только для поиска.

Запуск из папки `BookApp`:

```bash
dotnet run
```
