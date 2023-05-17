using BackgroundServiceDemo.Models;

namespace BackgroundServiceDemo.Web
{
    public interface IRestService
    {
        Task<string> GetRealTimeStockPriceByName(string name, string exchange);
        Task<ExchangeRatesResponse> GetRealTimeExchangeRates();
    }
}
