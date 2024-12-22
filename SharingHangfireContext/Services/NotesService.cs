using SharingHangfireContext.Accessors;
using SharingHangfireContext.Entities;

namespace SharingHangfireContext.Services;

public class NotesService(
    NotesDbContext dbContext,
    IContextAccessor contextAccessor)
{
    public List<Note> GetCompletedNotes()
    {
        return dbContext.Notes
            .Where(n => n.CategoryId == contextAccessor.GetCategory().Id && n.IsCompleted).ToList();
    }

    public void CleanupCompletedNotes()
    {
        var completedNotes = this.GetCompletedNotes();

        dbContext.Notes.RemoveRange(completedNotes);
        dbContext.SaveChanges();
    }

    public Category? GetCategory(int categoryId)
    {
        return dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
    }
}
