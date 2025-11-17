using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace CyclingIntervalsGui.Services;

public class FileManager
{
    private readonly Window _mainWindow;

    public FileManager(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task<string?> PickFileAsync()
    {
        var provider = _mainWindow.StorageProvider;
        var config = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Selecciona un archivo .fit",
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Archivos FIT")
                {
                    Patterns = new[] { "*.fit" }
                }
            }
        };

        var result = await provider.OpenFilePickerAsync(config);

        var folder = result.FirstOrDefault();
        if (folder is null)
            return null;

        string? path = folder.TryGetLocalPath();

        return path;
    }
}