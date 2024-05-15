using System.IO;
using System.Net.Http;
using Wpf.Ui.Controls;
using System.Threading.Tasks;
using System.Windows;
using Windows.Networking;

namespace ShadesToolkit.Views.Pages
{
    public partial class Hosts : FluentWindow
    {
        public Hosts()
        {
            InitializeComponent();

            // Sayfa açıldığında host dosyasını kontrol et
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string hostsPath = Path.Combine(currentDirectory, "Mount", "Windows", "System32", "drivers", "etc", "hosts");

                if (File.Exists(hostsPath))
                {
                    // Hosts dosyası varsa içeriğini textbox'a yaz
                    string hostsContent = File.ReadAllText(hostsPath);
                    HostTextBlock.Text = hostsContent;
                }
                else
                {
                    // Hosts dosyası yoksa hata mesajı göster ve pencereyi kapat
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "Error";
                    messageBox.Content = "Hosts file not found.";
                    messageBox.ShowDialogAsync();
                    this.Close(); // Pencereyi kapat
                    return; // Fonksiyonu sonlandır
                }
            }
            catch (Exception ex)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = $"Error checking hosts file: {ex.Message}";
                messageBox.ShowDialogAsync();
                this.Close(); // Pencereyi kapat
                return; // Fonksiyonu sonlandır
            }
        }

        private void convert_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string hostsPath = Path.Combine(currentDirectory, "Mount", "Windows", "System32", "drivers", "etc", "hosts");

                File.WriteAllText(hostsPath, HostTextBlock.Text);
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Succes!";
                messageBox.Content = "Hosts file saved.";
                messageBox.ShowDialogAsync();
            }
            catch (Exception ex)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = $"Error saving the hosts file: {ex.Message}";
                messageBox.ShowDialogAsync();
            }
        }

        private async void StevenBlack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentPanel.Visibility = Visibility.Collapsed;
                ProgressRing.Visibility = Visibility.Visible;
                ProgressRing.IsIndeterminate = true;
                HostPage.IsEnabled = false;

                await Task.Run(async () =>
                {
                    string url = "https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts";
                    using (HttpClient client = new HttpClient())
                    {
                        try
                        {
                            string content = await client.GetStringAsync(url);

                            // UI'ı güncellemek için Dispatcher.Invoke kullanın
                            Dispatcher.Invoke(() =>
                            {
                                HostTextBlock.Text = content;
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                                messageBox.Title = "Error";
                                messageBox.Content = $"StevenBlack hosts dosyası indirilirken hata oluştu: {ex.Message}";
                                messageBox.ShowDialogAsync();
                            });
                        }
                        finally
                        {
                            // UI'ı güncellemek için Dispatcher.Invoke kullanın
                            Dispatcher.Invoke(() =>
                            {
                                ProgressRing.IsIndeterminate = false;
                                ProgressRing.Visibility = Visibility.Collapsed;
                                ContentPanel.Visibility = Visibility.Visible;
                                HostPage.IsEnabled = true;
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Error";
                messageBox.Content = $"StevenBlack hosts dosyası indirilirken hata oluştu: {ex.Message}";
                await messageBox.ShowDialogAsync();
            }
            finally
            {
                ProgressRing.IsIndeterminate = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Visible;
                HostPage.IsEnabled = true;
            }
        }

        private void default_small_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string hostsContent = @"# Copyright (c) 1993-2009 Microsoft Corp.
#
# This is a sample HOSTS file used by Microsoft TCP/IP for Windows.
#
# This file contains the mappings of IP addresses to host names. Each
# entry should be kept on an individual line. The IP address should
# be placed in the first column followed by the corresponding host name.
# The IP address and the host name should be separated by at least one
# space.
#
# Additionally, comments (such as these) may be inserted on individual
# lines or following the machine name denoted by a '#' symbol.
#
# For example:
#
#      102.54.94.97     rhino.acme.com          # source server
#       38.25.63.10     x.acme.com              # x client host

# localhost name resolution is handled within DNS itself.
#	127.0.0.1       localhost
#	::1             localhost
";

                HostTextBlock.Text = hostsContent;
            }
            catch (Exception ex)
            {
                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                messageBox.Title = "Hata";
                messageBox.Content = $"Varsayılan hosts dosyası yüklenirken hata oluştu: {ex.Message}";
                messageBox.ShowDialogAsync();
            }
            finally
            {
                ProgressRing.IsIndeterminate = false;
                ProgressRing.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Visible;
                HostPage.IsEnabled = true;
            }
        }
    }
}