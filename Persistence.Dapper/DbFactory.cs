using System;

namespace Sensemaking.Persistence.Dapper
{
    public static class DbFactory
    {
        public static IDb Create(string connectionString)
        {
            Validation.BasedOn(errors =>
            {
                if(connectionString.IsNullOrEmpty())
                  errors.Add("A connection string is required.");
            });

            return new Db(connectionString);
        }
    }
}