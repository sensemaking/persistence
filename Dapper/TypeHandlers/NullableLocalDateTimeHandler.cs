using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Dapper
{
    public class NullableLocalDateTimeHandler : SqlMapper.TypeHandler<LocalDateTime?>
    {
        public static readonly NullableLocalDateTimeHandler Default = new NullableLocalDateTimeHandler();
        private NullableLocalDateTimeHandler() {} 

        public override void SetValue(IDbDataParameter parameter, LocalDateTime? value)
        {
            parameter.Value = value?.ToDateTimeUnspecified();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.DateTime2;
        }

        public override LocalDateTime? Parse(object value)
        {
            return value switch
            {
                DBNull _ => null,
                DateTime dateTime => LocalDateTime.FromDateTime(dateTime),
                _ => throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(LocalDateTime?)}")
            };
        }
    }}