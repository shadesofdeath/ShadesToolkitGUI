using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages.ImageInfo
{
    public partial class ImageFlags : FluentWindow
    {
        // Olayı tanımlayın
        public static event Action RefreshDataGridEvent;

        public ImageFlags()
        {
            InitializeComponent();

            this.Closed += (sender, e) => RefreshDataGridEvent?.Invoke();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string imageName = ImageNameTextBox.Text;

            if (imageName.Contains(" "))
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Warning";
                messageBox.Content = "Do not leave spaces in the Image flags.";
                messageBox.ShowDialogAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(imageName))
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Warning";
                messageBox.Content = "Image flags cannot be left blank.";
                messageBox.ShowDialogAsync();
                return;
            }
            string appPath = AppDomain.CurrentDomain.BaseDirectory;
            string toolsPath = Path.Combine(appPath, "Tools", "WimLib");
            string jsonPath = Path.Combine(appPath, "wimFiles.json");

            var wimFiles = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(jsonPath));
            string wimFilePath = wimFiles[0];

            string command = $"wimlib-imagex info \"{wimFilePath}\" 1 --image-property FLAGS={imageName} --image-property WINDOWS/EDITIONID={imageName}";

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
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Failed";
                messageBox.Content = "Failed: " + errorOutput;
                messageBox.ShowDialogAsync();
                return;
            }
            Thread.Sleep(6000);
            Wpf.Ui.Controls.MessageBox messageBoxFailed = new Wpf.Ui.Controls.MessageBox();
            messageBoxFailed.Title = "Succes";
            messageBoxFailed.Content = $"Image flags changed to '{imageName}'.";
            messageBoxFailed.ShowDialogAsync();
            var sourcePage = new Source(new ViewModels.Pages.SourceViewModel());
            sourcePage.sourceListView.Items.Refresh();
        }
    }

}
