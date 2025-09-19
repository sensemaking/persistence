using System;
using System.Linq;
using System.Data;
using Fdb.Rx.Domain;
using Fdb.Rx.Domain.Events;
using Fdb.Rx.Persistence.Dapper;
using Fdb.Rx.Test;
using NSubstitute;
using Sensemaking.Bdd;

namespace Fdb.Rx.Testing.Domain.Lifecycle;

public class CommonLifecycleSteps : Specification
{
    private const string collection = "test_collection";
    private const string published_collection = $"{collection}AsPublished";

    protected static readonly User charlotte_bronte = Authors.charlotte_bronte;
    protected static readonly User jane_austin = Authors.jane_austin;
    protected static readonly User system_user = Authors.system_user;
    protected static readonly User another_system_user = Authors.another_system_user;
    protected static readonly User human_user = Authors.human_user;

    protected IContentRepository repository;
    protected StubContent content;
    private Action<StubContent> changed_handler;
    private Action<StubContent> made_ready_for_qc_handler;
    private Action<StubContent, StubContent> qcd_handler;
    private StubContent live_content_before_qc;
    private bool was_live_before_qc => live_content_before_qc != null;
    private Action<StubContent, StubContent> suspended_handler;
    private StubContent live_content_before_suspend;
    private Action<StubContent, StubContent> retired_handler;
    private StubContent live_content_before_retired;
    private bool was_live_before_retired => live_content_before_retired != null;
    private Action<StubContent> reactivated_handler;

    private readonly IDb db = DbFactory.Create(Startup.Database.connection_string);

    protected override void before_all()
    {
        base.before_all();
        db.Execute($"CREATE TABLE {collection} (Id varchar(200), Document varchar(MAX))", commandType: CommandType.Text);
        db.Execute($"CREATE TABLE {published_collection} (Id varchar(200), Document varchar(MAX))", commandType: CommandType.Text);
    }

    protected override void after_all()
    {
        db.Execute($"DROP TABLE {collection}", commandType: CommandType.Text);
        db.Execute($"DROP TABLE {published_collection}", commandType: CommandType.Text);
        base.after_all();
    }

    protected override void before_each()
    {
        base.before_each();
        repository = RepositoryBuilder.For.Dapper(db)
            .Register<StubContent>(collection, null)
            .Handling(() => new IHandleDomainEvents[]
            {
                new FakeChangedDomainEventHandler<StubContent>(changed_handler = Substitute.For<Action<StubContent>>()),
                new FakeMadeReadyForQcDomainEventHandler<StubContent>(made_ready_for_qc_handler = Substitute.For<Action<StubContent>>()),
                new FakeQcdDomainEventHandler<StubContent>(qcd_handler = Substitute.For<Action<StubContent, StubContent>>()),
                new FakeSuspendedDomainEventHandler<StubContent>(suspended_handler = Substitute.For<Action<StubContent, StubContent>>()),
                new FakeRetiredDomainEventHandler<StubContent>(retired_handler = Substitute.For<Action<StubContent, StubContent>>()),
                new FakeReactivatedDomainEventHandler<StubContent>(reactivated_handler = Substitute.For<Action<StubContent>>())
            })
            .Get().Content;

        content = default;
        live_content_before_qc = default;
        live_content_before_retired = default;
        live_content_before_suspend = default;
    }

    protected Action new_content_is_created_by(User? user = null)
    {
        return () =>
        {
            content = new StubContent("Some content", user ?? charlotte_bronte);
            repository.Save(content, user ?? charlotte_bronte).Await();
        };
    }

    protected void some_new_content()
    {
        Given(new_content_is_created_by(charlotte_bronte));
    }

    protected void some_ready_for_qc_content()
    {
        Given(some_new_content);
        And(it_is_made_ready_for_qc_by(charlotte_bronte));
    }

    protected void some_live_content()
    {
        Given(some_ready_for_qc_content);
        And(it_is_qcd_by(jane_austin));
    }

    protected void some_suspended_content()
    {
        Given(some_live_content);
        And(it_is_suspended_by(jane_austin));
    }

    protected void some_retired_content()
    {
        Given(some_live_content);
        And(it_is_retired_by(jane_austin));
    }

    protected Action it_is_changed_by(User user)
    {
        return () =>
        {
            content.Text = $"Changed by {user.Name}";
            trying(() => repository.Save(content, user).Await());
        };
    }

