using Dapper;

namespace Sensemaking.NodaTime.Dapper
{
    public static class NodaTimeForDapper
    {
        public static void Register()
        {
            SqlMapper.AddTypeHandler(InstantHandler.Default);
            SqlMapper.AddTypeHandler(LocalDateHandler.Default);
            SqlMapper.AddTypeHandler(LocalDateTimeHandler.Default);
            SqlMapper.AddTypeHandler(LocalTimeHandler.Default);
            SqlMapper.AddTypeHandler(OffsetDateTimeHandler.Default);
        }
    }
}
