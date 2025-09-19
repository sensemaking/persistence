using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using NodaTime;

namespace Sensemaking.Persistence.Dapper
{
    public class PeriodHandler : SqlMapper.TypeHandler<Period>
    {
        public override void SetValue(IDbDataParameter parameter, Period? period)
        {
            parameter.Value = period!.Days;

            if (parameter is SqlParameter sqlParameter)
                sqlParameter.SqlDbType = SqlDbType.Int;
        }

        public override Period Parse(object period)
        {
            if (period is int periodDays)
                return Period.FromDays(periodDays);

            throw new DataException("Cannot convert " + period.GetType() + " to NodaTime.Period");
        }
    }
}