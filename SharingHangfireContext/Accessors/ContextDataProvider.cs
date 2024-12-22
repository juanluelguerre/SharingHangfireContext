using SharingHangfireContext.Entities;
using SharingHangfireContext.Services;

namespace SharingHangfireContext.Accessors;

public class ContextDataProvider(NotesDbContext dbContext) : IContextDataProvider
{
    private int? categoryId;
    private Category? category;

    public Category GetCategory()
    {
        if (this.category is not null) return this.category;

        if (this.categoryId is null) throw new Exception("Category not found");

        var category = dbContext.Categories.FirstOrDefault(c => c.Id == this.categoryId);
        this.category = category ?? throw new Exception("Category not found");

        return this.category;
    }

    public void SetCategoryId(int categoryId)
    {
        this.categoryId = categoryId;
    }
}
