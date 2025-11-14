using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CyclingIntervalsGui.ViewModels;

namespace CyclingIntervalsGui.Views;

/// <summary>
/// Vista principal de la aplicación.
/// Nota: Esta clase contiene SOLO la inicialización de Avalonia.
/// Toda la lógica se encuentra en MainWindowViewModel a través de bindings MVVM.
/// NO hay lógica de negocio aquí.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
