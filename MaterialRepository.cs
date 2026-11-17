using FacultyArchive.Models;

namespace FacultyArchive.Repositories;

public class MaterialRepository
{
    private List<Material> Items = new List<Material>();

    public IReadOnlyList<Material> GetAll()
    {
        return Items;
    }

    public void Load(IEnumerable<Material> items)
    {
        Items = items.ToList();
    }

    public void Add(Material m)
    {
        Items.Add(m);
    }

    public void Update(Material m)
    {
        int index = Items.FindIndex(x => x.Id == m.Id);
        if (index >= 0)
            Items[index] = m;
    }

    public void Remove(Guid id)
    {
        Items.RemoveAll(x => x.Id == id);
    }

    // LINQ

    public IEnumerable<Material> SearchByAuthor(string text)
    {
        return Items.Where(m => m.AuthorFullName.ToLower().Contains(text.ToLower()));
    }

    public IEnumerable<Material> SearchByTitle(string text)
    {
        return Items.Where(m => m.Title.ToLower().Contains(text.ToLower()));
    }

    public IEnumerable<Material> SearchByFaculty(string text)
    {
        return Items.Where(m => m.Faculty.ToLower().Contains(text.ToLower()));
    }

    public IEnumerable<Material> SearchByDepartment(string text)
    {
        return Items.Where(m => m.Department.ToLower().Contains(text.ToLower()));
    }

    public IEnumerable<Material> SearchByType(MaterialType type)
    {
        return Items.Where(m => m.Type == type);
    }

    public IEnumerable<Material> SearchByVolume(int min, int max)
    {
        return Items.Where(m => m.Volume >= min && m.Volume <= max);
    }

    public IEnumerable<Material> SearchByDate(DateTime from, DateTime to)
    {
        return Items.Where(m => m.CreatedDate.Date >= from.Date && m.CreatedDate.Date <= to.Date);
    }
}
