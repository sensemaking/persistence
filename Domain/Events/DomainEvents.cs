using System.Diagnostics.CodeAnalysis;

namespace Sensemaking.Domain.Events
{
    public class Changed<T> : DomainEvent where T : IAggregate
    {
        public Changed(T wasChanged)
        {
            WasChanged = wasChanged;
        }

        public T WasChanged { get; }
    }

    public class MadeReadyForQc<T> : DomainEvent where T : IAggregate
    {
        public MadeReadyForQc(T wasChanged)
        {
            WasChanged = wasChanged;
        }

        public T WasChanged { get; }
    }


    public class Qcd<T> : DomainEvent where T : IAggregate
    {
        public Qcd(T wasQcd, [MaybeNull] T previouslyLive)
        {
            WasQcd = wasQcd;
            PreviouslyLive = previouslyLive;
        }

        public T WasQcd { get; }

        [MaybeNull]
        public T PreviouslyLive { get; set; }
    }

    public class Suspended<T> : DomainEvent where T : IAggregate
    {
        public Suspended(T wasSuspended, T previouslyLive)
        {
            WasSuspended = wasSuspended;
            PreviouslyLive = previouslyLive;
        }

        public T WasSuspended { get; }
        public T PreviouslyLive { get; set; }
    }

    public class Retired<T> : DomainEvent where T : IAggregate
    {
        public Retired(T wasRetired, [MaybeNull] T previouslyLive)
        {
            WasRetired = wasRetired;
            PreviouslyLive = previouslyLive;
        }

        public T WasRetired { get; }

        [MaybeNull]
        public T PreviouslyLive { get; set; }
    }

    public class Reactivated<T> : DomainEvent where T : IAggregate
    {
        public Reactivated(T wasReactivated)
        {
            WasReactivated = wasReactivated;
        }

        public T WasReactivated { get; }
    }

    public class Deleted<T> : DomainEvent where T : IAggregate
    {
        public Deleted(T wasDeleted)
        {
            WasDeleted = wasDeleted;
        }

        public T WasDeleted { get; }
    }
}
