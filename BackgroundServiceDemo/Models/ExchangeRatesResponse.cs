namespace BackgroundServiceDemo.Models
{
    public class ExchangeRatesResponse
    {
        public string Disclaimer { get; set; }
        public string Base { get; set; }
        public Dictionary<string, double> Rates { get; set; }
    }
}
