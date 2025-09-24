using System;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NodaTime;

namespace Sensemaking.NodaTime.Dapper;

public class OffsetDateHandler : SqlMapper.TypeHandler<OffsetDate>
    {
        private OffsetDateHandler()
        {
        }

        public static readonly OffsetDateHandler Default = new();

        public override void SetValue(IDbDataParameter parameter, OffsetDate date)
        {
            parameter.Value = date.At(new LocalTime());

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.DateTimeOffset;
        }

        public override OffsetDate Parse(object value)
        {
            if (value is DateTimeOffset dateTimeOffset)
                return OffsetDateTime.FromDateTimeOffset(dateTimeOffset).ToOffsetDate();

            throw new DataException("Cannot convert " + value.GetType() + " to NodaTime.OffsetDateTime");
        }
    }