namespace SharingHangfireContext.Accessors;

public interface IContextDataProvider : IContextAccessor
{
    void SetCategoryId(int categoryId);
}
