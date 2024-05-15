using ManagedWimLib;
using Microsoft.Dism;
using Newtonsoft.Json;
using ShadesToolkit.Helpers;
using ShadesToolkit.ViewModels.Pages;
using ShadesToolkit.Views.Pages.ImageInfo;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Microsoft.Win32;

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
        public string Language { get; set; }
        public string LastChanges { get; set; }
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
            //LoadWimFilesAsync();
            _ = LoadWimFilesOrGetMountedWimPathAsync();
            ImageName.RefreshDataGridEvent += RefreshDataGrid;
            ImageDescription.RefreshDataGridEvent += RefreshDataGrid;
            ImageFlags.RefreshDataGridEvent += RefreshDataGrid;

        }

        private async Task LoadWimFilesOrGetMountedWimPathAsync()
        {
            // 1. JSON dosyasını yükleyin ve WIM bilgilerini ListView'e ekleyin
            if (File.Exists("wimFiles.json") && !string.IsNullOrEmpty(File.ReadAllText("wimFiles.json")))
            {
                LoadWimFilesAsync();
                return; // JSON dosyası mevcut ve doluysa, işlemi sonlandırın.
            }

            // 2. Mount edilen sistemi kontrol edin (DismAPI kullanarak)
            string mountPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mount");

            DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");

            try
            {
                DismMountedImageInfoCollection mountedImages = await Task.Run(() => DismApi.GetMountedImages());

                foreach (DismMountedImageInfo mountedImage in mountedImages)
                {
                    // 3. Mount edilen sistemin yolunu JSON'a kaydedin
                    await SaveWimPathToJSON(mountedImage.ImageFilePath);

                    // 4. WIM bilgilerini ListView'e ekleyin
                    await AddWimFileToDataGridAsync(mountedImage.ImageFilePath);
                }
            }
            finally
            {
                DismApi.Shutdown();
            }

        }

        private async Task SaveWimPathToJSON(string wimPath)
        {
            List<string> wimFiles = new List<string> { wimPath };
            await File.WriteAllTextAsync("wimFiles.json", JsonConvert.SerializeObject(wimFiles));
        }
        private void RefreshDataGrid()
        {
            sourceListView.Items.Clear();
            LoadWimFilesAsync();
        }
        public void SetControlsEnabled()
        {
            add_btn.IsEnabled = true;
            Refresh_Btn.IsEnabled = true;
            ISO_btn.IsEnabled = true;
            convert_btn.IsEnabled = true;
            host_btn.IsEnabled = true;
            iso_download_btn.IsEnabled = true;
        }
        public void SetControlsDisabled()
        {
            add_btn.IsEnabled = false;
            Refresh_Btn.IsEnabled = false;
            ISO_btn.IsEnabled = false;
            convert_btn.IsEnabled = false;
            host_btn.IsEnabled = false;
            iso_download_btn.IsEnabled = false;
        }

        private async Task<string> GetMountStatusAsync(string filename, int imageIndex)
        {

            DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");
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
                            string mountPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mount");
                            SetControlsDisabled();
                            sourceListView.Visibility = Visibility.Hidden;
                            CleanupMountsPanel.Visibility = Visibility.Visible;
                            DataText.Text = "Remounting..";
                            await Task.Run(() => DismApi.RemountImage(mountPath));
                            SetControlsEnabled();
                            sourceListView.Visibility = Visibility.Visible;
                            CleanupMountsPanel.Visibility = Visibility.Hidden;
                            return "Mounted";
                        case DismMountStatus.Invalid:
                            return "Invalid";
                        default:
                            return "Unknown Status";
                    }
                }
            }

            string status = "Not Mounted";
            DismApi.Shutdown();
            return status;
        }
        public static void InitNativeLibrary()
        {
            string arch = null;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    arch = "x86";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                case Architecture.Arm:
                    arch = "armhf";
                    break;
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
            }
            string libPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "WimLib", arch, "libwim-15.dll");

            if (!File.Exists(libPath))
                throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");
            Wim.GlobalInit(libPath, InitFlags.None);
        }
        public async void LoadWimFilesAsync()
        {
            if (File.Exists("wimFiles.json"))
            {
                string jsonContent = await File.ReadAllTextAsync("wimFiles.json");
                if (!string.IsNullOrWhiteSpace(jsonContent))
                {
                    List<string> wimFiles = JsonConvert.DeserializeObject<List<string>>(jsonContent);

                    foreach (string filename in wimFiles)
                    {
                        try
                        {
                            await AddWimFileToDataGridAsync(filename);
                        }
                        catch (FileNotFoundException) { }
                    }
                }
            }
        }
        private async void Refresh_Btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Remove all items from the ListView
                sourceListView.Items.Clear();

                // Reload the WIM files from the directory
                RefreshDataGrid();
            }
            catch (Exception ex)
            {
                // Hata oluşursa, burada hata mesajı gösterin
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Hata";
                messageBox.Content = ex.Message;
                await messageBox.ShowDialogAsync();
            }
        }
        private async void AddWim_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (sourceListView.Items.Count > 0)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Warning";
                messageBox.Content = "Already showing a wim information";
                await messageBox.ShowDialogAsync();
                return;
            }
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

                foreach (WimFile item in sourceListView.Items)
                {
                    if (Path.GetFileName(item.Filename) == Path.GetFileName(filename))
                    {
                        Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                        messageBox.Title = "Warning";
                        messageBox.Content = "Only one wim file can be selected. One wim file is already in the list.";
                        await messageBox.ShowDialogAsync();
                        return;
                    }
                }

                await AddWimFileToDataGridAsync(filename);

                List<string> wimFiles = new List<string> { filename };
                await File.WriteAllTextAsync("wimFiles.json", JsonConvert.SerializeObject(wimFiles));
            }
        }


        private async Task AddWimFileToDataGridAsync(string filename)
        {
            string mountPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mount");

            if (!Directory.Exists(mountPath))
            {
                Directory.CreateDirectory(mountPath);
            }
            DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");
            if (File.Exists(filename))
            {
                try
                {
                    DismImageInfoCollection imageInfos = await Task.Run(() => DismApi.GetImageInfo(filename));

                    foreach (DismImageInfo imageInfo in imageInfos)
                    {
                        double imageSizeInGB = (double)imageInfo.ImageSize / 1024 / 1024 / 1024;

                        imageSizeInGB = Math.Round(imageSizeInGB, 2);

                        double imageSizeInMB = imageInfo.ImageSize / 1024 / 1024;
                        imageSizeInMB = Math.Round(imageSizeInMB, 2);

                        string status = await GetMountStatusAsync(filename, imageInfo.ImageIndex);

                        WimFile item = new WimFile
                        {
                            Filename = filename,
                            OS = imageInfo.ImageName,
                            Architecture = imageInfo.Architecture.ToString() == "AMD64" ? "x64" : imageInfo.Architecture.ToString(),
                            Flags = imageInfo.EditionId.ToString(),
                            Index = imageInfo.ImageIndex,
                            Size = $"{imageSizeInGB} GB ({imageSizeInMB} MB)",
                            Status = status,
                            Language = imageInfo.DefaultLanguage.DisplayName,
                            LastChanges = imageInfo.CustomizedInfo.ModifiedTime.ToLongDateString(),
                        };

                        if (!sourceListView.Items.Contains(item))
                        {
                            sourceListView.Items.Add(item);
                        }
                    }
                }
                catch (FileNotFoundException) { }
            }
            else { }
            DismApi.Shutdown();
        }


        private void Add_Btn_Click(object sender, RoutedEventArgs e)
        {
            contextMenu.PlacementTarget = sender as Wpf.Ui.Controls.Button;
            contextMenu.IsOpen = true;
        }
        private async Task MountUnmountAsync(bool isMount)
        {
            DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");
            if (sourceListView.SelectedItem is not null)
            {
                var selectedItem = (WimFile)sourceListView.SelectedItem;
                string filename = selectedItem.Filename;
                int imageIndex = selectedItem.Index;

                // Check if any index of this WIM is already mounted
                bool isAnyIndexMounted = await IsAnyIndexMountedAsync(filename);

                if (isMount && isAnyIndexMounted)
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = $"You already have a mounted system, please unmount it first and then you can mount it again.";
                    await messageBox.ShowDialogAsync();
                    DismApi.Shutdown();
                    return;
                }

                string currentStatus = await GetMountStatusAsync(filename, imageIndex);

                if (isMount && currentStatus == "Mounted")
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = $"Index {imageIndex} of {filename} is already mounted.";
                    await messageBox.ShowDialogAsync();
                    DismApi.Shutdown();
                    return;
                }

                if (!isMount && currentStatus == "Not Mounted")
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = $"Index {imageIndex} of {filename} is already unmounted.";
                    await messageBox.ShowDialogAsync();
                    DismApi.Shutdown();
                    return;
                }

                string sevenZipExePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools", "7z.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = sevenZipExePath,
                    Arguments = $"l \"{filename}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string processOutput = await process.StandardOutput.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    if (processOutput.Contains("Method = LZMS"))
                    {
                        Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                        messageBox.Title = "Warning";
                        messageBox.Content = "This file cannot be mounted because it is compressed with the LZMS algorithm.";
                        await messageBox.ShowDialogAsync();
                        DismApi.Shutdown();
                        return;
                    }
                }

                contextMenuControl.IsEnabled = false;
                contextMenuControl.Visibility = Visibility.Hidden;
                SetControlsDisabled();
                selectedItem.Status = "Starting..";
                sourceListView.Items.Refresh();
                string mountPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Mount");

                DismApi.Initialize(DismLogLevel.LogErrors, "ShadesToolkit.txt");

                DismProgressCallback progressCallback = new DismProgressCallback((DismProgress progress) =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        int adjustedProgress = progress.Current % 100;
                        selectedItem.Status = $"{(isMount ? "Mounting" : "Unmounting")}... {adjustedProgress}%";
                        selectedItem.Progress = adjustedProgress;
                        sourceListView.Items.Refresh();
                    });
                });

                try
                {
                    selectedItem.IsMounting = true;
                    if (isMount)
                    {
                        await Task.Run(() => DismApi.MountImage(filename, mountPath, imageIndex, false, DismMountImageOptions.None, progressCallback));
                    }
                    else
                    {
                        Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                        messageBox.Title = "Warning";
                        messageBox.Content = "Should changes to the image be saved ?";
                        messageBox.PrimaryButtonText = "Yes";
                        messageBox.SecondaryButtonText = "No";
                        var result = await messageBox.ShowDialogAsync();
                        if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
                        {
                            await Task.Run(() => DismApi.UnmountImage(mountPath, true, progressCallback));
                            // ExportImage after unmounting with save
                            await ExportImageAsync(selectedItem.Filename, selectedItem.Index);
                        }
                        else if (result == Wpf.Ui.Controls.MessageBoxResult.Secondary)
                        {
                            await Task.Run(() => DismApi.UnmountImage(mountPath, false, progressCallback));
                            // ExportImage after unmounting without save
                            await ExportImageAsync(selectedItem.Filename, selectedItem.Index);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Error";
                    messageBox.Content = $"{ex.Message}";
                    await messageBox.ShowDialogAsync();
                }

                finally
                {
                    DismApi.Shutdown();
                    selectedItem.IsMounting = false;
                    selectedItem.Status = await GetMountStatusAsync(selectedItem.Filename, selectedItem.Index);
                    sourceListView.Items.Refresh();
                    contextMenuControl.Visibility = Visibility.Visible;
                    contextMenuControl.IsEnabled = true;
                    SetControlsEnabled();
                }

            }
        }

        private async Task<bool> IsAnyIndexMountedAsync(string filename)
        {
            DismMountedImageInfoCollection mountedImageInfos = await Task.Run(() => DismApi.GetMountedImages());
            foreach (DismMountedImageInfo mountedImageInfo in mountedImageInfos)
            {
                if (mountedImageInfo.ImageFilePath == filename)
                {
                    return true; // Any index of this WIM is mounted
                }
            }
            return false; // No index of this WIM is mounted
        }

        private async void ExportWim_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (sourceListView.SelectedItem is not null)
            {
                var selectedItem = (WimFile)sourceListView.SelectedItem;
                string filename = selectedItem.Filename;
                int index = selectedItem.Index;
                


                // Kullanıcının çıktıyı kaydetmek istediği konumu seçmesine izin verin
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "WIM Files (*.wim)|*.wim";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string outputPath = saveFileDialog.FileName;

                    // Durumu "Exporting.." olarak güncelleyin
                    selectedItem.Status = "Exporting..";
                    sourceListView.Items.Refresh();
                    SetControlsDisabled();
                    // ManagedWimLib'i kullanarak WIM dosyasını çıkarın
                    InitNativeLibrary();
                    using (Wim srcWim = Wim.OpenWim(filename, OpenFlags.CheckIntegrity))
                    {
                        using (Wim destWim = Wim.CreateNewWim(CompressionType.LZX))
                        {
                            ManagedWimLib.ProgressCallback progressCallback = (ManagedWimLib.ProgressMsg msgType, object info, object progctx) =>
                            {
                                if (msgType == ManagedWimLib.ProgressMsg.WriteStreams)
                                {
                                    var progressInfo = (ManagedWimLib.WriteStreamsProgress)info;
                                    double progressPercentage = 100.0 * progressInfo.CompletedBytes / progressInfo.TotalBytes;

                                    this.Dispatcher.Invoke(() =>
                                    {
                                        // İlerlemeyi güncelleyin
                                        selectedItem.Progress = (int)progressPercentage;

                                        // Durumu ve ilerlemeyi birleştirin
                                        selectedItem.Status = $"Exporting.. {progressPercentage:0}%";
                                        sourceListView.Items.Refresh();
                                    });
                                }
                                return CallbackStatus.Continue;
                            };

                            destWim.RegisterCallback(progressCallback);
                            await Task.Run(() =>
                            {
                                srcWim.ExportImage(index, destWim, null, null, ExportFlags.None);
                                destWim.Write(outputPath, Wim.AllImages, WriteFlags.CheckIntegrity, Wim.DefaultThreads);
                            });

                        }
                    }
                    Wim.GlobalCleanup();

                    // Durumu güncelleyin
                    selectedItem.Status = "Exported";
                    SetControlsEnabled();
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Succes";
                    messageBox.Content = "Export completed successfully.";
                    await messageBox.ShowDialogAsync();
                    selectedItem.Status = await GetMountStatusAsync(selectedItem.Filename, selectedItem.Index);
                    sourceListView.Items.Refresh();
                }
            }
        }
        private async Task ExportImageAsync(string filename, int index)
        {
            // Çıktı yolu, orijinal WIM dosyasının yanında "install_new.wim" olarak ayarlanır
            string outputPath = Path.Combine(Path.GetDirectoryName(filename), "install_new.wim");

            // Durumu "Exporting.." olarak güncelleyin
            var selectedItem = (WimFile)sourceListView.SelectedItem;
            selectedItem.Status = "Saving..";
            sourceListView.Items.Refresh();

            // ManagedWimLib'i kullanarak WIM dosyasını çıkarın
            InitNativeLibrary();
            using (Wim srcWim = Wim.OpenWim(filename, OpenFlags.None))
            {
                using (Wim destWim = Wim.CreateNewWim(CompressionType.LZX))
                {
                    ManagedWimLib.ProgressCallback progressCallback = (ManagedWimLib.ProgressMsg msgType, object info, object progctx) =>
                    {
                        if (msgType == ManagedWimLib.ProgressMsg.WriteStreams)
                        {
                            var progressInfo = (ManagedWimLib.WriteStreamsProgress)info;
                            double progressPercentage = 100.0 * progressInfo.CompletedBytes / progressInfo.TotalBytes;

                            this.Dispatcher.Invoke(() =>
                            {
                                // İlerlemeyi güncelleyin
                                selectedItem.Progress = (int)progressPercentage;

                                // Durumu ve ilerlemeyi birleştirin
                                selectedItem.Status = $"Saving.. {progressPercentage:0}%";
                                sourceListView.Items.Refresh();
                            });
                        }
                        return CallbackStatus.Continue;
                    };

                    destWim.RegisterCallback(progressCallback);
                    await Task.Run(() =>
                    {
                        srcWim.ExportImage(index, destWim, null, null, ExportFlags.None);
                        destWim.Write(outputPath, Wim.AllImages, WriteFlags.CheckIntegrity, Wim.DefaultThreads);
                    });

                }
            }
            Wim.GlobalCleanup();

            // Orijinal "install.wim" dosyasını silin
            File.Delete(filename);

            // Yeni oluşturulan "install_new.wim" dosyasını "install.wim" olarak yeniden adlandırın
            File.Move(outputPath, filename);

            // Durumu güncelleyin
            selectedItem.Status = "Exported";
            Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
            messageBox.Title = "Succes";
            messageBox.Content = "Wim file exported.";
            await messageBox.ShowDialogAsync();
            await Task.Delay(3000);
            selectedItem.Status = await GetMountStatusAsync(selectedItem.Filename, selectedItem.Index);
            sourceListView.Items.Refresh();
        }

        private async void Mount_ClickAsync(object sender, RoutedEventArgs e) => await MountUnmountAsync(true);

        private async void Unmount_ClickAsync(object sender, RoutedEventArgs e) => await MountUnmountAsync(false);

        private void sourceListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var source = e.OriginalSource as FrameworkElement;
            if (source.DataContext is WimFile)
            {
                sourceListView.ContextMenu.IsOpen = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private async void AddIso_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (sourceListView.Items.Count > 0)
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

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools", "7z.exe");
                psi.Arguments = $"l \"{filename}\"";
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                Process process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!output.Contains("install.wim"))
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = "Esd file type is not supported.  Please convert esd type to wim file type using converter.";
                    await messageBox.ShowDialogAsync();
                    return;
                }

                string extractPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Extracted");
                Directory.CreateDirectory(extractPath);
                SetControlsDisabled();
                sourceListView.Visibility = Visibility.Hidden;
                CleanupMountsPanel.Visibility = Visibility.Visible;
                DataText.Text = "Extracting..";

                await Task.Run(() =>
                {
                    psi.Arguments = $"x \"{filename}\" -o\"{Path.Combine(extractPath)}\" -y";
                    process = Process.Start(psi);
                    process.WaitForExit();
                });

                string wimFile = Path.Combine(extractPath, "sources", "install.wim");

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
                SetControlsEnabled();
                sourceListView.Visibility = Visibility.Visible;
                CleanupMountsPanel.Visibility = Visibility.Hidden;
            }
        }

        private async void Forget_ClickAsync(object sender, RoutedEventArgs e)
        {
            var itemsToRemove = new List<WimFile>();
            foreach (var item in sourceListView.Items)
            {
                if (item is WimFile wimFile && wimFile.Status == "Not Mounted")
                {
                    itemsToRemove.Add(wimFile);
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = "You need to unmount first to remove it from the list.";
                    await messageBox.ShowDialogAsync();
                    return;
                }
            }

            if (itemsToRemove.Count > 0)
            {
                Wpf.Ui.Controls.MessageBox confirmBox = new Wpf.Ui.Controls.MessageBox();
                confirmBox.Title = "Confirmation";
                confirmBox.Content = "Are you sure you want to remove these items from the list? The files are not deleted!";
                confirmBox.PrimaryButtonText = "Yes";
                confirmBox.CloseButtonText = "No";
                var result = await confirmBox.ShowDialogAsync();

                if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
                {
                    foreach (var item in itemsToRemove)
                    {
                        sourceListView.Items.Remove(item);

                        List<string> wimFiles = new List<string>();
                        if (File.Exists("wimFiles.json"))
                        {
                            wimFiles = JsonConvert.DeserializeObject<List<string>>(await File.ReadAllTextAsync("wimFiles.json"));
                        }

                        if (wimFiles.Contains(item.Filename))
                        {
                            wimFiles.Remove(item.Filename);
                            await File.WriteAllTextAsync("wimFiles.json", JsonConvert.SerializeObject(wimFiles));
                        }
                    }
                }
            }
            sourceListView.Items.Refresh();
        }


        private void Iso_Creator_Click(object sender, RoutedEventArgs e)
        {
            IsoCreator IsoCreator = new IsoCreator();
            IsoCreator.Show();
        }
        private void convert_btn_Click(object sender, RoutedEventArgs e)
        {
            WimToEsd wimConverter = new WimToEsd();
            wimConverter.Show();
        }
        private void host_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string hostsPath = Path.Combine(currentDirectory, "Mount", "Windows", "System32", "drivers", "etc", "hosts");
                if (File.Exists(hostsPath))
                {
                    Hosts hostsPage = new Hosts();
                    hostsPage.Show();
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Warning";
                    messageBox.Content = "Please mount a system first.";
                    messageBox.ShowDialogAsync();
                }
            }
            catch { }

        }

        private void OpenFileDirectory_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (sourceListView.SelectedItem is not null)
            {
                var selectedItem = (WimFile)sourceListView.SelectedItem;
                string filePath = selectedItem.Filename;
                string directoryPath = Path.GetDirectoryName(filePath);
                Process.Start("explorer.exe", directoryPath);
            }
        }

        private void OpenMountDirectory_Click(object sender, RoutedEventArgs e)
        {
            var mountPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mount");
            if (Directory.Exists(mountPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo("explorer.exe", mountPath));
                }
                catch { }
            }
            else
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Warning";
                messageBox.Content = "No mount directory found.";
                messageBox.ShowDialogAsync();
            }
        }

        private void CreateISO_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (WimFile)sourceListView.SelectedItem;
            string filePath = selectedItem.Filename;
            DirectoryInfo directoryInfo = Directory.GetParent(filePath);
            string parentDirectoryPath = directoryInfo?.Parent?.FullName;
            IsoCreator isoCreator = new IsoCreator();
            isoCreator.FolderDirectory.Text = parentDirectoryPath;
            isoCreator.SelectSaveDirectory.RaiseEvent(new RoutedEventArgs(Wpf.Ui.Controls.Button.ClickEvent));
            isoCreator.Show();
        }

        private void ImageName_Click(object sender, RoutedEventArgs e)
        {
            ImageName ImageName = new ImageName();
            ImageName.Show();
        }

        private void ImageDescription_Click(object sender, RoutedEventArgs e)
        {
            ImageDescription ImageDescription = new ImageDescription();
            ImageDescription.Show();
        }
        private void ImageFlags_Click(object sender, RoutedEventArgs e)
        {
            ImageFlags ImageFlags = new ImageFlags();
            ImageFlags.Show();
        }

        private void iso_download_btn_Click(object sender, RoutedEventArgs e)
        {
            ISODownloader ISODownloader = new ISODownloader();
            ISODownloader.Show();
        }
    }
}