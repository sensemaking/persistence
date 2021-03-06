using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Dapper
{
    public class LocalTimeHandler : SqlMapper.TypeHandler<LocalTime>
    {
        public static readonly LocalTimeHandler Default = new LocalTimeHandler();
        private LocalTimeHandler() { }

        public override void SetValue(IDbDataParameter parameter, LocalTime value)
        {
            parameter.Value = TimeSpan.FromTicks(value.TickOfDay);

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.Time;
        }

        public override LocalTime Parse(object value)
        {
            return value switch
            {
                TimeSpan timeSpan => LocalTime.FromTicksSinceMidnight(timeSpan.Ticks),
                DateTime dateTime => LocalTime.FromTicksSinceMidnight(dateTime.TimeOfDay.Ticks),
                _ => throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(LocalTime)}")
            };
        }
    }
}