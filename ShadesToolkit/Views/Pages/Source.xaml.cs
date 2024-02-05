using Microsoft.Dism;
using Newtonsoft.Json;
using ShadesToolkit.ViewModels.Pages;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages
{
    public class WimFile
    {
        public bool IsMounting { get; set; }
        public string Filename { get; set; }
        public string OS { get; set; }
        public string Architecture { get; set; }
        public string Flags { get; set; }
        public int Index { get; set; }
        public string Size { get; set; }
        public string Status { get; set; }
        public CultureInfo Language { get; set; }
        public DateTime LastChanges { get; set; }
        public double Progress { get; set; }

        public Visibility IsInvalidStatus => Status == "Invalid" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsMountedStatus => Status == "Mounted" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProgressRingVisibility => IsMounting ? Visibility.Visible : Visibility.Collapsed;
    }

    public partial class Source : INavigableView<SourceViewModel>
    {
        public SourceViewModel ViewModel { get; }

        public Source(SourceViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
            LoadWimFilesAsync();
        }

        private void InitializeDismApi()
        {
            DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");
        }

        private void ShutdownDismApi()
        {
            DismApi.Shutdown();
        }

        private async Task<string> GetMountStatusAsync(string filename, int imageIndex)
        {
            InitializeDismApi();
            DismMountedImageInfoCollection mountedImageInfos = await Task.Run(() => DismApi.GetMountedImages());
            foreach (DismMountedImageInfo mountedImageInfo in mountedImageInfos)
            {
                if (mountedImageInfo.ImageFilePath == filename && mountedImageInfo.ImageIndex == imageIndex)
                {
                    switch (mountedImageInfo.MountStatus)
                    {
                        case DismMountStatus.Ok:
                            return "Mounted";
                        case DismMountStatus.NeedsRemount:
                            return "Needs Remount";
                        case DismMountStatus.Invalid:
                            return "Invalid";
                        default:
                            return "Unknown Status";
                    }
                }
            }

            string status = "Not Mounted";
            if (status == "Not Mounted")
            {
                sourceDataGrid.Visibility = Visibility.Hidden;
                CleanupMountsPanel.Visibility = Visibility.Visible;
                await CheckAndCleanMountpointsAsync();
                CleanupMountsPanel.Visibility = Visibility.Hidden;
                sourceDataGrid.Visibility = Visibility.Visible;
            }

            ShutdownDismApi();
            return status;
        }

        private async Task CheckAndCleanMountpointsAsync()
        {
            string mountPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mount");
            if (Directory.Exists(mountPath) && Directory.GetFileSystemEntries(mountPath).Length > 0)
            {
                await Task.Run(() => DismApi.CleanupMountpoints());
            }
        }

        private async void LoadWimFilesAsync()
        {
            if (File.Exists("wimFiles.json"))
            {
                string jsonContent = await File.ReadAllTextAsync("wimFiles.json");
                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    List<string> wimFiles = JsonConvert.DeserializeObject<List<string>>(jsonContent);

                    foreach (string filename in wimFiles)
                    {
                        await AddWimFileToDataGridAsync(filename);
                    }
                }
            }
        }

        private async void AddWim_ClickAsync(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".wim";
            dlg.Title = "Select Install.wim file!";
            dlg.Filter = "WIM Files (*.wim)|*.wim";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                if (Path.GetFileName(filename).ToLower() != "install.wim")
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = "Only the file 'install.wim' can be selected. Please select the correct file.";
                    await messageBox.ShowDialogAsync();
                    return;
                }

                // Check if a WIM file with the same name is already in the DataGrid
                foreach (WimFile item in sourceDataGrid.Items)
                {
                    if (Path.GetFileName(item.Filename) == Path.GetFileName(filename))
                    {
                        // If a WIM file with the same name is already in the DataGrid, show a message box and return
                        Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                        messageBox.Title = "Warning";
                        messageBox.Content = "Only one wim file can be selected. One wim file is already in the list.";
                        await messageBox.ShowDialogAsync();
                        return;
                    }
                }

                await AddWimFileToDataGridAsync(filename);

                List<string> wimFiles = new List<string>();
                if (File.Exists("wimFiles.json"))
                {
                    wimFiles = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync("wimFiles.json"));
                }

                if (!wimFiles.Contains(filename))
                {
                    wimFiles.Add(filename);
                    await File.WriteAllTextAsync("wimFiles.json", JsonConvert.SerializeObject(wimFiles));
                }
            }
        }

        private async Task AddWimFileToDataGridAsync(string filename)
        {
            InitializeDismApi();
            DismImageInfoCollection imageInfos = await Task.Run(() => DismApi.GetImageInfo(filename));

            foreach (DismImageInfo imageInfo in imageInfos)
            {
                double imageSizeInGB = imageInfo.ImageSize / 1024 / 1024 / 1024;
                imageSizeInGB = Math.Round(imageSizeInGB, 2);

                string status = await GetMountStatusAsync(filename, imageInfo.ImageIndex);

                WimFile item = new WimFile
                {
                    Filename = filename,
                    OS = imageInfo.ImageDescription,
                    Architecture = imageInfo.Architecture.ToString() == "AMD64" ? "x64" : imageInfo.Architecture.ToString(),
                    Flags = imageInfo.EditionId.ToString() == "Core" ? "Home" : imageInfo.EditionId.ToString(),
                    Index = imageInfo.ImageIndex,
                    Size = imageSizeInGB + " GB",
                    Status = status,
                    Language = imageInfo.DefaultLanguage,
                    LastChanges = imageInfo.CustomizedInfo.ModifiedTime
                };

                if (!sourceDataGrid.Items.Contains(item))
                {
                    sourceDataGrid.Items.Add(item);
                }
            }
            ShutdownDismApi();
        }

        private void Add_Btn_Click(object sender, RoutedEventArgs e)
        {
            contextMenu.PlacementTarget = sender as Wpf.Ui.Controls.Button;
            contextMenu.IsOpen = true;
        }
        private async void MountUnmount_ClickAsync(object sender, RoutedEventArgs e, bool isMount)
        {
            if (sourceDataGrid.SelectedItem is not null)
            {
                contextMenuControl.IsEnabled = false;
                contextMenuControl.Visibility = Visibility.Hidden;
                var selectedItem = (WimFile)sourceDataGrid.SelectedItem;
                selectedItem.Status = "Starting..";
                sourceDataGrid.Items.Refresh();
                string filename = selectedItem.Filename;
                string mountPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mount");

                InitializeDismApi();
                add_btn.IsEnabled = false;
                var worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += (s, args) =>
                {
                    DismProgressCallback progressCallback = new DismProgressCallback((DismProgress progress) =>
                    {
                        worker.ReportProgress(isMount ? progress.Current : 100 - progress.Current);
                    });

                    selectedItem.IsMounting = true;
                    if (isMount)
                    {
                        DismApi.MountImage(filename, mountPath, selectedItem.Index, false, DismMountImageOptions.None, progressCallback);
                    }
                    else
                    {
                        DismApi.UnmountImage(mountPath, false, progressCallback);
                    }
                };
                worker.ProgressChanged += (s, args) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        selectedItem.Status = $"{(isMount ? "Mounting" : "Unmounting")}... {args.ProgressPercentage}%";
                        selectedItem.Progress = args.ProgressPercentage;
                        sourceDataGrid.Items.Refresh();
                    });
                };
                worker.RunWorkerCompleted += async (s, args) =>
                {
                    ShutdownDismApi();
                    selectedItem.IsMounting = false;
                    selectedItem.Status = await GetMountStatusAsync(selectedItem.Filename, selectedItem.Index);
                    sourceDataGrid.Items.Refresh();
                    contextMenuControl.Visibility = Visibility.Visible;
                    contextMenuControl.IsEnabled = true;
                    add_btn.IsEnabled = true;
                };
                worker.RunWorkerAsync();
            }
        }
        private async void Mount_ClickAsync(object sender, RoutedEventArgs e) => MountUnmount_ClickAsync(sender, e, true);

        private async void Unmount_ClickAsync(object sender, RoutedEventArgs e) => MountUnmount_ClickAsync(sender, e, false);

        private void sourceDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var source = e.OriginalSource as FrameworkElement;
            if (source.DataContext is WimFile)
            {
                // Kullanıcı bir satırın üzerinde, bu yüzden ContextMenu'yu göster
                sourceDataGrid.ContextMenu.IsOpen = true;
            }
            else
            {
                // Kullanıcı bir başlık üzerinde, bu yüzden ContextMenu'yu gösterme
                e.Handled = true;
            }
        }

        private async void AddIso_ClickAsync(object sender, RoutedEventArgs e)
        {
            // DataGrid'de zaten gösterilen bir veri olup olmadığını kontrol et
            if (sourceDataGrid.Items.Count > 0)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Warning";
                messageBox.Content = "Already showing a wim information";
                await messageBox.ShowDialogAsync();
                return;
            }

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".iso";
            dlg.Title = "Select ISO file!";
            dlg.Filter = "ISO Files (*.iso)|*.iso";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                // ISO dosyasını çıkartma işlemi
                string extractPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Extracted");
                Directory.CreateDirectory(extractPath);
                add_btn.IsEnabled = false;
                sourceDataGrid.Visibility = Visibility.Hidden;
                CleanupMountsPanel.Visibility = Visibility.Visible;
                DataText.Text = "Extracting..";
                // Çıkartma işlemi için bir ProcessStartInfo nesnesi oluştur
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"& {{ $diskImage = Mount-DiskImage '{filename}' -PassThru; $volume = Get-Volume -DiskImage $diskImage; Copy-Item -Path ($volume.DriveLetter + ':\\*') -Destination '{extractPath}' -Recurse -Force; Dismount-DiskImage -ImagePath '{filename}' }}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                // Process nesnesi oluştur ve başlat
                Process process = new Process { StartInfo = startInfo };
                process.Start();

                // Çıkartma işlemi tamamlanana kadar bekleyin
                await process.WaitForExitAsync();

                // Çıkarılan dosyaların listesini al
                string wimFile = Path.Combine(extractPath, "sources", "install.wim");

                if (File.Exists(wimFile))
                {
                    // WIM dosyasını DataGrid'e ekle
                    await AddWimFileToDataGridAsync(wimFile);

                    List<string> wimFiles = new List<string>();
                    if (File.Exists("wimFiles.json"))
                    {
                        wimFiles = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync("wimFiles.json"));
                    }

                    if (!wimFiles.Contains(wimFile))
                    {
                        wimFiles.Add(wimFile);
                        await File.WriteAllTextAsync("wimFiles.json", JsonConvert.SerializeObject(wimFiles));
                    }
                }
                add_btn.IsEnabled = true;
                sourceDataGrid.Visibility = Visibility.Visible;
                CleanupMountsPanel.Visibility = Visibility.Hidden;
                DataText.Text = "Cleaning in progress, please wait..";
            }
        }

        private async void Forget_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (sourceDataGrid.SelectedItem is not null)
            {
                var selectedItem = (WimFile)sourceDataGrid.SelectedItem;
                if (selectedItem.Status == "Not Mounted")
                {
                    // WIM bilgilerini temizle
                    sourceDataGrid.Items.Remove(selectedItem);

                    // JSON dosyasını güncelle
                    List<string> wimFiles = new List<string>();
                    if (File.Exists("wimFiles.json"))
                    {
                        wimFiles = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync("wimFiles.json"));
                    }

                    if (wimFiles.Contains(selectedItem.Filename))
                    {
                        wimFiles.Remove(selectedItem.Filename);
                        await File.WriteAllTextAsync("wimFiles.json", JsonConvert.SerializeObject(wimFiles));
                    }

                    // DataGrid'i yenile
                    sourceDataGrid.Items.Refresh();
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = "You need to unmount first to remove it from the list.";
                    await messageBox.ShowDialogAsync();
                }
            }
        }


    }
}