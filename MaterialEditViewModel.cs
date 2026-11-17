using System.Windows.Input;
using FacultyArchive.Models;
using FacultyArchive.Repositories;

namespace FacultyArchive.ViewModels;

public class MaterialEditViewModel : ObservableObject
{
    private MaterialRepository repository;

    public MaterialEditViewModel(MaterialRepository repository, Material? material)
    {
        this.repository = repository;

        if (material == null)
        {
            IsNew = true;
            Material = new Material();   //add 
        }
        else
        {
            IsNew = false;
            Material = material.Clone(); // edit
        }

        SaveCommand = new Command(async () => await Save());
        CancelCommand = new Command(CloseForm);
    }

    public Material Material { get; }
    public bool IsNew { get; }
    public Array MaterialTypes { get; } = Enum.GetValues(typeof(MaterialType));

    public string Header
    {
        get
        {
            if (IsNew)
                return "Новий матеріал";
            return "Редагування матеріалу";
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }


    public event Action? Closed;

    private void CloseForm()
    {
        if (Closed != null)
            Closed();
    }

    private async Task Save()
    {
        bool ok = await Validate();
        if (ok == false)
            return; 

        if (IsNew)
            repository.Add(Material);
        else
            repository.Update(Material);

        CloseForm();
    }


    private async Task<bool> Validate()
    {
        if (string.IsNullOrWhiteSpace(Material.AuthorFullName) ||
            string.IsNullOrWhiteSpace(Material.Title) ||
            string.IsNullOrWhiteSpace(Material.Faculty) ||
            string.IsNullOrWhiteSpace(Material.Department))
        {
            await Shell.Current.DisplayAlert("Помилка", "Заповніть усі текстові поля.", "OK");
            return false;
        }

        if (IsFullName(Material.AuthorFullName) == false)
        {
            await Shell.Current.DisplayAlert("Помилка",
                "П.І.П. має містити лише літери, пробіли, дефіс, апостроф і крапку.", "OK");
            return false;
        }

        if (HasLetter(Material.Title) == false ||
            HasLetter(Material.Faculty) == false ||
            HasLetter(Material.Department) == false)
        {
            await Shell.Current.DisplayAlert("Помилка",
                "Назва, факультет і кафедра мають містити текст, а не лише цифри чи символи.", "OK");
            return false;
        }

        if (Material.Volume <= 0)
        {
            await Shell.Current.DisplayAlert("Помилка", "Обсяг має бути більшим за 0.", "OK");
            return false;
        }

        return true;
    }

    private bool IsFullName(string text)
    {
        foreach (char c in text)
        {
            bool allowed = char.IsLetter(c) || c == ' ' || c == '-' || c == '\'' || c == '.';
            if (allowed == false)
                return false;
        }
        return true;
    }

    private bool HasLetter(string text)
    {
        foreach (char c in text)
        {
            if (char.IsLetter(c))
                return true;
        }
        return false;
    }
}
