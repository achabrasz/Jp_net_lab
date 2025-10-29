using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Contracts;
using Prism.Events;
using Timer = System.Timers.Timer;

namespace DashboardApp;

public partial class MainPage : ContentPage
{
    private CompositionContainer _container;
    private IEventAggregator _eventAggregator;
    private FileSystemWatcher _watcher;
    private Timer _pollTimer;
    private HashSet<string> _lastDlls = new();

    public MainPage()
    {
        InitializeComponent();
        CopyWidgetsToAppData();
        SetupMef();
        LoadWidgets();
        WatchPluginsFolder();
    }

    private void SetupMef()
    {
        Console.WriteLine($"AppDataDirectory: {FileSystem.Current.AppDataDirectory}");
        Console.WriteLine($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");

        var catalog = new AggregateCatalog();
        var path = Path.Combine(FileSystem.Current.AppDataDirectory, "Widgets");

        Directory.CreateDirectory(path);
        catalog.Catalogs.Add(new DirectoryCatalog(path));

        _container = new CompositionContainer(catalog);
        _eventAggregator = new EventAggregator();
        _container.ComposeExportedValue(_eventAggregator);
    }

    private void CopyWidgetsToAppData()
    {
        // The source is the output folder of your built app
        var sourcePath = Path.Combine(AppContext.BaseDirectory, "Widgets");
        var destPath = Path.Combine(FileSystem.Current.AppDataDirectory, "Widgets");

        try
        {
            Directory.CreateDirectory(destPath);

            if (!Directory.Exists(sourcePath))
            {
                Console.WriteLine($"[WARN] Source path not found: {sourcePath}");
                return;
            }

            foreach (var file in Directory.GetFiles(sourcePath, "*.dll", SearchOption.AllDirectories))
            {
                var destFile = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                Console.WriteLine($"Copied {file} → {destFile}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] CopyWidgetsToAppData: {ex}");
        }
    }
    
    private void LoadWidgets()
    {
        SetupMef();
        var widgets = _container.GetExports<IWidget>();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            WidgetsContainer.Children.Clear();
            foreach (var widget in widgets)
                WidgetsContainer.Children.Add(widget.Value.View);
        });
    }

    private void WatchPluginsFolder()
    {
        var path = Path.Combine(FileSystem.Current.AppDataDirectory, "Widgets");
        _watcher = new FileSystemWatcher(path, "*.dll")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        _watcher.Created += (s, e) => MainThread.BeginInvokeOnMainThread(LoadWidgets);
        _watcher.Deleted += (s, e) => MainThread.BeginInvokeOnMainThread(LoadWidgets);
        _watcher.Changed += (s, e) => MainThread.BeginInvokeOnMainThread(LoadWidgets);
        _watcher.Renamed += (s, e) => MainThread.BeginInvokeOnMainThread(LoadWidgets);

        _watcher.EnableRaisingEvents = true;
    }
    
    private void SendData_Clicked(object sender, EventArgs e)
    {
        _eventAggregator.GetEvent<DataSubmittedEvent>().Publish(DataEntry.Text ?? "");
    }
}