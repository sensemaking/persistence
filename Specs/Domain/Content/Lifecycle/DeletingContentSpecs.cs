using System;
using NUnit.Framework;
using Sensemaking.Bdd;

namespace Sensemaking.Specs.Domain.Lifecycle;

public class DeletingContentSpecs : CommonLifecycleSteps
{
    [Test]
    public void content_deletion()
    {
        scenario(humans_can_delete_content);
        scenario(system_users_can_delete_content);
    }

    private void humans_can_delete_content()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_deleted_by(charlotte_bronte));
        Then(delete_exits);
    }

    private void system_users_can_delete_content()
    {
        Given(new_content_is_created_by(system_user));
        When(it_is_deleted_by(system_user));
        Then(delete_exits);
    }

    private void delete_exits()
    {
        repository.Get<StubContent>(content.Id.ToString()).Result.should_be_null();
    }
}