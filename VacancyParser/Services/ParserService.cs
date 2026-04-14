using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacancyParser.Models;
using VacancyParser.Parsers;

namespace VacancyParser.Services
{
    public class ParserService
    {
        private readonly IParser _amountParser;
        private readonly PhoneCheckService _phoneService;
        private readonly CsvService _csvService;

        public ParserService()
        {
            _amountParser = new AmountWorkParser();
            _phoneService = new PhoneCheckService();
            _csvService = new CsvService();
        }

        public async Task ParseVacanciesAsync()
        {
            var ads = await _amountParser.ParseAsync();

            var validAds = new List<JobAd>();

            foreach (var ad in ads)
            {
                if (ad.Phones == null || ad.Phones.Count == 0)
                    continue;

                bool isValid = true;

                foreach (var phone in ad.Phones)
                {
                    var exists = await _phoneService.CheckAsync(phone);

                    if (!exists)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    validAds.Add(ad);
                }
            }
            
            _csvService.Save(validAds);
        }
    }
}