    protected Action it_is_made_ready_for_qc_by(User user)
    {
        return () => trying(() => repository.MakeReadyForQc(content, user).Await());
    }

    protected Action it_is_qcd_by(User user)
    {
        return () => trying(() =>
        {
            live_content_before_qc = repository.GetLive<StubContent>(content.Id.ToString()).Await();
            repository.Qc(content, user).Await();
        });
    }

    protected Action it_is_suspended_by(User user)
    {
        return () => trying(() =>
        {
            live_content_before_suspend = repository.GetLive<StubContent>(content.Id.ToString()).Await();
            repository.Suspend(content, user).Await();
        });
    }

    protected Action it_is_retired_by(User user)
    {
        return () => trying(() =>
        {
            live_content_before_retired = repository.GetLive<StubContent>(content.Id.ToString()).Await();
            repository.Retire(content, user).Await();
        });
    }

    protected Action it_is_reactivated_by(User user)
    {
        return () => trying(() => repository.Reactivate(content, user).Await());
    }

    protected Action it_is_deleted_by(User user)
    {
        return () => trying(() => repository.Delete(content, user).Await());
    }

    protected Action it_moves_to(ContentLifecycles lifecycle)
    {
        return () => content.Lifecycle.should_be(lifecycle);
    }

    protected Action it_remains(ContentLifecycles lifecycle)
    {
        return it_moves_to(lifecycle);
    }

    protected Action it_has_transitions(Transitions transitions)
    {
        return () => content.Transitions().should_be(transitions);
    }

    protected Action it_has_transitions_for(User user, Transitions transitions)
    {
        return () => content.Transitions(user).should_be(transitions);
    }

    protected Action it_was_edited_by(User user)
    {
        return () => content.EditList.should_contain(user);
    }

    protected Action the_last_human_editor_is(User? user)
    {
        return () => content.LastHumanEditor()!.should_be(user!);
    }

    protected void it_is_not_edited()
    {
        content.HasEdits.should_be_false();
        content.LastHumanEditor()!.should_be_null();
    }

    protected void it_is_live()
    {
        repository.GetLive<StubContent>(content.Id.ToString()).Await().Text.should_be(content.Text);
    }

    protected void it_is_no_longer_live()
    {
        repository.GetLive<StubContent>(content.Id.ToString()).Await().should_be_null();
    }

    protected bool has_previous_human_edits()
    {
        return content.EditList.Any(x => x.IsHuman);
    }

    public void qc_exits()
    {
        Then(it_moves_to(ContentLifecycles.Live));
        And(it_is_live);
        And(it_is_not_edited);
        And(it_has_transitions(Transitions.Change | Transitions.Suspend | Transitions.Retire));
        And(it_has_transitions_for(system_user, Transitions.Change));
        And(event_is_raised(Transitions.Qc));
    }

    protected Action event_is_raised(Transitions transition)
    {
        return () =>
        {
            switch (transition)
            {
                case Transitions.Change:
                    changed_handler.Received().Invoke(Arg.Is<StubContent>(x => x.Equals(content)));
                    changed_handler.ClearReceivedCalls();
                    break;
                case Transitions.MakeReadyForQc:
                    made_ready_for_qc_handler.Received().Invoke(Arg.Is<StubContent>(x => x.Equals(content)));
                    made_ready_for_qc_handler.ClearReceivedCalls();
                    break;
                case Transitions.Qc:
                    qcd_handler.Received().Invoke(Arg.Is<StubContent>(x => x.Equals(content)), Arg.Is<StubContent>(x => was_live_before_qc ? x.Text == live_content_before_qc.Text : x == null));
                    qcd_handler.ClearReceivedCalls();
                    break;
                case Transitions.Suspend:
                    suspended_handler.Received().Invoke(Arg.Is<StubContent>(x => x.Equals(content)), Arg.Is<StubContent>(x => x.Text == live_content_before_suspend.Text));
                    suspended_handler.ClearReceivedCalls();
                    break;
                case Transitions.Retire:
                    retired_handler.Received().Invoke(Arg.Is<StubContent>(x => x.Equals(content)), Arg.Is<StubContent>(x => was_live_before_retired ? x.Text == live_content_before_retired.Text : x == null));
                    retired_handler.ClearReceivedCalls();
                    break;
                case Transitions.Reactivate:
                    reactivated_handler.Received().Invoke(Arg.Is<StubContent>(x => x.Equals(content)));
                    reactivated_handler.ClearReceivedCalls();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transition), transition, null);
            }
        };
    }
}