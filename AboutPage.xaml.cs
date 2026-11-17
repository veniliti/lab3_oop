namespace FacultyArchive.Views;

public partial class AboutPage : ContentPage
{
    // TODO: впишіть свої дані
    public string FullName { get; } = "Прізвище Ім'я По батькові";
    public int Course { get; } = 2;
    public string Group { get; } = "КН-00";
    public int Year { get; } = 2026;
    public string Description { get; } =
        "Програма для ведення електронного архіву факультету: перегляд, додавання, " +
        "редагування та видалення навчальних матеріалів, збереження у JSON-файл, " +
        "пошук за кількома критеріями (LINQ).";

    public AboutPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnCloseClicked(object? sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
