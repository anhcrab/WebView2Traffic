namespace WebView2Traffic.Models
{
    public class TrafficURL
    {
        public int ID { get; set; }
        public string Keyword { get; set; }
        public string URL { get; set; }
        public int Rank { get; set; }
        public int RequireQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string Type { get; set; }
        public string Mobile { get; set; }

        // Override ToString để dễ debug (tùy chọn)
        public override string ToString()
        {
            return $"#{ID} | Traffic {Type} | Keyword: {Keyword} | URL: {URL} | Tiến độ hoàn thành: {CurrentQuantity}/{RequireQuantity}";
        }
    }
}
