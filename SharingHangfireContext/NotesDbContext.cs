using Microsoft.EntityFrameworkCore;
using SharingHangfireContext.Entities;

namespace SharingHangfireContext;

public class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes { get; set; }
    public DbSet<Category> Categories { get; set; }
}
