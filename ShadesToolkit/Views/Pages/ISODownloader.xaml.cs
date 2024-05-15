using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;

namespace ShadesToolkit.Views.Pages
{
    public partial class ISODownloader : FluentWindow
    {
        private const string LangsUrl = "https://www.microsoft.com/en-us/api/controls/contentinclude/html?pageId=cd06bda8-ff9c-4a6e-912a-b92a21f42526&host=www.microsoft.com&segments=software-download%2cwindows11&query=&action=getskuinformationbyproductedition&sdVersion=2";
        private const string DownUrl = "https://www.microsoft.com/en-us/api/controls/contentinclude/html?pageId=cfa9e580-a81e-4a4b-a846-7b21bf4e2e5b&host=www.microsoft.com&segments=software-download%2Cwindows11&query=&action=GetProductDownloadLinksBySku&sdVersion=2";
        private const string SessionUrl = "https://vlscppe.microsoft.com/fp/tags?org_id=y6jn8c31&session_id=";
        private const string SharedSessionGUID = "47cbc254-4a79-4be6-9866-9c625eb20911";
        private string ApiUrl;

        private HttpClient httpClient;
        private string sessionId;
        private bool sharedSession;
        private string skuId;
        private string productId;
        public ISODownloader()
        {
            InitializeComponent();
            httpClient = new HttpClient();
            sessionId = Guid.NewGuid().ToString();
            sharedSession = false;
            LoadProducts();
            Loaded += OnLoaded;
        }
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var apiUrl = await FetchApiUrl();

            ApiUrl = apiUrl;
        }

