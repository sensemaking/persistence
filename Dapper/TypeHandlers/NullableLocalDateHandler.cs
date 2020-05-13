using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Dapper
{
    public class NullableLocalDateHandler : SqlMapper.TypeHandler<LocalDate?>
    {
        public static readonly NullableLocalDateHandler Default = new NullableLocalDateHandler();
        private NullableLocalDateHandler() {} 

        public override void SetValue(IDbDataParameter parameter, LocalDate? value)
        {
            parameter.Value = value?.AtMidnight().ToDateTimeUnspecified();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.Date;
        }

        public override LocalDate? Parse(object value)
        {
            return value switch
            {
                DBNull _ => null,
                DateTime dateTime => LocalDateTime.FromDateTime(dateTime).Date,
                _ => throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(LocalDate?)}")
            };
        }
    }}