using System.Collections.ObjectModel;
using System.Windows.Input;
using FacultyArchive.Models;
using FacultyArchive.Repositories;
using FacultyArchive.Services;
using FacultyArchive.Views;

namespace FacultyArchive.ViewModels;

public class MainViewModel : ObservableObject
{
    //Search criteria
    const string ByAuthor = "Автор";
    const string ByTitle = "Назва";
    const string ByFaculty = "Факультет";
    const string ByDepartment = "Кафедра";
    const string ByType = "Вид матеріалу";
    const string ByVolume = "Обсяг";
    const string ByDate = "Дата створення";

    private JsonFileService fileService = new JsonFileService();
    private MaterialRepository repository = new MaterialRepository();

    private Material? selectedMaterial;
    private string currentFilePath = "";
    private string selectedSearchCriterion = ByAuthor;
    private string searchText = "";
    private int volumeFrom = 0;
    private int volumeTo = 1000;
    private DateTime dateFrom = DateTime.Today.AddYears(-5);
    private DateTime dateTo = DateTime.Today;

    public MainViewModel()
    {
        OpenFileCommand = new Command(async () => await OpenFile());
        AddCommand = new Command(async () => await OpenEditor(null));
        EditCommand = new Command(async () => await Edit());
        DeleteCommand = new Command(async () => await Delete());
        SaveCommand = new Command(async () => await Save());
        SearchCommand = new Command(Search);
        ResetSearchCommand = new Command(ResetSearch);
        ShowAboutCommand = new Command(async () => await Shell.Current.Navigation.PushModalAsync(new AboutPage()));
    }

   
    public ObservableCollection<Material> Materials { get; } = new ObservableCollection<Material>();

    public string[] SearchCriteria { get; } = { ByAuthor, ByTitle, ByFaculty, ByDepartment, ByType, ByVolume, ByDate };
    public string[] MaterialTypeNames { get; } = Enum.GetNames(typeof(MaterialType));

    public Material? SelectedMaterial
    {
        get { return selectedMaterial; }
        set { selectedMaterial = value; OnPropertyChanged(nameof(SelectedMaterial)); }
    }

    public string CurrentFilePath
    {
        get { return currentFilePath; }
        set { currentFilePath = value; OnPropertyChanged(nameof(CurrentFilePath)); }
    }

    public string SelectedSearchCriterion
    {
        get { return selectedSearchCriterion; }
        set
        {
            selectedSearchCriterion = value;
            SearchText = "";
            OnPropertyChanged(nameof(SelectedSearchCriterion));
          
            OnPropertyChanged(nameof(IsTextCriterion));
            OnPropertyChanged(nameof(IsTypeCriterion));
            OnPropertyChanged(nameof(IsVolumeCriterion));
            OnPropertyChanged(nameof(IsDateCriterion));
        }
    }

    public string SearchText
    {
        get { return searchText; }
        set { searchText = value; OnPropertyChanged(nameof(SearchText)); }
    }

    public int VolumeFrom
    {
        get { return volumeFrom; }
        set { volumeFrom = value; OnPropertyChanged(nameof(VolumeFrom)); }
    }

    public int VolumeTo
    {
        get { return volumeTo; }
        set { volumeTo = value; OnPropertyChanged(nameof(VolumeTo)); }
    }

    public DateTime DateFrom
    {
        get { return dateFrom; }
        set { dateFrom = value; OnPropertyChanged(nameof(DateFrom)); }
    }

    public DateTime DateTo
    {
        get { return dateTo; }
        set { dateTo = value; OnPropertyChanged(nameof(DateTo)); }
    }

    public bool IsTextCriterion
    {
        get
        {
            return selectedSearchCriterion == ByAuthor || selectedSearchCriterion == ByTitle
                || selectedSearchCriterion == ByFaculty || selectedSearchCriterion == ByDepartment;
        }
    }
    public bool IsTypeCriterion { get { return selectedSearchCriterion == ByType; } }
    public bool IsVolumeCriterion { get { return selectedSearchCriterion == ByVolume; } }
    public bool IsDateCriterion { get { return selectedSearchCriterion == ByDate; } }


