using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using WebView2Traffic.Data;
using WebView2Traffic.Models;

namespace WebView2Traffic.ViewModels
{
    public class ClientWindowViewModel : INotifyPropertyChanged
    {
        public string Uid { get; } = Guid.NewGuid().ToString();

        private const int LIMIT_SERP = 7;
        private const string PROXY_IP_ADDRESS = "117.0.200.23";
        private const string PROXY_PORT = "40599";
        private const string PROXY_USERNAME = "yiekd_phanp";
        private const string PROXY_PASSWORD = "HsRDWj87";
        private CoreWebView2Environment _webView2Environment;

        private readonly List<string> _internalLinks =
        [
            "https://terusvn.com/dich-vu-seo-tong-the-uy-tin-hieu-qua-tai-terus/",
            "https://terusvn.com/thiet-ke-website-tai-hcm/",
            "https://terusvn.com/dich-vu-quang-cao-google-tai-terus/",
            "https://terusvn.com/dich-vu-facebook-ads-tai-terus/"
        ];

        private readonly WebView2 _webView2;

        private CancellationTokenSource? _cancellationTokenSource;

        private TrafficURL _trafficURL = new()
        {
            ID = 0,
            Keyword = "Khởi tạo client",
            URL = "https://www.bing.com",
            Rank = 0,
            RequireQuantity = 1,
            CurrentQuantity = 1,
            Type = "Direct",
            Mobile = "no",
        };
        public TrafficURL TrafficURL
        {
            get { return _trafficURL; }
            set
            {
                _trafficURL = value;
                OnPropertyChanged(nameof(TrafficURL));
            }
        }

