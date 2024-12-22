namespace SharingHangfireContext.Entities;

public class Note
{
    protected Note()
    {
    }

    private Note(int id, string name, int categoryId)
    {
        this.Id = id;
        this.Name = name;
        this.CategoryId = categoryId;
    }

    public static Note Create(int id, string name, int categoryId)
    {
        return new Note(id, name, categoryId);
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsCompleted { get; private set; }
}
