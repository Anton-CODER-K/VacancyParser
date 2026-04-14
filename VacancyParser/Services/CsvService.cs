using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VacancyParser.Models;

namespace VacancyParser.Services
{
    public class CsvService
    {
        private readonly string _filePath = "Output/leads.csv";

        public void Save(List<JobAd> ads)
        {
            Directory.CreateDirectory("Output");

            bool fileExists = File.Exists(_filePath);

            using (var writer = new StreamWriter(_filePath, true, new UTF8Encoding(true))) 
            {
                if (!fileExists)
                {
                    writer.WriteLine("Date;Title;Description;Phones;Email;Location");
                }

                foreach (var ad in ads)
                {
                    var phones = string.Join("|", ad.Phones);
                    var line = $"{DateTime.Now:yyyy-MM-dd HH:mm};" +
                           $"{Escape(ad.Title)};" +
                           $"{Escape(ad.Description)};" +
                           $"{Escape(phones)};" +
                           $"{Escape(ad.Email)};" +
                           $"{Escape(ad.Location)}";

                    writer.WriteLine(line);
                }

            }
        }

        private string Escape(string value)
        {
            if (value == null) return string.Empty;
            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }
    }
}
