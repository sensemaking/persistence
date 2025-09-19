using System;
using Fdb.Rx.Domain;
using Fdb.Rx.Test.Domain;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Domain;

public partial class SubstituteRepositoriesSpecs
{
    private static readonly User charlotte_bronte = new User(Guid.NewGuid(), "Charlotte Bronte");
    private SubstituteRepository the_repository;
    private SubstituteContentRepository the_content_repository;
    private StubContent stub_aggregate;
    private StubContent another_stub_aggregate;
    private StubContent a_third_stub_aggregate;
    private StubContent returned_stub_aggregate;
    private StubContent returned_stub_content;
    private StubContent[] returned_stub_aggregates;
    private StubContent[] returned_stub_contents;
    private StubContent saved_aggregate;
    private StubContent saved_content;
    private StubContent deleted_aggregate;
    private StubContent deleted_content;

    protected override void before_each()
    {
        base.before_each();
        stub_aggregate = new StubContent("Some content", charlotte_bronte);
        another_stub_aggregate = new StubContent("Different content", charlotte_bronte);
        a_third_stub_aggregate = new StubContent("More different content", charlotte_bronte);
        returned_stub_aggregate = default;
        returned_stub_content = default;
        the_repository = RepositoryBuilder.For.Substitutes().Get().Repository as SubstituteRepository;
        the_content_repository = RepositoryBuilder.For.Substitutes().Get().Content as SubstituteContentRepository;
        saved_aggregate = default;
        saved_content = default;
        deleted_aggregate = default;
        deleted_content = default;
        
        the_repository.ClearSubstitute();
        the_repository.ClearReceivedCalls();
        the_content_repository.ClearSubstitute();
        the_content_repository.ClearReceivedCalls();
    }

    private Action an_aggregate_is_saved<T>(T aggregate) where T : IAmContent
    {
        return () =>
        {
            the_repository.Save(aggregate).Await();
            the_content_repository.Save(aggregate, charlotte_bronte).Await();
        };
    }

    private Action the_aggregate_is_modified_in_memory_later(StubContent stubAggregate)
    {
        return () => stubAggregate.Text = "modified in memory content";
    }

    private Action when_getting(IAggregate aggregate)
    {
        return () =>
        {
            returned_stub_aggregate = the_repository.Get<StubContent>(aggregate.Id).Await();
            returned_stub_content = the_content_repository.Get<StubContent>(aggregate.Id).Await();
        };
    }

    private Action when_getting_multiple(StubContent stubAggregate, StubContent anotherStubAggregate)
    {
        return () =>
        {
            returned_stub_aggregates = the_repository.Get<StubContent>(stubAggregate.Id.ToString(), anotherStubAggregate.Id.ToString()).Await();
            returned_stub_contents = the_content_repository.Get<StubContent>(stubAggregate.Id.ToString(), anotherStubAggregate.Id.ToString()).Await();
        };
    }

    private void when_getting_all()
    {
        returned_stub_aggregates = the_repository.GetAll<StubContent>().Await();
        returned_stub_contents = the_content_repository.GetAll<StubContent>().Await();
    }
    
    private Action a_save_is_mocked_for(StubContent aggregate)
    {
        return () =>
        {
            the_repository.OnSaving<StubContent>(aggregate.Id.ToString(), content => saved_aggregate = content);
            the_content_repository.OnSaving<StubContent>(aggregate.Id.ToString(), content => saved_content = content);
        };
    }

    private Action a_delete_is_mocked_for(StubContent aggregate)
    {
        return () =>
        {
            the_repository.OnDeleting<StubContent>(aggregate.Id.ToString(), content => deleted_aggregate = content);
            the_content_repository.OnDeleting<StubContent>(aggregate.Id.ToString(), content => deleted_content = content);
        };
    }

    private void getting_it_again() { }

    private Action saving(StubContent stubAggregate)
    {
        return () =>
        {
            the_repository.Save(stubAggregate).Await();
            the_content_repository.Save(stubAggregate, charlotte_bronte).Await();
        };
    }

    private Action deleting(StubContent stubAggregate)
    {
        return () =>
        {
            the_repository.Delete(stubAggregate).Await();
            the_content_repository.Delete(stubAggregate, charlotte_bronte).Await();
        };
    }

    private Action deleting_by_id(StubContent stubAggregate)
    {
        return () =>
        {
            the_repository.Save(stubAggregate).Await();
            the_repository.Delete<StubContent>(stubAggregate.Id.ToString()).Await();
            the_content_repository.Save(stubAggregate, charlotte_bronte).Await();
            the_content_repository.Delete<StubContent>(stubAggregate.Id.ToString(), charlotte_bronte).Await();
        };
    }

    private Action the_aggregate_is_returned(IAggregate aggregate)
    {
        return () =>
        {
            returned_stub_aggregate.should_be(aggregate);
            returned_stub_content.should_be(aggregate);
        };
    }

    private Action the_aggregate_has_the_same_contents(StubContent aggregate)
    {
        return () =>
        {
            returned_stub_aggregate.Text.should_be(aggregate.Text);
            returned_stub_content.Text.should_be(aggregate.Text);
        };
    }

    private void it_is_not_the_same_instance()
    {
        ReferenceEquals(returned_stub_aggregate, the_repository.Get<StubContent>(stub_aggregate.Id.ToString()).Result).should_be_false();
        ReferenceEquals(returned_stub_content, the_content_repository.Get<StubContent>(stub_aggregate.Id.ToString()).Result).should_be_false();
    }

    private Action it_was_saved(IAggregate aggregate)
    {
        return () =>
        {
            saved_aggregate.should_be(aggregate);
            saved_content.should_be(aggregate);
        };
    }

    private Action it_was_deleted(StubContent aggregate)
    {
        return () =>
        {
            deleted_aggregate.should_be(aggregate);
            deleted_content.should_be(aggregate);
        };
    }

    private Action the_aggregates_are_returned(StubContent aggregate, StubContent anotherAggregate)
    {
        return () =>
        {
            returned_stub_aggregates.Length.should_be(2);
            returned_stub_contents.Length.should_be(2);
            
            returned_stub_aggregates.should_contain(aggregate);
            returned_stub_contents.should_contain(aggregate);
            returned_stub_aggregates.should_contain(anotherAggregate);
            returned_stub_contents.should_contain(anotherAggregate);
        };
    }

    private void all_aggregates_are_returned()
    {
        returned_stub_aggregates.Length.should_be(3);
        returned_stub_contents.Length.should_be(3);
            
        returned_stub_aggregates.should_contain(stub_aggregate);
        returned_stub_contents.should_contain(stub_aggregate);
        returned_stub_aggregates.should_contain(another_stub_aggregate);
        returned_stub_contents.should_contain(another_stub_aggregate);
        returned_stub_aggregates.should_contain(a_third_stub_aggregate);
        returned_stub_contents.should_contain(a_third_stub_aggregate);
    }
}