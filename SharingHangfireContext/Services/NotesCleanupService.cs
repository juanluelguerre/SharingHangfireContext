using Hangfire;
using SharingHangfireContext.Accessors;

namespace SharingHangfireContext.Services;

public class NotesCleanupService(
    IContextDataProvider contextDataProvider,
    NotesService notesService)
{
    [AutomaticRetry(Attempts = 0)]
    public void CleanupCompletedNotes(int categoryId)
    {
        contextDataProvider.SetCategoryId(categoryId);

        notesService.CleanupCompletedNotes();
    }
}
