using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardGameGeekLike.Utility
{
    public static class DateGenerator
    {  
        public static int year;
        public static int month;
        public static int day;

        public static DateOnly GetRandomDate(int maxYear)
        {
            var random = new Random();

            int year = random.Next(1945, maxYear+1);

            int month = random.Next(1, 13);
            
            var day = month switch
            {
                2 => random.Next(1, 29), // February (ignoring leap years for simplicity)
                4 or 6 or 9 or 11 => random.Next(1, 31), // April, June, September, November
                _ => random.Next(1, 32) // Months with 31 days
            };

            string date_string = $"{day:00}/{month:00}/{year}";

            var parsedDate = DateOnly.ParseExact(date_string,"dd/MM/yyyy");

            return parsedDate;
        }   
    }
}