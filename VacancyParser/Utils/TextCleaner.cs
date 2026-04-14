using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace VacancyParser.Utils
{
    internal class TextCleaner
    {
        public List<string> ExtractCleaner(string description)
        {

            if (string.IsNullOrWhiteSpace(description))
                return new List<string>();

            var matches = Regex.Matches(description, @"(\+1\s?)?(\(?\d{3}\)?[\s\-\.]?)\d{3}[\s\-\.]?\d{4}");

            

            var phones = matches
                .Select(m => CleanRawPhone(m.Value))
                .Where(p => p.Length == 12) 
                .Distinct()
                .ToList();

            

            return phones;
        }

        private string CleanRawPhone(string phone)
        {
            phone = Regex.Replace(phone, @"[^\d+]", "");

            if (!phone.StartsWith("+") && phone.Length == 10)
                phone = "+1" + phone;

            if (phone.StartsWith("1") && phone.Length == 11)
                phone = "+" + phone;

            return phone;
        }
    }
}
