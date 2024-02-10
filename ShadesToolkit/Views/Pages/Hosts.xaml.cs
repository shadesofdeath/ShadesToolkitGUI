using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.UI.Core;
using Wpf.Ui.Controls;

namespace ShadesToolkit.Views.Pages
{
    public partial class Hosts : FluentWindow
    {
        public Hosts()
        {
            InitializeComponent();

            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string hostsPath = System.IO.Path.Combine(currentDirectory, "Mount", "Windows", "System32", "drivers", "etc", "hosts");
                if (File.Exists(hostsPath))
                {
                    textBlock.Text = File.ReadAllText(hostsPath);
                }
                else
                {
                    System.Windows.MessageBox.Show("Hosts file not found: " + hostsPath);
                }
            }
            catch { }            
        }

        private void convert_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string hostsPath = System.IO.Path.Combine(currentDirectory, "Mount", "Windows", "System32", "drivers", "etc", "hosts");
                File.WriteAllText(hostsPath, textBlock.Text);
                System.Windows.MessageBox.Show("Host file saved.");
            }
            catch { }    
        }

        private async void StevenBlack_Click(object sender, RoutedEventArgs e)
        {
            ContentPanel.Visibility = Visibility.Collapsed;
            ProgressRing.Visibility = Visibility.Visible;
            ProgressRing.IsIndeterminate = true;

            await Task.Run(async () =>
            {
                string url = "https://raw.githubusercontent.com/StevenBlack/hosts/master/hosts";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        string content = await client.GetStringAsync(url);
                        Dispatcher.Invoke(() =>
                        {
                            textBlock.Text = content;
                        });
                    }
                    catch (Exception)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show("A problem has occurred please try later");
                        });
                    }
                }
            });

            ProgressRing.IsIndeterminate = false;
            ProgressRing.Visibility = Visibility.Collapsed;
            ContentPanel.Visibility = Visibility.Visible;
        }

        private void default_small_Click(object sender, RoutedEventArgs e)
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

            textBlock.Text = hostsContent;
        }
    }
}
