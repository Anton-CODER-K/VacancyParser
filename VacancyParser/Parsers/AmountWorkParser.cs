using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VacancyParser.Models;
using VacancyParser.Services;
using VacancyParser.Utils;
using Microsoft.Extensions.Logging;
using VacancyParser.Logger;

namespace VacancyParser.Parsers
{
    public class AmountWorkParser : IParser
    {
        private readonly TextCleaner _textCleaner;
        private readonly BrowserService _browserService;
        private readonly ContentFilterService _filterService;

        private const string baseUrl = "https://amountwork.com";

        public AmountWorkParser()
        {
            _browserService = new BrowserService();
            _textCleaner = new TextCleaner();
            _filterService = new ContentFilterService();
        }

        public async Task<List<JobAd>> ParseAsync()
        {
            FileLogger.Info("Starting AmountWorkParser...");
            var result = new List<JobAd>();

            var browser = await _browserService.GetBrowserAsync();
            var context = await browser.NewContextAsync(new()
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
                DeviceScaleFactor = 1,
                IsMobile = false,
                HasTouch = false,
                Locale = "uk-UA",
                TimezoneId = "Europe/Kyiv"
            });
            await context.AddInitScriptAsync(@"
                Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                window.chrome = { runtime: {} };
                Object.defineProperty(navigator, 'languages', { get: () => ['uk-UA', 'uk', 'en-US', 'en'] });
                Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] });
            ");
            context.SetDefaultTimeout(60000);
            context.SetDefaultNavigationTimeout(60000);

            int pageNumber = 1;

            while (true)
            {
                try
                {
                    var page = await context.NewPageAsync();

                    await page.GotoAsync($"https://amountwork.com/ua/rabota/ssha/voditel/?page={pageNumber}");
                    FileLogger.Info($"Vacancy page parsing {pageNumber}");

                    var pageLinks = await page.QuerySelectorAllAsync(".pagination .page-item a");

                    int maxPage = 1;

                    foreach (var link in pageLinks)
                    {
                        var textNextPage = await link.InnerTextAsync();

                        if (int.TryParse(textNextPage, out int pageNum))
                        {
                            if (pageNum > maxPage)
                                maxPage = pageNum;
                        }
                    }

                    Console.WriteLine($"Max page: {maxPage}");


                    pageNumber++;
                    var vacancies = await page.QuerySelectorAllAsync(".vacancies-list-item");

                    var text = await page.InnerTextAsync("body");
                    IElementHandle vacancyDetals;

                    if (pageNumber > maxPage)
                    {
                        Console.WriteLine("No more pages");
                        break;
                    }

                    Console.WriteLine(vacancies.Count);

                    foreach (var vacancy in vacancies)
                    {

                        var phones = new List<string>();
                        string description = string.Empty;
                        string location = string.Empty;
                        string title = string.Empty;

                        var titleEl = await vacancy.QuerySelectorAsync("h3 a");
                        if (titleEl == null) continue;

                        title = await titleEl.InnerTextAsync();

                        var href = await titleEl.GetAttributeAsync("href");
                        var fullUrl = baseUrl + href;
                        var pageVacancies = await browser.NewPageAsync();
                        await pageVacancies.GotoAsync(fullUrl);

                        vacancyDetals = await pageVacancies.QuerySelectorAsync(".vacancy-container");
                        if (vacancyDetals == null) continue;

                        var descriptionEl = await vacancyDetals.QuerySelectorAsync(".vacancy-description");
                        description = descriptionEl != null ? await descriptionEl.InnerTextAsync() : string.Empty;

                        if (_filterService.ContainsBlockedWords(title) || _filterService.ContainsBlockedWords(description))
                        {
                            await pageVacancies.CloseAsync();
                            continue;
                        }

                        var descriptionPhones = _textCleaner.ExtractCleaner(description);
                        foreach (var p in descriptionPhones)
                            Console.WriteLine($"Number to description: " + p);

                        phones.AddRange(descriptionPhones);



                        var InfoCompany = await pageVacancies.QuerySelectorAllAsync(
                            ".company-info-contact, .company-info-country"
                        );

                        foreach (var company in InfoCompany)
                        {
                            var icon = await company.QuerySelectorAsync("i");
                            if (icon == null) continue;

                            var className = await icon.GetAttributeAsync("class");
                            if (className == null) continue;


                            var valueEl = await company.QuerySelectorAsync("span.second");
                            if (valueEl == null) continue;

                            var value = await valueEl.InnerTextAsync();

                            if (className.Contains("fa-phone-alt"))
                            {
                                phones.AddRange(_textCleaner.ExtractCleaner(value));
                                Console.WriteLine($"Number to Vavansy" + value);
                            }
                            else if (className.Contains("marker-alt"))
                            {
                                location = value;
                            }

                        }

                        phones = phones.SelectMany(p => _textCleaner.ExtractCleaner(p)).Distinct().ToList();

                        Console.WriteLine("Всі телофони які додаються і переходять в інший сервіс");
                        foreach (var p in phones)
                        {
                            Console.WriteLine(p);
                        }

                        result.Add(new JobAd
                        {
                            Title = title,
                            Description = description,
                            Phones = phones,
                            Email = string.Empty,
                            Location = location,
                        });




                        await pageVacancies.CloseAsync();


                        Console.WriteLine("------------------------------------------------------");
                    }
                    await page.CloseAsync();
                }
                catch (PlaywrightException ex)
                {
                    Console.WriteLine($"Playwright exception: {ex.Message}");
                    FileLogger.Warn($"Vacancy page parsing error: {ex.Message}");
                    break;
                }
            }
            
           
            Console.ReadLine();

            await browser.CloseAsync();

            return result;
        }
    }
}
