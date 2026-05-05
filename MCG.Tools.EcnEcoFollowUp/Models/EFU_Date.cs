using System.Globalization;
using System.Text.RegularExpressions;

namespace MCG.Tools.EcnEcoFollowUp.Models
{
    public class EFU_Date
    {
        private string _CompletePdmDate;
        public string CompletePdmDate
        {
            get
            {
                return _CompletePdmDate;
            }
            set
            {
                _CompletePdmDate = value;
                SplitDate();
            }
        }

        private string _StandardDate;
        public string StandardDate
        {
            get
            {
                return _StandardDate;
            }
            set
            {
                _StandardDate = value;
                SplitStandardDate();
            }
        }

        public string YearStr { get; set; }
        public string MonthStr { get; set; }
        public string DayStr { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }

        private void SplitDate()
        {
            try
            {
                if (CompletePdmDate != null && CompletePdmDate.Length > 9)
                {
                    YearStr = CompletePdmDate.Substring(0, 4);
                    MonthStr = CompletePdmDate.Substring(6, 2);
                    DayStr = CompletePdmDate.Substring(9, 2);

                    Year = Convert.ToInt32(YearStr);
                    Month = Convert.ToInt32(MonthStr);
                    Day = Convert.ToInt32(DayStr);
                }
            }
            catch (Exception)
            {
            }
        }

        private void SplitStandardDate()
        {
            try
            {
                Regex DataRegex = new Regex(@"^\d\d-\d\d-\d\d\d\d$");
                if (DataRegex.IsMatch(StandardDate))
                {
                    YearStr = StandardDate.Substring(6, 4);
                    MonthStr = StandardDate.Substring(3, 2);
                    DayStr = StandardDate.Substring(0, 2);

                    Year = Convert.ToInt32(YearStr);
                    Month = Convert.ToInt32(MonthStr);
                    Day = Convert.ToInt32(DayStr);
                }
                else if (StandardDate != null && StandardDate.Length > 9)
                {
                    YearStr = StandardDate.Substring(6, 4);
                    MonthStr = StandardDate.Substring(3, 2);
                    DayStr = StandardDate.Substring(0, 2);

                    Year = Convert.ToInt32(YearStr);
                    Month = Convert.ToInt32(MonthStr);
                    Day = Convert.ToInt32(DayStr);
                }
            }
            catch (Exception)
            {
            }
        }

        public string GetMonth()
        {
            try
            {
                return $"{MonthStr}/{YearStr.Substring(YearStr.Length - 2, 2)}";
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string GetWeek()
        {
            try
            {
                int WeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(new DateTime(Year, Month, Day), CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
                if (WeekNumber < 10)
                    return $"W0{WeekNumber}/{YearStr.Substring(YearStr.Length - 2, 2)}";
                else
                    return $"W{WeekNumber}/{YearStr.Substring(YearStr.Length - 2, 2)}";
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DateTime GetDate()
        {
            try
            {
                return new DateTime(Year, Month, Day);
            }
            catch (Exception)
            {
                return default(DateTime);
            }
        }
    }
}