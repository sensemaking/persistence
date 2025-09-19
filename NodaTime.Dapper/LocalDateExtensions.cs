using System.Globalization;
using NodaTime;

namespace Fdb.Rx.Persistence.Dapper
{
    public static class LocalDateExtensions
    {
        public static string AsStringForDateOnUdt(this LocalDate date)
        {
            return date.ToString("yyyyMMdd", DateTimeFormatInfo.InvariantInfo);
        }
    }
}