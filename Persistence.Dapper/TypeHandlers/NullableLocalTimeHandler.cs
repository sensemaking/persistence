using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Persistence.Dapper
{
    public class NullableLocalTimeHandler : SqlMapper.TypeHandler<LocalTime?>
    {
        public static readonly NullableLocalTimeHandler Default = new NullableLocalTimeHandler();
        private NullableLocalTimeHandler() {} 

        public override void SetValue(IDbDataParameter parameter, LocalTime? value)
        {
            parameter.Value = value.HasValue
                ? TimeSpan.FromTicks(value.Value.TickOfDay)
                : default(TimeSpan?);

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.Time;
        }

        public override LocalTime? Parse(object value)
        {
            return value switch
            {
                DBNull _ => null,
                TimeSpan timeSpan => LocalTime.FromTicksSinceMidnight(timeSpan.Ticks),
                DateTime dateTime => LocalTime.FromTicksSinceMidnight(dateTime.TimeOfDay.Ticks),
                _ => throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(LocalTime?)}")
            };
        }
    }}