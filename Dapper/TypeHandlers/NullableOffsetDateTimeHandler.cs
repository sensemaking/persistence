using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Dapper
{
    public class NullableOffsetDateTimeHandler : SqlMapper.TypeHandler<OffsetDateTime?>
    {
        public static readonly NullableOffsetDateTimeHandler Default = new NullableOffsetDateTimeHandler();
        private NullableOffsetDateTimeHandler() {}

        public override void SetValue(IDbDataParameter parameter, OffsetDateTime? value)
        {
            parameter.Value = value?.ToDateTimeOffset();

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.DateTimeOffset;
        }

        public override OffsetDateTime? Parse(object value)
        {
            return value switch
            {
                DBNull _ => null,
                DateTimeOffset dateTimeOffset => OffsetDateTime.FromDateTimeOffset(dateTimeOffset),
                _ => throw new ArgumentException($"Cannot convert {value.GetType()} to {typeof(OffsetDateTime?)}")
            };
        }
    }}