        private async Task<string?> FetchApiUrl()
        {
            string apiUrlSource = "https://raw.githubusercontent.com/gravesoft/msdl/main/js/msdl.js";
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrlSource);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiUrlLine = content.Split('\n').FirstOrDefault(line => line.Contains("const apiUrl ="));
                    if (apiUrlLine != null)
                    {
                        var apiUrl = apiUrlLine.Split('"')[1];
                        return apiUrl;
                    }
                }
            }
            return null;
        }

        private async void LoadProducts()
        {
            try
            {
                string productsUrl = "https://raw.githubusercontent.com/gravesoft/msdl/main/data/products.json";
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetStringAsync(productsUrl);
                    var products = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                    ProductComboBox.ItemsSource = products;
                    ProductComboBox.DisplayMemberPath = "Value";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null) return;

            var selectedProduct = (KeyValuePair<string, string>)ProductComboBox.SelectedItem;
            productId = selectedProduct.Key;

            LoadLanguages(productId); // Asenkron çağrı kaldırıldı
        }

        private void LoadLanguages(string productId) // async ve Task kaldırıldı
        {
            try
            {
                HomeWindow.IsEnabled = false; // UI engelleme işlemi başlatıldı

                string url = LangsUrl + $"&productEditionId={productId}&sessionId={sessionId}";
                string response = httpClient.GetStringAsync(url).GetAwaiter().GetResult();

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(response);

                var options = doc.DocumentNode.SelectNodes("//select[@id='product-languages']/option")
                    .Where(o => o.GetAttributeValue("value", "") != "")
                    .Select(o => new
                    {
                        Text = o.InnerText.Trim(),
                        Value = JsonConvert.DeserializeObject<dynamic>(WebUtility.HtmlDecode(o.GetAttributeValue("value", ""))).id.ToString()
                    })
                    .ToList();

                LanguageComboBox.ItemsSource = options;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading languages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                HomeWindow.IsEnabled = true; // UI engelleme işlemi sonlandırıldı
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem == null || LanguageComboBox.SelectedItem == null)
            {
                return;
            }

            if (LanguageComboBox.SelectedItem == null) return;
            StatusTextBox.Text = "Getting connection information..";
            ProgressBar.Visibility = Visibility.Visible;
            HomeWindow.IsEnabled = false;
            var selectedLanguage = (dynamic)LanguageComboBox.SelectedItem;
            skuId = selectedLanguage.Value;

            // Download linklerini asenkron olarak al
            var downloadLinkNodes = await GetDownloadLink(skuId);

            if (downloadLinkNodes != null)
            {
                var window = new DownloadOptionWindow();

                var selectedProduct = (KeyValuePair<string, string>)ProductComboBox.SelectedItem;
                if (selectedProduct.Value.Contains("Windows 11"))
                {
                    window.X32Button.Visibility = Visibility.Collapsed;
                }

                if (window.ShowDialog() == true)
                {
                    string selectedOption = window.SelectedOption;

                    foreach (var downloadLinkNode in downloadLinkNodes)
                    {
                        string downloadLink = downloadLinkNode.GetAttributeValue("href", "");
                        if (selectedOption == "x32" && downloadLink.Contains("x32"))
                        {
                            await DownloadFileAsync(downloadLink); // Asenkron indirme
                            break;
                        }
                        else if (selectedOption == "x64" && downloadLink.Contains("x64"))
                        {
                            HomeWindow.IsEnabled = true;
                            await DownloadFileAsync(downloadLink); // Asenkron indirme
                            break;
                        }
                    }
                }
                else
                {
                    StatusTextBox.Text = "";
                    ProgressBar.Visibility = Visibility.Hidden;
                    HomeWindow.IsEnabled = true;
                }
            }
            else
            {
                StatusTextBox.Text = "";
                ProgressBar.Visibility = Visibility.Hidden;
                LanguageComboBox.IsEnabled = false;
                ProductComboBox.IsEnabled = false;
                DownloadButton.IsEnabled = false;
            }
        }


        private async Task<HtmlNodeCollection?> GetDownloadLink(string skuId)
        {
            try
            {
                string url = DownUrl + $"&skuId={skuId}&sessionId={sessionId}";
                string response = await TryGetResponse(url);

                if (response == null || response.Contains("Error"))
                {
                    url = DownUrl + $"&skuId={skuId}&sessionId={SharedSessionGUID}";
                    response = await TryGetResponse(url);
                }

                if (response == null || response.Contains("Error"))
                {
                    url = ApiUrl + "/proxy?product_id=" + productId + "&sku_id=" + skuId;
                    response = await TryGetResponse(url);
                }

                if (response != null && !response.Contains("Error"))
                {
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(response);

                    var downloadLinkNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '.iso')]");
                    return downloadLinkNodes;
                }
                else
                {
                    StatusTextBox.Text = "Error receiving connection!";
                    return null;
                }
            }
            catch (Exception)
            {
                StatusTextBox.Text = "Error receiving connection! ";
                return null;
            }
        }


        private async Task DownloadFileAsync(string downloadLink)
        {
            try
            {
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                var uri = new Uri(downloadLink);
                string fileName = Path.GetFileName(uri.LocalPath);
                saveFileDialog.FileName = fileName;
                saveFileDialog.Filter = "ISO files (*.iso)|*.iso";
                saveFileDialog.DefaultExt = ".iso";

                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fullPath = saveFileDialog.FileName;

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadProgressChanged += (s, e) =>
                        {
                            ProgressBar.Value = e.ProgressPercentage;
                            double bytesIn = double.Parse(e.BytesReceived.ToString());
                            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                            double percentage = bytesIn / totalBytes * 100;

                            if (bytesIn > 1024 * 1024 * 1024)
                            {
                                IsoSize.Text = $"( {Math.Round(bytesIn / 1024 / 1024 / 1024, 2)} GB / {Math.Round(totalBytes / 1024 / 1024 / 1024, 2)} GB )";
                            }
                            else
                            {
                                IsoSize.Text = $"( {Math.Round(bytesIn / 1024 / 1024, 2)} MB / {Math.Round(totalBytes / 1024 / 1024 / 1024, 2)} GB )";
                            }
                        };


                        webClient.DownloadFileCompleted += async (s, e) =>
                        {
                            if (e.Error == null)
                            {
                                ProgressBar.Visibility = Visibility.Hidden;
                                LanguageComboBox.IsEnabled = true;
                                ProductComboBox.IsEnabled = true;
                                DownloadButton.IsEnabled = true;
                                StatusTextBox.Text = "";
                                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                                messageBox.Title = "Bilgi";
                                messageBox.Content = "ISO downloaded successfully.";
                                await messageBox.ShowDialogAsync();
                                return;
                                //MessageBox.Show("ISO downloaded successfully.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                            }
                            else
                            {
                                ProgressBar.Visibility = Visibility.Hidden;
                                LanguageComboBox.IsEnabled = true;
                                ProductComboBox.IsEnabled = true;
                                DownloadButton.IsEnabled = true;
                                StatusTextBox.Text = "";
                                Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                                messageBox.Title = "Error";
                                messageBox.Content = $"Error downloading ISO: {e.Error.Message}";
                                await messageBox.ShowDialogAsync();
                                return;
                                //MessageBox.Show($"Error downloading ISO: {e.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        };
                        StatusTextBox.Text = "Download Started..";
                        HomeWindow.IsEnabled = true;
                        LanguageComboBox.IsEnabled = false;
                        ProductComboBox.IsEnabled = false;
                        DownloadButton.IsEnabled = false;
                        ProgressBar.Visibility = Visibility.Visible;
                        await webClient.DownloadFileTaskAsync(new Uri(downloadLink), fullPath);
                    }
                }
                else
                {
                    StatusTextBox.Text = "";
                    ProgressBar.Visibility = Visibility.Hidden;
                    LanguageComboBox.IsEnabled = true;
                    ProductComboBox.IsEnabled = true;
                    DownloadButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading ISO:: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBox.Text = "Error Occurred!";
                StatusTextBox.Text = "";
                ProgressBar.Visibility = Visibility.Hidden;
                LanguageComboBox.IsEnabled = true;
                ProductComboBox.IsEnabled = true;
                DownloadButton.IsEnabled = true;
            }
        }


        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
        }


        private async Task<string?> TryGetResponse(string url)
        {
            try
            {
                string response = await httpClient.GetStringAsync(url);
                if (response.Contains("Error"))
                {
                    CloseButton.IsEnabled = false;
                    url = ApiUrl + "proxy" + "?product_id=" + productId + "&sku_id=" + skuId;
                    response = await httpClient.GetStringAsync(url);
                    CloseButton.IsEnabled = true;
                }
                if (response.Contains("Error"))
                {
                    CloseButton.IsEnabled = false;
                    url = url.Replace(sessionId, SharedSessionGUID);
                    response = await httpClient.GetStringAsync(url);
                    CloseButton.IsEnabled = true;
                }

                return response;
            }
            catch (Exception)
            {
                CloseButton.IsEnabled = true;
                return null;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
