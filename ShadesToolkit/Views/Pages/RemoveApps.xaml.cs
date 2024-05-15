using Microsoft.Dism;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;
using ShadesToolkit.ViewModels.Pages;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages
{
    public class AppxPackage : INotifyPropertyChanged
    {
        public string AppName { get; set; }
        public string DisplayName { get; set; }
        public string Version { get; set; }
        public string Type { get; set; } // "Appx" 

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public partial class RemoveApps : INavigableView<RemoveAppsViewModel>
    {
        public RemoveAppsViewModel ViewModel { get; }
        private string nsudoLCPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "NSudoLC.exe");
        private string mountPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mount");

        private List<string> excludedAppNames = new List<string>() { "Microsoft.DesktopAppInstaller", "Microsoft.VCLibs.140.00", "Microsoft.XboxSpeechToTextOverlay" };

        public RemoveApps(RemoveAppsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
            Loaded += RemoveApps_Loaded;
        }

        private bool _appxPackagesLoaded = false; // Flag to track if packages have been loaded

        private async void RemoveApps_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_appxPackagesLoaded)
            {
                await LoadAppxPackages();
                _appxPackagesLoaded = true; // Set the flag to prevent reloading
            }
        }
        private Dictionary<string, string> GetAppNamesFromResource()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("ShadesToolkit.Helpers.ProvisionedApps.json"))
                {
                    if (stream == null)
                    {
                        return new Dictionary<string, string>();
                    }

                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    }
                }
            }
            catch { return new Dictionary<string, string>(); }
        }

        private async Task LoadAppxPackages()
        {
            try
            {
                RemoveAppsButtons.Visibility = Visibility.Collapsed;
                ProgressRing.Visibility = Visibility.Visible;
                AppxPackagesListView.Items.Clear();

                await Task.Run(() =>
                {
                    DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");

                    var appNames = GetAppNamesFromResource();

                    using (DismSession session = DismApi.OpenOfflineSession(mountPath))
                    {
                        DismAppxPackageCollection packages = DismApi.GetProvisionedAppxPackages(session);

                        foreach (DismAppxPackage package in packages)
                        {
                            string appName = appNames.ContainsKey(package.DisplayName) ? appNames[package.DisplayName] : package.DisplayName;

                            // Öğenin ListView'de zaten olup olmadığını kontrol edin
                            bool itemExists = AppxPackagesListView.Items.Cast<AppxPackage>().Any(item => item.DisplayName == package.DisplayName);

                            if (!itemExists)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    AppxPackagesListView.Items.Add(new AppxPackage
                                    {
                                        AppName = appName,
                                        DisplayName = package.DisplayName,
                                        Version = package.Version.ToString(),
                                        Type = "Appx",
                                        IsChecked = false,
                                        IsEnabled = !excludedAppNames.Contains(package.DisplayName)
                                    });
                                });
                            }
                        }
                    }
                    DismApi.Shutdown();
                });

            }
            catch (Exception Ex)
            {
                // Show general error message using Wpf.Ui.Controls.MessageBox
                Wpf.Ui.Controls.MessageBox errorMessageBox = new Wpf.Ui.Controls.MessageBox();
                errorMessageBox.Title = "Error";
                errorMessageBox.Content = $"{Ex.Message}";
                await errorMessageBox.ShowDialogAsync();

                RemoveAppsButtons.Visibility = Visibility.Visible;
                ProgressRing.Visibility = Visibility.Collapsed;
            }
            finally
            {
                RemoveAppsButtons.Visibility = Visibility.Visible;
                ProgressRing.Visibility = Visibility.Collapsed;
                AppxPackagesListView.Visibility = Visibility.Visible;
            }
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadAppxPackages();
        }

        private async void Remove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if any toggleswitch is checked
                bool anyChecked = AppxPackagesListView.Items.Cast<AppxPackage>().Any(item => item.IsChecked);

                if (!anyChecked)
                {
                    // Show a message if no items are selected
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = "If you have not selected any apps, turn on the apps you want to uninstall.";
                    await messageBox.ShowDialogAsync();
                    return;
                }

                var selectedItems = AppxPackagesListView.Items.Cast<AppxPackage>().Where(item => item.IsChecked).ToList();

                DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");

                using (DismSession session = DismApi.OpenOfflineSession(mountPath))
                {
                    foreach (var item in selectedItems)
                    {
                        if (item.Type == "Appx")
                        {
                            try
                            {
                                await Task.Run(() =>
                                {
                                    var package = DismApi.GetProvisionedAppxPackages(session).FirstOrDefault(p => p.DisplayName == item.DisplayName);
                                    if (package != null)
                                    {
                                        DismApi.RemoveProvisionedAppxPackage(session, package.PackageName);
                                    }
                                });
                                Dispatcher.Invoke(() => AppxPackagesListView.Items.Remove(item));
                            }
                            catch (Exception ex)
                            {
                                // Show error message using Wpf.Ui.Controls.MessageBox
                                Wpf.Ui.Controls.MessageBox errorMessageBox = new Wpf.Ui.Controls.MessageBox();
                                errorMessageBox.Title = "Error";
                                errorMessageBox.Content = $"Error removing app '{item.DisplayName}': {ex.Message}";
                                await errorMessageBox.ShowDialogAsync();
                            }
                        }
                    }
                }

                DismApi.Shutdown();

                foreach (AppxPackage item in AppxPackagesListView.Items)
                {
                    item.IsChecked = false;
                }
                MasterToggleSwitch.IsChecked = false;
            }
            catch (Exception ex)
            {
                // Show general error message using Wpf.Ui.Controls.MessageBox
                Wpf.Ui.Controls.MessageBox errorMessageBox = new Wpf.Ui.Controls.MessageBox();
                errorMessageBox.Title = "Error";
                errorMessageBox.Content = $"An error occurred while removing apps: {ex.Message}";
                await errorMessageBox.ShowDialogAsync();
            }
        }

        private async void AddApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                // Tüm paket türlerini ekle
                openFileDialog.Filter = "App Packages (.appx)|*.appx;";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string packagePath = openFileDialog.FileName;

                    // .appx, .msix, .appxbundle gibi paketleri eklemek için DismApi.AddProvisionedAppxPackage kullanın
                    // (Bu örnekte ekleme no licence olarak yapılır)
                    DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");

                    using (DismSession session = DismApi.OpenOfflineSession(mountPath))
                    {
                        // Gerekli bağımlılık paketleri ve lisans bilgilerini burada belirtin
                        List<string>? dependencyPackages = null;
                        string? licensePath = null;
                        string customDataPath = null;

                        // .appx, .msix, .appxbundle gibi paketleri ekleyin
                        DismApi.AddProvisionedAppxPackage(session, packagePath, dependencyPackages, licensePath, customDataPath);

                    }

                    DismApi.Shutdown();

                    // Uygulamanın başarıyla eklendiğini bildirin
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Success";
                    messageBox.Content = "Application added successfully.";
                    await messageBox.ShowDialogAsync();
                }
            }
            catch (Exception ex)
            {
                // Hataları işleyin
                Wpf.Ui.Controls.MessageBox errorMessageBox = new Wpf.Ui.Controls.MessageBox();
                errorMessageBox.Title = "Error";
                errorMessageBox.Content = $"An error occurred: {ex.Message}";
                await errorMessageBox.ShowDialogAsync();
            }
        }
        private void MasterToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (AppxPackage item in AppxPackagesListView.Items)
                {
                    if (!excludedAppNames.Contains(item.DisplayName))
                    {
                        item.IsChecked = true;
                    }
                }
            }
            catch { }
        }

        private void MasterToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (AppxPackage item in AppxPackagesListView.Items)
                {
                    if (!excludedAppNames.Contains(item.DisplayName))
                    {
                        item.IsChecked = false;
                    }
                }
            }
            catch { }
        }
    }
}