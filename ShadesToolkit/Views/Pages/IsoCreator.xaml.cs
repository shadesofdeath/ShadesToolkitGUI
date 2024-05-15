using System.Diagnostics;
using System.IO;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages
{
    public partial class IsoCreator : FluentWindow
    {
        public IsoCreator()
        {
            InitializeComponent();
            SelectFolder.Click += SelectFolder_Click;
            SelectSaveDirectory.Click += SelectSaveDirectory_Click;
            CreateISO.Click += CreateISO_Click;
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FolderDirectory.Text = dialog.SelectedPath;
            }
        }

        private void SelectSaveDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "ShadesToolkit",
                DefaultExt = ".iso",
                Filter = "ISO Image (.iso)|*.iso"
            };
            if (dialog.ShowDialog() == true)
            {
                SaveDirectory.Text = dialog.FileName;
            }
        }

        private async void CreateISO_Click(object sender, RoutedEventArgs e)
        {
            ProgressProcess.Visibility = Visibility.Visible;
            ProcessText.Visibility = Visibility.Visible;
            ProcessText.Text = "ISO Creating please wait..";
            string folderDirectory = FolderDirectory.Text;
            string saveDirectory = SaveDirectory.Text;

            if (string.IsNullOrEmpty(folderDirectory) || string.IsNullOrEmpty(saveDirectory))
            {
                ProcessText.Text = "Please select a valid folder and save directory.";
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    string efiFilePath = Path.Combine(folderDirectory, "efi", "microsoft", "boot", "efisys.bin");
                    string etfsbootPath = Path.Combine(folderDirectory, "boot", "etfsboot.com");
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "oscdimg.exe"),
                            Arguments = $"-l\"ShadesToolkit\" -m -o -u2 -udfver102 -bootdata:2#p0,e,b\"{etfsbootPath}\"#pEF,e,b\"{efiFilePath}\" \"{folderDirectory}\" \"{saveDirectory}\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    Dispatcher.Invoke(() =>
                    {
                        ProgressProcess.Visibility = Visibility.Hidden;
                        if (process.ExitCode == 0)
                        {
                            ProcessText.Text = "ISO Created successfully";
                        }
                        else
                        {
                            ProcessText.Text = $"Failed with exit code {process.ExitCode}. Output: {process.StandardOutput.ReadToEnd()}";
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProgressProcess.Visibility = Visibility.Hidden;
                        ProcessText.Text = $"An error occurred: {ex.Message}";
                    });
                }
            });
        }
    }
}