        private string _currentUrl;
        public string CurrentUrl
        {
            get { return _currentUrl; }
            set
            {
                _currentUrl = value;
                OnPropertyChanged(nameof(CurrentUrl));
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private bool _isScrolling;
        public bool IsScrolling
        {
            get => _isScrolling;
            set
            {
                _isScrolling = value;
                OnPropertyChanged(nameof(IsScrolling));
            }
        }

        private bool _notFoundSERP;
        public bool NotFoundSERP
        {
            get => _notFoundSERP;
            set
            {
                _notFoundSERP = value;
                OnPropertyChanged(nameof(NotFoundSERP));
            }
        }

        public ClientWindowViewModel(TrafficURL trafficURL, WebView2 webView2)
        {
            _webView2 = webView2;
            _webView2.ContentLoading += WebView2_ContentLoading;
            _webView2.NavigationCompleted += WebView2_NavigationCompleted;
            _trafficURL = trafficURL;
            CurrentUrl = $"Client {Uid} | {webView2.Source?.ToString()}";
        }

        private void WebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            IsLoading = false;
            CurrentUrl = $"Client {Uid} | {_webView2.Source?.ToString()}";
            Debug.WriteLine($"Loaded: {_webView2.Source?.ToString()} | StatusCode: {e.HttpStatusCode}");
        }

        private void WebView2_ContentLoading(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2ContentLoadingEventArgs e)
        {
            CurrentUrl = $"Client {Uid} | Loading...";
        }

        public async Task InitializeWebView2Async()
        {
            try
            {
                if (_webView2.CoreWebView2 == null)
                {
                    //var proxy = $"{PROXY_USERNAME}:{PROXY_PASSWORD}:{PROXY_IP_ADDRESS}:{PROXY_PORT}";
                    //var options = new CoreWebView2EnvironmentOptions
                    //{
                    //    AdditionalBrowserArguments = $"--proxy-server={proxy}"
                    //};

                    //_webView2Environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                    await _webView2.EnsureCoreWebView2Async();
                }
                await StartSessionAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when load WebView2: " + ex.Message);
                MessageBox.Show("Error when load WebView2: " + ex.Message);
            }
        }

        public async Task StartSessionAsync()
        {
            if (IsRunning) return;
            IsRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                await NavigateToGoogle();
                if (_trafficURL.Type?.ToLower(System.Globalization.CultureInfo.CurrentCulture) == "search")
                {
                    Debug.WriteLine("Search");
                    if (!await PerformSearchAndNavigateAsync(_trafficURL.Keyword, _trafficURL.URL, token))
                    {
                        // Handle captcha or timeout, for now just navigate directly
                        NotFoundSERP = true;
                        await NavigateToTargetUrlAsync(_trafficURL.URL);
                    }
                }
                else if (_trafficURL.Type?.ToLower(System.Globalization.CultureInfo.CurrentCulture) == "direct")
                {
                    Debug.WriteLine("Direct");
                    NotFoundSERP = true;
                    await NavigateToTargetUrlAsync(_trafficURL.URL);
                }

                if (!token.IsCancellationRequested)
                {
                    await ScrollAndProcessInternalLinksAsync(token);
                    if (_trafficURL.Type == "Direct" || (_trafficURL.Type == "Search" && !NotFoundSERP))
                    {
                        DataSource.Instance.UpdateTrafficURL(new TrafficURL
                        {
                            ID = _trafficURL.ID,
                            CurrentQuantity = _trafficURL.CurrentQuantity + 1,
                            RequireQuantity = _trafficURL.RequireQuantity // Keep the original require quantity
                        });
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Phiên chạy cho {_trafficURL.URL} đã bị dừng.");
            }
            finally
            {
                IsRunning = false;
                _cancellationTokenSource?.Dispose();
                Debug.WriteLine("Session completed");
                // Optionally start a new session if needed
                var trafficURL = DataSource.Instance.GetNextTrafficURL(TrafficURL.ID + 1);
                if (trafficURL != null)
                {
                    TrafficURL = trafficURL;
                    await StartSessionAsync();
                }
            }
        }

        public void StopSession()
        {
            _cancellationTokenSource?.Cancel();
            IsRunning = false;
            // Optionally perform cleanup on WebView2 if needed
        }

        private async Task NavigateToGoogle()
        {
            _webView2.CoreWebView2.Navigate("https://www.google.com");
            await Task.Delay(3000); // Wait for page to load
        }

        private async Task<bool> PerformSearchAndNavigateAsync(string keyword, string targetUrl, CancellationToken token)
        {
            await ExecuteJavaScriptAsync($"document.querySelector('textarea').value = '{keyword}';");
            await ExecuteJavaScriptAsync($"document.querySelector('form[action=\"/search\"]').submit();");
            IsLoading = true;
            while (IsLoading)
            {
                await Task.Delay(5000, token); // Wait for search results
            }

            // Basic captcha check (you'll need more robust logic)
            if (CheckForCaptcha())
            {
                // Notify user about captcha (for now, log to console)
                Debug.WriteLine($"Captcha detected for {_trafficURL.URL}. Waiting 5 minutes.");
                // Wait for 5 minutes or until cancelled
                try
                {
                    var countdown = 300;
                    while (CheckForCaptcha() && countdown > 0)
                    {
                        countdown--;
                        await Task.Delay(TimeSpan.FromSeconds(1), token);
                    }
                    if (CheckForCaptcha())
                    {
                        Debug.WriteLine($"Captcha still present for {_trafficURL.URL}. Switching to direct traffic.");
                        return false;
                    }
                }
                catch (TaskCanceledException)
                {
                    return false;
                }
            }

            return await FindAndClickTargetUrlAsync(targetUrl, token);
        }

        private async Task<bool> FindAndClickTargetUrlAsync(string targetUrl, CancellationToken token)
        {
            for (int page = 1; page <= LIMIT_SERP; page++) // Limit search pages
            {
                string selector = $"a[href^='{targetUrl}'] h3";
                string? result = await EvaluateJavaScriptAsync($"document.querySelector('{selector}')?.innerText;");
                if (!string.IsNullOrEmpty(result) && result != "null" && result != "undefined")
                {
                    await ScrollToBottomAsync(token);
                    while (IsScrolling) await Task.Delay(5000, token);
                    Debug.WriteLine($"Text link: {result}");
                    await ExecuteJavaScriptAsync($"setTimeout(() => {{ document.querySelector('a[href^=\"{targetUrl}\"]').click(); }}, 1000);");
                    await Task.Delay(5000, token); // Wait for target page to load
                    return true;
                }
                if (page < 5)
                {
                    await ScrollToBottomAsync(token);
                    while (IsScrolling) await Task.Delay(5000, token);
                    Debug.WriteLine($"Not found in page {page}");
                    string nextPageSelector = $"#botstuff table a[aria-label='Page {page + 1}']";
                    string? nextPageAvailable = await EvaluateJavaScriptAsync($"document.querySelector(\"{nextPageSelector}\") !== null;");
                    if (!string.IsNullOrEmpty(nextPageAvailable) && nextPageAvailable == "true")
                    {
                        Debug.WriteLine($"Pagination: {nextPageAvailable}");
                        await ExecuteJavaScriptAsync($"document.querySelector(\"{nextPageSelector}\").click();");
                        IsLoading = true;
                        while (IsLoading)
                        {
                            await Task.Delay(5000, token); // Wait for next page
                        }
                        continue;
                    }
                    else
                    {
                        break; // No more next pages
                    }
                }
            }
            return false; // Target URL not found
        }

        private async Task NavigateToTargetUrlAsync(string targetUrl)
        {
            _webView2.CoreWebView2.Navigate(targetUrl);
            await Task.Delay(5000); // Wait for page to load
        }

        private async Task ScrollAndProcessInternalLinksAsync(CancellationToken token)
        {
            Debug.WriteLine("Start internal link");
            await ScrollToBottomAsync(token);
            while (IsScrolling) await Task.Delay(5000, token);

            foreach (var internalLink in _internalLinks)
            {
                if (token.IsCancellationRequested) break;
                string selector = $"a[href^='{internalLink}']";
                string? internalLinkFound = await EvaluateJavaScriptAsync($"document.querySelector(\"{selector}\")?.href;");
                if (!string.IsNullOrEmpty(internalLinkFound))
                {
                    await ExecuteJavaScriptAsync($"document.querySelector(\"{selector}\").click();");
                    await Task.Delay(5000, token);
                    await ScrollToBottomAsync(token);
                    await Task.Delay(1000, token);
                }
            }
        }

        private async Task ScrollToBottomAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            IsScrolling = true;
            await ExecuteJavaScriptAsync(ScrollToBottomJs());
            if (!await IsAtBottomAsync(token))
            {
                await Task.Delay(30 * 1000, token);
            }
            IsScrolling = false;
        }

        private async Task<bool> IsAtBottomAsync(CancellationToken token)
        {
            if (!IsScrolling) return false;
            if (token.IsCancellationRequested) return false;
            string? res = await EvaluateJavaScriptAsync(IsAtBottomJs());
            return !string.IsNullOrEmpty(res) && res == "true";
        }

        private bool CheckForCaptcha()
        {
            // Basic check for common captcha elements - improve this logic
            string[] captchaList = [
                "google.com/sorry",
                "ipv6.google.com",
                "ipv4.google.com",
                "www.ipv6.google.com",
                "www.ipv4.google.com"
            ];
            return captchaList.Any(_webView2.Source.ToString().Contains);
            //_webView2.
            //string captchaElement1 = await ExecuteJavaScriptAsync("document.querySelector('#px-captcha')?.innerText;");
            //string captchaElement2 = await ExecuteJavaScriptAsync("document.querySelector('.g-recaptcha')?.innerText;");
            //return !string.IsNullOrEmpty(captchaElement1) || !string.IsNullOrEmpty(captchaElement2);
        }

        private async Task ExecuteJavaScriptAsync(string script)
        {
            if (_webView2?.CoreWebView2 != null)
            {
                await _webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
        }

        private async Task<string?> EvaluateJavaScriptAsync(string script)
        {
            if (_webView2?.CoreWebView2 != null)
            {
                return await _webView2.CoreWebView2.ExecuteScriptAsync(script);
            }
            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string ScrollToBottomJs(int seconds = 30)
        {
            string script = $"const totalDuration = {seconds} * 1000";
            script += @"
                function scrollToBottomWithPauses(totalTime, pauseDuration) {
                  const startTime = performance.now();
                  const endTime = startTime + totalTime;
                  const scrollHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
                  let lastScrollTop = 0;
                  function scrollStep() {
                    const currentTime = performance.now();
                    if (currentTime >= endTime) {
                      window.scrollTo(0, scrollHeight);
                      return;
                    }
                    const elapsed = currentTime - startTime;
                    const progress = Math.min(1, elapsed / totalTime);
                    const targetScrollTop = progress * scrollHeight;
                    window.scrollTo(0, targetScrollTop);
                    if (Math.abs(targetScrollTop - lastScrollTop) < 1) {
                      setTimeout(scrollStep, pauseDuration);
                    } else {
                      lastScrollTop = targetScrollTop;
                      setTimeout(scrollStep, 0);
                    }
                  }

                  scrollStep();
                }
                scrollToBottomWithPauses(totalDuration, 1000)
            ";
            return script;
        }

        private static string IsAtBottomJs()
        {
            var script = @"
                function isAtBottomOfPage() {
                  const documentHeight = Math.max(
                    document.body.scrollHeight,
                    document.documentElement.scrollHeight,
                    document.body.offsetHeight,
                    document.documentElement.offsetHeight,
                    document.documentElement.clientHeight
                  );
                  const windowHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;
                  const scrollY = window.scrollY || window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0;
                  return scrollY + windowHeight >= documentHeight - 10;
                }
            ";
            return script;
        }
    }
}
