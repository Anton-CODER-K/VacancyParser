using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyParser.Models
{
    public class JobAd
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Phones { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
    }
}
