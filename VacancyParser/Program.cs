
using VacancyParser.Services;

namespace VacancyParser
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Parser started...");
            
            var parserService = new ParserService();

            while (true)
            {
                try
                {
                    await parserService.ParseVacanciesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }

                Console.WriteLine("Waiting 1 hour");
                await Task.Delay(TimeSpan.FromHours(1));
            }

        }
    }
}