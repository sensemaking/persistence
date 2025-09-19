using System;
using System.Collections.Immutable;
using System.Linq;

namespace Fdb.Rx.Domain
{
    internal interface IRespondToTransitions
    {
        void EditedBy(User user);
        void MakeReadyForQc(User user);
        void Qc(User user);
        void Suspend<TU>(TU aggregate, User user) where TU : IAggregate?;
        void Retire<TU>(TU aggregate, User user) where TU : IAggregate?;
        void Reactivate(User user);
        void Delete(User user);
    }

    public abstract partial class Content<T> : IRespondToTransitions
    {
        public virtual void EditedBy(User user)
        {
            if (Lifecycle == ContentLifecycles.Retired && user.IsHuman) throw new ValidationException("Reactivate content before making changes.");
            
            EditList = EditList.Add(user);

            if (user.IsHuman)
                ReadyForQc = false;

            if(EditList.All(x => x.IsSystem))
                ReadyForQc = Lifecycle != ContentLifecycles.Retired;
        }

        public virtual void MakeReadyForQc(User user)
        {
            Validation.BasedOn((errors) =>
            {
                if(user.IsSystem) errors.Add("System users cannot make content ready for qc.");

                if(Lifecycle == ContentLifecycles.Retired) errors.Add("Retired content cannot be made ready for qc.");

                if(!CanBeMadeReadyForQc) errors.Add("Content must have been edited to be made ready for qc.");
            });

            ReadyForQc = true;
        }

        public virtual void Qc(User user)
        {
            Validation.BasedOn((errors) =>
            {
                if(user.IsSystem)
                    if(CreatedBy.IsHuman)
                        errors.Add("System users cannot qc content that was not system created.");
                    else if(HasBeenLive && Lifecycle == ContentLifecycles.Suspended)
                        errors.Add("System users cannot qc content once it has been live.");

                if(Lifecycle == ContentLifecycles.Retired) errors.Add("Retired content cannot be qcd.");

                if(!ReadyForQc) errors.Add("Content is not yet ready for qc.");

                if(LastHumanEditor() == user) errors.Add("You cannot qc as you are the last person to edit the content.");
            });

            Lifecycle = ContentLifecycles.Live;
            EditList = ImmutableArray.Create<User>();
            ReadyForQc = false;
            HasBeenLive = true;
        }

        public virtual void Suspend<TU>(TU aggregate, User user) where TU : IAggregate?
        {
            Validation.BasedOn((errors) =>
            {
                if (Lifecycle == ContentLifecycles.New) errors.Add("New content cannot be suspended.");

                if (Lifecycle == ContentLifecycles.Retired) errors.Add("Retired content cannot be suspended.");
    
                if (!user.IsHumanOrCreatingSystemUser(CreatedBy)) errors.Add("System users cannot suspend content unless they created it.");

            });

            Lifecycle = ContentLifecycles.Suspended;
        }

        public virtual void Retire<TU>(TU aggregate, User user) where TU : IAggregate?
        {
            Validation.BasedOn((errors) => { if(!user.IsHumanOrCreatingSystemUser(CreatedBy)) errors.Add("System users cannot retire content unless they created it."); });

            Lifecycle = ContentLifecycles.Retired;
            ReadyForQc = false;
        }

        public virtual void Reactivate(User user)
        {
            Validation.BasedOn((errors) =>
            {
                if(user.IsSystem) errors.Add("System users cannot reactivate content.");

                if(Lifecycle != ContentLifecycles.Retired) errors.Add("Only retired content can be reactivated.");
            });

            Lifecycle = HasBeenLive ? ContentLifecycles.Suspended : ContentLifecycles.New;
        }

        public virtual void Delete(User user) { }

        public Transitions Transitions()
        {
            var transitions = Domain.Transitions.Change;

            transitions |= Lifecycle switch
            {
                ContentLifecycles.New => Domain.Transitions.Retire,
                ContentLifecycles.Live => Domain.Transitions.Suspend | Domain.Transitions.Retire,
                ContentLifecycles.Suspended => Domain.Transitions.Retire,
                ContentLifecycles.Retired => Domain.Transitions.Reactivate,
                _ => throw new ArgumentOutOfRangeException()
            };

            if(CanBeMadeReadyForQc)
                transitions |= Domain.Transitions.MakeReadyForQc;

            if(ReadyForQc)
                transitions |= Domain.Transitions.Qc;

            return transitions;
        }

        public Transitions Transitions(User user)
        {
            return user.IsSystem
                ? Domain.Transitions.Change
                : LastHumanEditor() == user
                    ? Transitions() & ~Domain.Transitions.Qc
                    : Transitions();
        }

        public bool HasTransition(Transitions transition)
        {
            return Transitions().HasFlag(transition);
        }

        public bool HasTransitionFor(User user, Transitions transition)
        {
            return Transitions(user).HasFlag(transition);
        }

        private bool CanBeMadeReadyForQc => HasEditsOrSuspended && !AlreadyReadyForQcOrRetired;
        private bool HasEditsOrSuspended => HasEdits || Lifecycle == ContentLifecycles.Suspended;
        private bool AlreadyReadyForQcOrRetired => ReadyForQc || Lifecycle == ContentLifecycles.Retired;

    }
}