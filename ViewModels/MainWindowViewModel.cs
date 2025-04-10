using Microsoft.Web.WebView2.Wpf;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WebView2Traffic.Commands;
using WebView2Traffic.Data;
using WebView2Traffic.Models;
using WebView2Traffic.Views;
using Windows.Media.Protection.PlayReady;

namespace WebView2Traffic.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ClientProfile> Clients { get; } = [];

        private List<TrafficURL> _trafficURLs = [];
        public List<TrafficURL> TrafficURLs
        {
            get => _trafficURLs;
            set
            {
                _trafficURLs = value;
                OnPropertyChanged();
            }
        }

        private string? _excelFilePath;
        public string? ExcelFilePath
        {
            get => _excelFilePath;
            set
            {
                _excelFilePath = value;
                OnPropertyChanged();
                Console.WriteLine("Get File: " + value);
                LoadDataCommand.RaiseCanExecuteChanged(); // Enable/disable LoadDataCommand
            }
        }

        private string _loadExcelResponse = "No Data is loaded.";
        public string LoadExcelResponse
        {
            get => _loadExcelResponse;
            set
            {
                _loadExcelResponse = value;
                OnPropertyChanged();
            }
        }

        private string? _clientWindowCountText;
        public string? ClientWindowCountText
        {
            get => _clientWindowCountText;
            set
            {
                if (value != null && int.TryParse(value, out _))
                {
                    _clientWindowCount = int.Parse(value);
                }
                else
                {
                    _clientWindowCount = 0; // Or handle invalid input appropriately
                }
                _clientWindowCountText = value;
                OnPropertyChanged();
                AddClientsCommand.RaiseCanExecuteChanged(); // Enable/disable AddClientsCommand
            }
        }

        private int _clientWindowCount;

        public RelayCommand SelectFileCommand { get; set; }

        public RelayCommand LoadDataCommand { get; set; }

        public RelayCommand AddClientsCommand { get; set; }

        public RelayCommand StopAllClientsCommand { get; set; }

        public RelayCommand ReloadDataCommand { get; set; }

        public MainWindowViewModel()
        {
            SelectFileCommand = new RelayCommand(SelectFile, CanSelectFile);
            LoadDataCommand = new RelayCommand(LoadData, CanLoadData);
            AddClientsCommand = new RelayCommand(AddClients, CanAddClients);
            StopAllClientsCommand = new RelayCommand(StopAllClients, CanStopAllClients);
            ReloadDataCommand = new RelayCommand(ReloadData, CanReloadData);
        }

        private bool CanSelectFile(object obj)
        {
            return true;
        }

        private void SelectFile(object obj)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All files (*.*)|*.*"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                ExcelFilePath = openFileDialog.FileName;
            }
        }

        private void LoadData(object obj)
        {
            if (!string.IsNullOrEmpty(ExcelFilePath))
            {
                LoadExcelResponse = DataSource.Instance.SetFilePath(ExcelFilePath);
                TrafficURLs = DataSource.Instance.TrafficURLs;
                AddClientsCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanLoadData(object obj)
        {
            return !string.IsNullOrEmpty(ExcelFilePath);
        }

        private void AddClients(object obj)
        {
            if (_clientWindowCount > 0)
            {
                Console.WriteLine(_clientWindowCount);
                for (int i = 0; i < _clientWindowCount; i++)
                {
                    var trafficURL = DataSource.Instance.GetNextTrafficURL();
                    Console.WriteLine(trafficURL.ToString());
                    if (trafficURL != null)
                    {
                        var clientWindow = new ClientWindow()
                        {
                            Uid = Guid.NewGuid().ToString()
                        };
                        var clientViewModel = new ClientWindowViewModel(trafficURL, clientWindow.WebView);
                        _ = clientViewModel.InitializeWebView2Async();
                        clientWindow.DataContext = clientViewModel;
                        Clients.Add(new ClientProfile()
                        {
                            Uid = clientWindow.Uid,
                            Window = clientWindow,
                            ViewModel = clientViewModel
                        });
                        clientWindow.Show();
                    }
                    else
                    {
                        MessageBox.Show("Không có thêm Traffic URL nào để xử lý.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        break;
                    }
                }
                StopAllClientsCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanAddClients(object obj)
        {
            return _clientWindowCount > 0 && DataSource.Instance.TrafficURLs.Any(t => t.CurrentQuantity < t.RequireQuantity);
        }

        private void StopAllClients(object obj)
        {
            foreach (var client in Clients)
            {
                client.ViewModel.StopSession(); // Đóng tất cả cửa sổ client
            }
            Clients.Clear(); // Xóa tất cả client khỏi danh sách
        }

        private bool CanStopAllClients(object obj)
        {
            return Clients.Any();
        }

        private void ReloadData(object obj)
        {
            if (!string.IsNullOrEmpty(ExcelFilePath))
            {
                LoadExcelResponse = DataSource.Instance.SetFilePath(ExcelFilePath);
                TrafficURLs = DataSource.Instance.TrafficURLs;
            }
        }

        private bool CanReloadData(object obj)
        {
            return string.IsNullOrEmpty(ExcelFilePath);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
