using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FacultyArchive.Models;

namespace FacultyArchive.Services;

public class JsonFileService
{
    
    private JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true,                                  
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, 
        Converters = { new JsonStringEnumConverter() }         
    };

    public async Task<string?> PickFileAsync()
    {
        var jsonType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.iOS, new[] { "public.json" } },
            { DevicePlatform.MacCatalyst, new[] { "public.json" } }
        });

        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Оберіть JSON-файл",
            FileTypes = jsonType
        });

        if (result == null)
            return null;
        return result.FullPath;
    }

    public async Task<List<Material>> LoadAsync(string path)
    {
        string json = await File.ReadAllTextAsync(path);
        return Deserialize(json);
    }

    public async Task SaveAsync(string path, List<Material> items)
    {
        string json = Serialize(items);
        await File.WriteAllTextAsync(path, json);
    }

    private string Serialize(List<Material> items)
    {
        return JsonSerializer.Serialize(items, options);
    }

    private List<Material> Deserialize(string json)
    {
        var list = JsonSerializer.Deserialize<List<Material>>(json, options);
        if (list == null)
            return new List<Material>();
        return list;
    }
}
