using NUnit.Framework;

namespace Sensemaking.Dapper.Specs
{
    [SetUpFixture]
    public class Startup
    {
        private const string db_server = @"(localdb)\MSSQLLocalDB";

        internal class Database
        {
            internal static readonly string connection_string = $@"Server={db_server};Integrated Security=true;";
        }        
    }
}