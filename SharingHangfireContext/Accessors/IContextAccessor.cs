using SharingHangfireContext.Entities;

namespace SharingHangfireContext.Accessors;

public interface IContextAccessor
{
    Category GetCategory();
}