    public ICommand OpenFileCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ResetSearchCommand { get; }
    public ICommand ShowAboutCommand { get; }

    //Methods
    private async Task OpenFile()
    {
        try
        {
            string? path = await fileService.PickFileAsync();
            if (path == null)
                return; 

            List<Material> items = await fileService.LoadAsync(path);
            repository.Load(items);
            CurrentFilePath = path;
            Refresh(repository.GetAll());
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

 
    private async Task Save()
    {
        try
        {
            string path = CurrentFilePath;

            
            if (path == "")
            {
                path = await AskNewFilePath();
                if (path == "")
                    return;
            }

            await fileService.SaveAsync(path, repository.GetAll().ToList());
            CurrentFilePath = path;
            await Shell.Current.DisplayAlert("Збереження", "Дані збережено:\n" + path, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Помилка", ex.Message, "OK");
        }
    }

  
    private async Task<string> AskNewFilePath()
    {
        string? name = await Shell.Current.DisplayPromptAsync(
            "Новий файл", "Введіть назву файлу:", "Зберегти", "Скасувати", "materials");

        if (string.IsNullOrWhiteSpace(name))
            return "";

        name = Path.GetFileName(name);   
        if (!name.EndsWith(".json"))
            name = name + ".json";

        string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string path = Path.Combine(folder, name);


        if (File.Exists(path))
        {
            bool overwrite = await Shell.Current.DisplayAlert(
                "Файл існує", "Перезаписати " + name + "?", "Так", "Ні");
            if (!overwrite)
                return "";
        }

        return path;
    }

    private async Task Edit()
    {
        if (SelectedMaterial == null)
        {
            await Shell.Current.DisplayAlert("Увага", "Оберіть запис у таблиці.", "OK");
            return;
        }
        await OpenEditor(SelectedMaterial);
    }

  
    private async Task OpenEditor(Material? material)
    {
        var editViewModel = new MaterialEditViewModel(repository, material);

        editViewModel.Closed += async () =>
        {
            await Shell.Current.Navigation.PopModalAsync();
            Refresh(repository.GetAll());
        };

        await Shell.Current.Navigation.PushModalAsync(new MaterialEditPage(editViewModel));
    }

    private async Task Delete()
    {
        if (SelectedMaterial == null)
        {
            await Shell.Current.DisplayAlert("Увага", "Оберіть запис у таблиці.", "OK");
            return;
        }

        bool sure = await Shell.Current.DisplayAlert("Видалення",
            "Видалити «" + SelectedMaterial.Title + "»?", "Так", "Ні");
        if (!sure)
            return;

        repository.Remove(SelectedMaterial.Id);
        SelectedMaterial = null;
        Refresh(repository.GetAll());
    }

    private void Search()
    {
        IEnumerable<Material> result;

        if (selectedSearchCriterion == ByAuthor)
            result = repository.SearchByAuthor(SearchText);
        else if (selectedSearchCriterion == ByTitle)
            result = repository.SearchByTitle(SearchText);
        else if (selectedSearchCriterion == ByFaculty)
            result = repository.SearchByFaculty(SearchText);
        else if (selectedSearchCriterion == ByDepartment)
            result = repository.SearchByDepartment(SearchText);
        else if (selectedSearchCriterion == ByType)
        {
            if (Enum.TryParse(SearchText, out MaterialType type))
                result = repository.SearchByType(type);
            else
                result = new List<Material>();
        }
        else if (selectedSearchCriterion == ByVolume)
            result = repository.SearchByVolume(VolumeFrom, VolumeTo);
        else if (selectedSearchCriterion == ByDate)
            result = repository.SearchByDate(DateFrom, DateTo);
        else
            result = repository.GetAll();

        Refresh(result);
    }

    private void ResetSearch()
    {
        SearchText = "";
        VolumeFrom = 0;
        VolumeTo = 1000;
        DateFrom = DateTime.Today.AddYears(-5);
        DateTo = DateTime.Today;
        Refresh(repository.GetAll());
    }

    private void Refresh(IEnumerable<Material> source)
    {
        Materials.Clear();
        foreach (Material m in source)
            Materials.Add(m);
    }
}
