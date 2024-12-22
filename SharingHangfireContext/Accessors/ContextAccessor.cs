using SharingHangfireContext.Entities;

namespace SharingHangfireContext.Accessors;

public class ContextAccessor(IHttpContextAccessor httpContextAccessor, NotesDbContext dbContext)
    : IContextAccessor
{
    private Category? category;

    public Category GetCategory()
    {
        if (this.category is not null) return this.category;

        if (!Int32.TryParse(
                (string?)httpContextAccessor.HttpContext?.Request.Headers["CategoryId"],
                out var categoryId))
            throw new Exception("Category not found");

        var category = dbContext.Categories.FirstOrDefault(c => c.Id == categoryId);
        this.category = category ?? throw new Exception("Category not found");

        return this.category;
    }
}
