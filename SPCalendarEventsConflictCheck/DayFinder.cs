using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAPI.Model
{
    public class DayFinder
    {
        public static int FindDay(int year, int month, DayOfWeek day, int occurance)
        {

            if (occurance <= 0 || occurance > 5)
                throw new Exception("Occurance is invalid");

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            //Substract first day of the month with the required day of the week 
            var daysneeded = (int)day - (int)firstDayOfMonth.DayOfWeek;
            //if it is less than zero we need to get the next week day (add 7 days)
            if (daysneeded < 0) daysneeded = daysneeded + 7;
            //DayOfWeek is zero index based; multiply by the Occurance to get the day
            var resultedDay = (daysneeded + 1) + (7 * (occurance - 1));

            if (resultedDay > (firstDayOfMonth.AddMonths(1) - firstDayOfMonth).Days)
                throw new Exception(String.Format("No {0} occurance(s) of {1} in the required month", occurance, day.ToString()));

            return resultedDay;
        }
    }

}
