using Avalonia.Controls;
using CentralBankRate.UI.ViewModels;

namespace CentralBankRate.UI.Views;

public partial class MainWindow : Window
{
    // Нужен дизайнеру и XAML-загрузчику
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
