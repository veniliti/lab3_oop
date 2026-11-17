using FacultyArchive.ViewModels;

namespace FacultyArchive.Views;

public partial class MaterialEditPage : ContentPage
{
    public MaterialEditPage(MaterialEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
