using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Dapper
{
    public class InstantHandler : SqlMapper.TypeHandler<Instant>
    {
        public static readonly InstantHandler Default = new InstantHandler();
        private InstantHandler() { }

        public override void SetValue(IDbDataParameter parameter, Instant value)
        {
            parameter.Value = value.ToDateTimeUtc();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.DateTime2;
        }

        public override Instant Parse(object value)
        {
            return value switch
            {
                DateTimeOffset dateTimeOffset => Instant.FromDateTimeOffset(dateTimeOffset),
                DateTime dateTime => Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
                _ => throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(Instant)}")
            };
        }
    }
}