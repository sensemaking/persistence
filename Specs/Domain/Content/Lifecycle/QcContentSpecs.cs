using System;
using NUnit.Framework;

namespace Fdb.Rx.Testing.Domain.Lifecycle;

public class QcContentSpecs : CommonLifecycleSteps
{
    [Test]
    public void content_can_be_qcd()
    {
        scenario(new_content_can_be_qcd);
        scenario(live_content_can_be_qcd_again_once_ready_for_qc);
        scenario(suspended_content_can_be_qcd_again_once_ready_for_qc);
    }

    [Test]
    public void content_must_be_ready_for_qc()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_qcd_by(jane_austin));
        Then(() => informs<ValidationException>("Content is not yet ready for qc."));
    }

    [Test]
    public void live_content_created_by_a_system_can_be_edited_and_is_auto_qcd()
    {
        Given(new_content_is_created_by(system_user));
        When(it_is_changed_by(system_user));
        Then(qc_exits);
    }

    [Test]
    public void suspended_content_created_by_the_system_cannot_be_qcd_by_the_system()
    {
        Given(new_content_is_created_by(system_user));
        And(it_is_suspended_by(jane_austin));
        When(it_is_qcd_by(system_user));
        Then(() => informs<ValidationException>("System users cannot qc content once it has been live."));
    }


    [Test]
    public void system_user_cannot_qc_human_created_content()
    {
        scenario(new_human_created_content_cannot_be_qcd_by_system);
        scenario(live_human_created_content_cannot_be_qcd_by_system);
    }

    [Test]
    public void content_cannot_be_qcd_by_those_who_last_edited_them()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        When(it_is_made_ready_for_qc_by(jane_austin));
        When(it_is_qcd_by(charlotte_bronte));
        Then(() => informs<ValidationException>("You cannot qc as you are the last person to edit the content."));
    }

    [Test]
    public void retired_content_cannot_be_qcd()
    {
        Given(some_retired_content);
        When(it_is_qcd_by(human_user));
        Then(() => informs<ValidationException>("Retired content cannot be qcd."));
    }

    private void new_content_can_be_qcd()
    {
        Given(some_ready_for_qc_content);
        When(it_is_qcd_by(jane_austin));
        Then(qc_exits);
    }

    private void live_content_can_be_qcd_again_once_ready_for_qc()
    {
        Given(some_live_content);
        And(it_is_changed_by(charlotte_bronte));
        And(it_is_made_ready_for_qc_by(charlotte_bronte));
        When(it_is_qcd_by(jane_austin));
        Then(qc_exits);
    }

    private void suspended_content_can_be_qcd_again_once_ready_for_qc()
    {
        Given(some_suspended_content);
        And(it_is_made_ready_for_qc_by(charlotte_bronte));
        When(it_is_qcd_by(jane_austin));
        Then(qc_exits);
    }

    private void new_human_created_content_cannot_be_qcd_by_system()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        And(it_is_made_ready_for_qc_by(jane_austin));
        When(it_is_qcd_by(system_user));
        Then(() => informs<ValidationException>("System users cannot qc content that was not system created."));
    }

    private void live_human_created_content_cannot_be_qcd_by_system()
    {
        Given(new_content_is_created_by(charlotte_bronte));
        And(it_is_made_ready_for_qc_by(jane_austin));
        And(it_is_qcd_by(jane_austin));
        When(it_is_qcd_by(system_user));
        Then(() => informs<ValidationException>("System users cannot qc content that was not system created."));
    }

}