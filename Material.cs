namespace FacultyArchive.Models;

public class Material
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AuthorFullName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Faculty { get; set; } = "";
    public string Department { get; set; } = "";
    public MaterialType Type { get; set; } = MaterialType.Textbook;
    public int Volume { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Today;

    // Copy for editing
    public Material Clone()
    {
        return new Material
        {
            Id = Id,
            AuthorFullName = AuthorFullName,
            Title = Title,
            Faculty = Faculty,
            Department = Department,
            Type = Type,
            Volume = Volume,
            CreatedDate = CreatedDate
        };
    }
}
