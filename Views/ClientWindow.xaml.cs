using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Windows;

namespace WebView2Traffic.Views
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private CoreWebView2Environment? Environment;
        private const string PROXY_IP_ADDRESS = "117.0.200.23";
        private const string PROXY_PORT = "40599";
        private const string PROXY_USERNAME = "yiekd_phanp";
        private const string PROXY_PASSWORD = "HsRDWj87";
        private const string USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";

        public ClientWindow()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var proxy = $"{PROXY_IP_ADDRESS}:{PROXY_PORT}";
                var options = new CoreWebView2EnvironmentOptions
                {
                    AdditionalBrowserArguments = $"--proxy-server={proxy}"
                };
                Environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                await WebView.EnsureCoreWebView2Async(Environment);

                WebView.CoreWebView2.BasicAuthenticationRequested += CoreWebView2_BasicAuthenticationRequested;
                WebView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;

                WebView.Source = new Uri("https://www.google.com");
                WebView.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when load WebView2: " + ex.Message);
                MessageBox.Show("Error when load WebView2: " + ex.Message);
            }
        }

        private void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            WebView.CoreWebView2.Settings.UserAgent = USER_AGENT;
        }

        private void CoreWebView2_BasicAuthenticationRequested(object? sender, CoreWebView2BasicAuthenticationRequestedEventArgs e)
        {
            e.Response.UserName = PROXY_USERNAME;
            e.Response.Password = PROXY_PASSWORD;
        }
    }
}
