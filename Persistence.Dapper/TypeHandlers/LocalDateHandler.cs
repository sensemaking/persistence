using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Persistence.Dapper
{
    public class LocalDateHandler : SqlMapper.TypeHandler<LocalDate>
    {
        public static readonly LocalDateHandler Default = new LocalDateHandler();
        private LocalDateHandler() { }

        public override void SetValue(IDbDataParameter parameter, LocalDate value)
        {
            parameter.Value = value.AtMidnight().ToDateTimeUnspecified();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.Date;
        }

        public override LocalDate Parse(object value)
        {
            return value is DateTime dateTime 
                ? LocalDateTime.FromDateTime(dateTime).Date 
                : throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(LocalDate)}");
        }
    }}