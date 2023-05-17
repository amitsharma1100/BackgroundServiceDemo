using BackgroundServiceDemo.Web;

namespace BackgroundServiceDemo
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IRestService _service;
        private Timer? _timer = null;

        public TimedHostedService(ILogger<TimedHostedService> logger, IRestService service)
        {
            _logger = logger;
            _service = service;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            Console.Clear();
            _logger.LogInformation("**********Welcome to the exchange and stock rates portal**********.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                var count = Interlocked.Increment(ref executionCount);
                _logger.LogInformation("Please choose one of the following options-\n1. Exchange Rates\n2. Stock Prices\n3. Exit");
                var inputString = Console.ReadLine();
                if(Int32.TryParse(inputString, out int input))
                {
                    switch(input)
                    {
                        case 1:
                            {
                                GetExchangeRates();
                                break;
                            }

                        case 2:
                            {
                                GetStockPrices();
                                break;
                            }

                        case 3:
                            {
                                await StopAsync(CancellationToken.None);
                                break;
                            }

                        default:
                            {
                                _logger.LogInformation("Please provide a valid input.");
                                break;
                            };
                    }
                }
                else
                    _logger.LogInformation("Please provide numeric input only.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Timed Hosted Service exception encountered", ex);
            }
        }

        private async void GetExchangeRates()
        {
            var data = await _service.GetRealTimeExchangeRates();
            _logger.LogInformation($"Exchange rate at {DateTime.Now} - INR: {data.Rates["INR"]}");
        }

        private async void GetStockPrices()
        {
            _logger.LogInformation("Please enter the stock code");
            var stockCode = Console.ReadLine();

            var data = await _service.GetRealTimeStockPriceByName(stockCode, "US");
            _logger.LogInformation($"{stockCode}'s price is {data}");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
