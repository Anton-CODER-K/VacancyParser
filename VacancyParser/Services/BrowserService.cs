using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyParser.Services
{
    public class BrowserService
    {
        public async Task<IBrowser> GetBrowserAsync()
        {
            var playwright = await Playwright.CreateAsync();

            return await playwright.Chromium.LaunchAsync(new()
            {
                Headless = false,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });
        }
    }
}
