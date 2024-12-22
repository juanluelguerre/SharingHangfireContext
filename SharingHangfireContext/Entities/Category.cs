using Microsoft.AspNetCore.Http.HttpResults;

namespace SharingHangfireContext.Entities;

public class Category
{
    protected Category()
    {
    }

    private Category(int id, string name, Note[] notes)
    {
        this.Id = id;
        this.Name = name;
        this.Notes = notes;
    }

    public static Category Create(int id, string name, Note[] notes)
    {
        return new Category(id, name, notes);
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public IEnumerable<Note> Notes { get; private set; }
}
