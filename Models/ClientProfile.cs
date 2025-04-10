using WebView2Traffic.ViewModels;
using WebView2Traffic.Views;

namespace WebView2Traffic.Models
{
    public class ClientProfile
    {
        public required string Uid { get; set; }
        public required ClientWindow Window { get; set; }
        public required ClientWindowViewModel ViewModel { get; set; }
    }
}
