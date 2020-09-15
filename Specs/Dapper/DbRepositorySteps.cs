using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Sensemaking.Bdd;
using Sensemaking.Bdd.Dapper;
using Sensemaking.Domain;
using Sensemaking.Domain.Dapper;

namespace Sensemaking.Dapper.Specs
{
    public partial class DbRepositorySpecs : DbSpecification
    {
        private const string aggregate_table_name = "DbRepositoryTest";
        private const string publication_table_suffix = "AsPublished";
        private AnAggregate the_aggregate;
        private AnAggregate another_aggregate;
        private AnAggregate the_result;

        private DbRepository the_repository;
        private string aggregate_content = "I am some of the most interesting content ever scribed.";

        protected override void before_all()
        {
            base.before_all();
            using (var con = new SqlConnection(Startup.Database.connection_string))
            {
                con.Execute($"CREATE TABLE {aggregate_table_name} (Id varchar(200), Document varchar(200))");
                con.Execute($"CREATE TABLE {aggregate_table_name}{publication_table_suffix} (Id varchar(200), Document varchar(200))");
            }
        }

        protected override void after_all()
        {
            using (var con = new SqlConnection(Startup.Database.connection_string))
            {
                con.Execute($"DROP TABLE {aggregate_table_name}");
                con.Execute($"DROP TABLE {aggregate_table_name}{publication_table_suffix}");
            }
            base.after_all();
        }

        protected override void before_each()
        {
            base.before_each();
            another_aggregate = new AnAggregate(Guid.NewGuid().ToString(), aggregate_content + "more text");
            the_aggregate = new AnAggregate(Guid.NewGuid().ToString(), aggregate_content);
            the_repository = new DbRepository(new Db(Startup.Database.connection_string));
            the_repository.Register<AnAggregate>(aggregate_table_name, null);
            the_result = null;
        }

        private void an_aggregate() { }

        private void it_is_saved()
        {
            the_repository.SaveAsync(the_aggregate).Wait();
        }

        private void it_is_changed()
        {
            the_aggregate.Content = Guid.NewGuid().ToString();
            the_repository.SaveAsync(the_aggregate).Wait();
        }

        private void it_is_deleted()
        {
            the_repository.DeleteAsync(the_aggregate).Wait();
        }

        private void it_is_published()
        {
            the_repository.PublishAsync(the_aggregate).Wait();
        }

        private void it_is_published2()
        {
            the_repository.PublishAsync(another_aggregate).Wait();
        } 

        private void it_is_un_published()
        {
            the_repository.UnpublishAsync(another_aggregate).Wait();
        }

        private void getting_the_aggregate()
        {
            the_result = the_repository.GetAsync<AnAggregate>(the_aggregate.Id).Result;
        }

        private void getting_all_published_versions()
        {
            the_result = the_repository.GetAllPublishedAsync<AnAggregate>().Result.SingleOrDefault();
        }

        private void the_aggregate_is_retrieved()
        {
            the_result.Content.should_be(aggregate_content);
        }

        private void the_aggregate_is_not_retrieved()
        {
            the_result.should_be_null();
        }

        private void the_published_version_is_retrieved()
        {
            the_result.Content.should_be(aggregate_content);
        }

        private void the_published_version_is_not_retrieved()
        {
            the_result.should_be_null();
        }
    }

    internal class AnAggregate : PublishableAggregate<string>
    {
        public AnAggregate(string id, string content) : base(id)
        {
            Content = content;
        }

        public string Content { get; set; }
    }
}