using Hangfire;
using Microsoft.AspNetCore.Mvc;
using SharingHangfireContext.Entities;
using SharingHangfireContext.Services;

namespace SharingHangfireContext.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotesController(NotesService notesService) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Note>> GetNotes([FromHeader] int categoryId)
    {
        return notesService.GetCompletedNotes();
    }

    [HttpPost("run-cleanup-task")]
    public IActionResult RunCleanupTask([FromHeader] int categoryId)
    {
        BackgroundJob.Enqueue<NotesCleanupService>(
            service => service.CleanupCompletedNotes(categoryId));
        return this.Ok("Cleanup task triggered");
    }
}
