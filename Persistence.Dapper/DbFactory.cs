using System;
using System.Collections.Generic;
using System.Linq;

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

        public static IDb Create(string connectionString, IEnumerable<string> dbServers, string serverPlaceholder, MultipleDb.QueryOptions options = MultipleDb.QueryOptions.One)
        {
            Validation.BasedOn(errors =>
            {
                if (connectionString.IsNullOrEmpty())
                    errors.Add("A connection string is required.");
                else if (!connectionString.Contains(serverPlaceholder ?? string.Empty))
                        errors.Add("The connection string does not include the server placeholder.");
            
                if(dbServers == null || dbServers.None())
                    errors.Add("At least one database server is required.");

                if(serverPlaceholder.IsNullOrEmpty())
                    errors.Add("A server placeholder is required.");
            });

            return dbServers.Count() == 1 ? new Db(connectionString.Replace(serverPlaceholder, dbServers.Single()))
                : new MultipleDb(dbServers.Select(x => new Db(connectionString.Replace(serverPlaceholder, x))), options) as IDb;
        }
    }
}