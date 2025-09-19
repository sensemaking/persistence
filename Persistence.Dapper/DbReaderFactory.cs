using System;
using Fdb.Rx.Persistence.Security;

namespace Fdb.Rx.Persistence.Dapper
{
    public static class DbReaderFactory
    {
        public static IQueryDb Create(string connectionString, string dbServer, string serverPlaceholder) 
        {
            return Create(new NoAccessTokens(), connectionString, dbServer, serverPlaceholder);
        }

        public static IQueryDb Create(string connectionString)
        {
            Validation.BasedOn(errors =>
            {
                if(connectionString.IsNullOrEmpty())
                  errors.Add("A connection string is required.");
            });

            return new DbReader(connectionString);
        }

        public static IQueryDb Create(IProvideAccessTokens tokenProvider, string connectionString, string dbServer, string serverPlaceholder)
        {
            Validation.BasedOn(errors =>
            {
                if (connectionString.IsNullOrEmpty())
                    errors.Add("A connection string is required.");
                else if (!connectionString.Contains(serverPlaceholder ?? string.Empty))
                    errors.Add("The connection string does not include the server placeholder.");
            
                if(dbServer.IsNullOrEmpty())
                    errors.Add("A database server is required.");

                if(serverPlaceholder!.IsNullOrEmpty())
                    errors.Add("A server placeholder is required.");
            });

            return new DbReader(connectionString.Replace(serverPlaceholder, dbServer), tokenProvider);
        }
    }
}