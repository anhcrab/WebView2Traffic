using OfficeOpenXml;
using System.IO;
using WebView2Traffic.Models;

namespace WebView2Traffic.Data
{
    public sealed class DataSource
    {
        private static DataSource? _instance;
        private static readonly object _lock = new();
        private string? _excelFilePath;
        private const string TrafficSheetName = "Traffic URLs";

        public static DataSource Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new DataSource();
                    }
                    return _instance;
                }
            }
        }

        private DataSource()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Traffic");
        }

        public string SetFilePath(string filePath)
        {
            _excelFilePath = filePath;
            return LoadDataFromExcel();
        }

        private readonly List<TrafficURL> _trafficURLs = [];
        public List<TrafficURL> TrafficURLs => _trafficURLs;

        private string LoadDataFromExcel()
        {
            _trafficURLs.Clear();
            if (string.IsNullOrEmpty(_excelFilePath) || !File.Exists(_excelFilePath))
            {
                return "File not exist.";
            }

            try
            {
                using var package = new ExcelPackage(new FileInfo(_excelFilePath));
                var worksheet = package.Workbook.Worksheets[TrafficSheetName];
                if (worksheet == null)
                {
                    return "Sheet not have name \"Traffic URLs\" or not exist.";
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1) // No data rows
                {
                    return "File has no data to load.";
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    _trafficURLs.Add(new TrafficURL
                    {
                        ID = GetIntValue(worksheet.Cells[row, 1].Value),
                        Keyword = GetStringValue(worksheet.Cells[row, 2].Value),
                        URL = GetStringValue(worksheet.Cells[row, 3].Value),
                        Rank = GetIntValue(worksheet.Cells[row, 4].Value),
                        RequireQuantity = GetIntValue(worksheet.Cells[row, 5].Value),
                        CurrentQuantity = GetIntValue(worksheet.Cells[row, 6].Value),
                        Type = GetStringValue(worksheet.Cells[row, 7].Value),
                        Mobile = GetStringValue(worksheet.Cells[row, 8].Value)
                    });
                }
                System.Diagnostics.Debug.WriteLine($"Data loaded successfully from '{TrafficSheetName}'. {_trafficURLs.Count} items.");
                return $"Data loaded successfully from '{TrafficSheetName}'. {_trafficURLs.Count} items.";
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error loading data from Excel: {ex.Message}");
                return $"Error loading data from Excel: {ex.Message}";
            }
        }

        public TrafficURL? GetNextTrafficURL(int id = 1)
        {
            var tmpId = id;
            if (tmpId >= _trafficURLs.Count + 1) tmpId = 1;
            var traffic = _trafficURLs.FirstOrDefault(traffic =>
            {
                if (traffic.ID == tmpId && traffic.CurrentQuantity <= traffic.RequireQuantity) return true;
                return false;
            })!;
            while (traffic == null && tmpId <= _trafficURLs.Count)
        {
                if (tmpId == id - 1) break;
                traffic = _trafficURLs.FirstOrDefault(traffic =>
            {
                    if (traffic.ID == tmpId && traffic.CurrentQuantity <= traffic.RequireQuantity) return true;
                return false;
            })!;
                if (tmpId >= _trafficURLs.Count + 1) tmpId = 1;
            }
            return traffic;
        }

        public void UpdateTrafficURL(TrafficURL trafficURL)
        {
            var existing = _trafficURLs.FirstOrDefault(t => t.ID == trafficURL.ID);
            if (existing != null)
            {
                existing.CurrentQuantity = trafficURL.CurrentQuantity;
                SaveDataToExcel();
            }
        }

        private void SaveDataToExcel()
        {
            if (string.IsNullOrEmpty(_excelFilePath))
            {
                return;
            }

            try
            {
                using var package = new ExcelPackage(new FileInfo(_excelFilePath));
                var worksheet = package.Workbook.Worksheets[TrafficSheetName];
                if (worksheet == null)
                {
                    worksheet = package.Workbook.Worksheets.Add(TrafficSheetName);
                    // Add headers if the sheet was just created (optional)
                    worksheet.Cells[1, 1].Value = "ID";
                    worksheet.Cells[1, 2].Value = "Keyword";
                    worksheet.Cells[1, 3].Value = "URL";
                    worksheet.Cells[1, 4].Value = "Rank";
                    worksheet.Cells[1, 5].Value = "Require Quantity";
                    worksheet.Cells[1, 6].Value = "Current Quantity";
                    worksheet.Cells[1, 7].Value = "Type";
                    worksheet.Cells[1, 8].Value = "Mobile";
                }

                for (int i = 0; i < _trafficURLs.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = _trafficURLs[i].ID;
                    worksheet.Cells[i + 2, 2].Value = _trafficURLs[i].Keyword;
                    worksheet.Cells[i + 2, 3].Value = _trafficURLs[i].URL;
                    worksheet.Cells[i + 2, 4].Value = _trafficURLs[i].Rank;
                    worksheet.Cells[i + 2, 5].Value = _trafficURLs[i].RequireQuantity;
                    worksheet.Cells[i + 2, 6].Value = _trafficURLs[i].CurrentQuantity;
                    worksheet.Cells[i + 2, 7].Value = _trafficURLs[i].Type;
                    worksheet.Cells[i + 2, 8].Value = _trafficURLs[i].Mobile;
                }

                package.Save();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error saving data to Excel: {ex.Message}");
            }
        }

        private static int GetIntValue(object value)
        {
            if (value != null && int.TryParse(value.ToString(), out int result))
            {
                return result;
            }
            return 0; // Default value if conversion fails
        }

        private static string GetStringValue(object value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}
