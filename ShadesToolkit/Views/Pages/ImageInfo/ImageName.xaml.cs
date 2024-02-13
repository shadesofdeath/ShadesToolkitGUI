using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages.ImageInfo
{
    public partial class ImageName : FluentWindow
    {
        // Olayı tanımlayın
        public static event Action RefreshDataGridEvent;

        public ImageName()
        {
            InitializeComponent();

            this.Closed += (sender, e) => RefreshDataGridEvent?.Invoke();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string imageName = ImageNameTextBox.Text;

            if (imageName.Contains(" "))
            {
                System.Windows.MessageBox.Show("Do not leave spaces in the Image name.");
                return;
            }
            if (string.IsNullOrWhiteSpace(imageName))
            {
                System.Windows.MessageBox.Show("Image name cannot be left blank.");
                return;
            }
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string toolsPath = Path.Combine(appPath, "Tools", "WimLib");
            string jsonPath = Path.Combine(appPath, "wimFiles.json");

            var wimFiles = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(jsonPath));
            string wimFilePath = wimFiles[0]; // Assuming the first file in the list is the one you want to use

            string command = $"wimlib-imagex info \"{wimFilePath}\" 1 --image-property DISPLAYNAME={imageName}";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                WorkingDirectory = toolsPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
            string errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(errorOutput))
            {
                System.Windows.MessageBox.Show("Operation failed: " + errorOutput);
                return;
            }
            System.Windows.MessageBox.Show($"Image name changed to '{imageName}'.");
            var sourcePage = new ShadesToolkit.Views.Pages.Source(new ShadesToolkit.ViewModels.Pages.SourceViewModel());
            sourcePage.sourceDataGrid.Items.Refresh();
        }
    }

}
