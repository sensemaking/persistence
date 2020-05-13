using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Persistence.Dapper
{
    public class OffsetDateTimeHandler : SqlMapper.TypeHandler<OffsetDateTime>
    {
        public static readonly OffsetDateTimeHandler Default = new OffsetDateTimeHandler();
        private OffsetDateTimeHandler() { }

        public override void SetValue(IDbDataParameter parameter, OffsetDateTime value)
        {
            parameter.Value = value.ToDateTimeOffset();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.DateTimeOffset;
        }

        public override OffsetDateTime Parse(object value)
        {
            return value is DateTimeOffset dateTimeOffset
                ? OffsetDateTime.FromDateTimeOffset(dateTimeOffset)
                : throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(OffsetDateTime)}");
        }
    }
}