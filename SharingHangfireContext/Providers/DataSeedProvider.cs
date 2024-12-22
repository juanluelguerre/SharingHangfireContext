using SharingHangfireContext.Entities;

namespace SharingHangfireContext.Providers;

public class DataSeedProvider(NotesDbContext context) : IDataSeedProvider
{
    public async Task Seed()
    {
        const int category = 1;

        context.Categories.Add(
            Category.Create(
                category, $"Category {category}",
                [
                    Note.Create(1, "Note 1", category),
                    Note.Create(2, "Note 2", category),
                    Note.Create(3, "Note 3", category)
                ]));

        await context.SaveChangesAsync();
    }
}
