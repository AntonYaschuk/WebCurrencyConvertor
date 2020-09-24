using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;

namespace CurrencyApp.Models
{
    public class CurrencyService : BackgroundService
    {
        private readonly IMemoryCache memoryCache;

        public CurrencyService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //если не указать культуру, могут пропадать точки в xml файле
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ua-UA"); 

                   
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    //XML файлик от нбу
                    XDocument xml = XDocument.Load("https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange");

                    //конвертор
                    CurrencyConverter currencyConverter = new CurrencyConverter();
                    currencyConverter.USD = Convert.ToDecimal(xml.Elements("exchange").Elements("currency").FirstOrDefault(x => x.Element("r030").Value == "840").Elements("rate").FirstOrDefault().Value);
                    currencyConverter.EUR = Convert.ToDecimal(xml.Elements("exchange").Elements("currency").FirstOrDefault(x => x.Element("r030").Value == "978").Elements("rate").FirstOrDefault().Value);
                    

                    memoryCache.Set("key_currency", currencyConverter, TimeSpan.FromMinutes(1440));
                }
                catch (Exception e)
                {
                    
                }

                //обновление раз в час 
                await Task.Delay(3600000, stoppingToken);
            }
        }
    }
}
