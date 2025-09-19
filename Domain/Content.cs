using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;

namespace Fdb.Rx.Domain
{
    public interface IAmContent : IAggregate
    {
        ContentLifecycles Lifecycle { get; }
        bool IsLive { get; }
        ImmutableArray<User> EditList { get; }
        bool HasEdits { get; }
        User? LastHumanEditor();
        User CreatedBy { get; }
        Transitions Transitions(User user);
        Transitions Transitions();
    }

    public abstract partial class Content<T> : Aggregate<T>, IAmContent
    {
        protected Content() : base() { }

        protected Content(T id, User createdBy) : base(id)
        {
            EditList = ImmutableArray.Create<User>();
            CreatedBy = createdBy;
        }

        public ContentLifecycles Lifecycle { get; private set; }
        public bool IsLive => Lifecycle == ContentLifecycles.Live;

        public ImmutableArray<User> EditList { get; private set; }
        public bool HasEdits => EditList.Any();
        public User? LastHumanEditor()
        {
            var nonSystemUsers = EditList.Where(u => !u.IsSystem).ToImmutableArray();
            return !nonSystemUsers.Any() ? (User?) null : nonSystemUsers.Last();
        }

        public User CreatedBy { get; private set; }
        [JsonPropertyAttribute]
        private bool ReadyForQc { get; set; }
        [JsonPropertyAttribute]
        private bool HasBeenLive { get; set; }
    }

    public enum ContentLifecycles
    {
        New = 0,
        Live = 1,
        Suspended = 2,
        Retired = 3
    }
}