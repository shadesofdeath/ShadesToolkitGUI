using ManagedWimLib;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages
{
    public partial class WimToEsd : FluentWindow
    {
        private string selectedFilePath;
        private string convertedFilePath;
        private BackgroundWorker worker;

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

        public WimToEsd()
        {
            InitializeComponent();

            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;

            selectButton.Click += (sender, e) =>
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "WIM or ESD files (*.wim, *.esd)|*.wim;*.esd";
                if (openFileDialog.ShowDialog() == true)
                {
                    selectedFilePath = openFileDialog.FileName;
                    filePathTextBox.Text = selectedFilePath;
                }
            };

            saveButton.Click += async (sender, e) =>
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFilePath);
                if (selectedFilePath.EndsWith(".wim"))
                {
                    saveFileDialog.Filter = "ESD files (*.esd)|*.esd";
                    saveFileDialog.FileName = fileNameWithoutExtension + ".esd";
                }
                else if (selectedFilePath.EndsWith(".esd"))
                {
                    saveFileDialog.Filter = "WIM files (*.wim)|*.wim";
                    saveFileDialog.FileName = fileNameWithoutExtension + ".wim";
                }
                if (saveFileDialog.ShowDialog() == true)
                {
                    convertedFilePath = saveFileDialog.FileName;
                    progressLabel.Visibility = Visibility.Visible;
                    progressPercentageLabel.Visibility = Visibility.Visible; // Yeni eklenen satır
                    progress.Visibility = Visibility.Visible;
                    progressLabel.Content = "Conversion in progress..";
                    worker.RunWorkerAsync();
                }
            };


        }


        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            InitNativeLibrary();
            using (Wim srcWim = Wim.OpenWim(selectedFilePath, OpenFlags.None))
            {
                using (Wim destWim = Wim.CreateNewWim(GetCompressionType()))
                {
                    ProgressCallback progressCallback = new ProgressCallback(ProgressFunc);
                    destWim.RegisterCallback(progressCallback);
                    srcWim.ExportImage(1, destWim, "ImageName", null, ExportFlags.Gift);
                    destWim.Write(convertedFilePath, Wim.AllImages, WriteFlags.None, Wim.DefaultThreads);
                }
            }
            Wim.GlobalCleanup();
            Dispatcher.Invoke(() => progress.IsIndeterminate = false);
            progressLabel.Content = "Conversion completed.";
        }

        private CallbackStatus ProgressFunc(ProgressMsg msgType, object info, object progctx)
        {
            if (msgType == ProgressMsg.WriteStreams)
            {
                var progressInfo = (WriteStreamsProgress)info;
                double progressPercentage = 100.0 * progressInfo.CompletedBytes / progressInfo.TotalBytes;

                Dispatcher.Invoke(() =>
                {
                    progress.Value = progressPercentage;
                    progressPercentageLabel.Content = $"%{progressPercentage:0.00}";

                    // Dönüşümün tamamlandığını kontrol et
                    if (progressPercentage >= 100.0)
                    {
                        progressLabel.Content = "Conversion completed.";
                    }
                });
            }

            return CallbackStatus.Continue;
        }


        private CompressionType GetCompressionType()
        {
            return (CompressionType)Dispatcher.Invoke(new Func<CompressionType>(() =>
            {
                string selectedCompression = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                switch (selectedCompression)
                {
                    case "LZX":
                        return CompressionType.LZX;
                    case "XPRESS":
                        return CompressionType.XPRESS;
                    case "LZMS":
                        return CompressionType.LZMS;
                    default:
                        return CompressionType.LZX;
                }
            }));
        }

    }
}
