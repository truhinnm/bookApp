using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookApp.Data
{
    public static class LibraryDatabaseInitializer
    {
        public static async Task InitializeAsync(LibraryDbContext context)
        {
            var creator = context.Database.GetService<IRelationalDatabaseCreator>()
                ?? throw new InvalidOperationException("Не удалось получить создатель базы данных.");

            if (!await creator.ExistsAsync())
            {
                await creator.CreateAsync();
            }

            await context.Database.MigrateAsync();
        }
    }
}
