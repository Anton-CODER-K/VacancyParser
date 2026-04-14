using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace VacancyParser.Services
{
    public class PhoneCheckService
    {
        private readonly HttpClient _http = new HttpClient();

        public async Task<bool> CheckAsync(string phone)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(
                    "http://localhost:5296/check",
                    phone
                );

                if (response.StatusCode == HttpStatusCode.Created)
                    return true;

                if (response.StatusCode == HttpStatusCode.Conflict)
                    return false;

                if (response.StatusCode == HttpStatusCode.BadRequest)
                    return false;

                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Server error: {error}");

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");

                await Task.Delay(5000);

                return false;
            }
        }
    }
}
