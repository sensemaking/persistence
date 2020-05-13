using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Dapper
{
    public class LocalDateTimeHandler : SqlMapper.TypeHandler<LocalDateTime>
    {
        public static readonly LocalDateTimeHandler Default = new LocalDateTimeHandler();
        private LocalDateTimeHandler() { }

        public override void SetValue(IDbDataParameter parameter, LocalDateTime value)
        {
            parameter.Value = value.ToDateTimeUnspecified();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.DateTime2;
        }

        public override LocalDateTime Parse(object value)
        {
            return value is DateTime dateTime 
                ? LocalDateTime.FromDateTime(dateTime) 
                : throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(LocalDateTime)}");
        }
    }
}