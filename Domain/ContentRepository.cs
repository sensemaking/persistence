using System;
using System.Collections.Generic;
using System.Serialization;
using System.Threading.Tasks;
using Sensemaking.Domain;
using Sensemaking.Domain.Events;
using Sensemaking.Monitoring;
using Newtonsoft.Json.Linq;

namespace Sensemaking.Domain
{
    public interface IRepositories
    {
        IContentRepository Content { get; }
        IRepository Repository { get; }
        IMonitor Monitor { get; }
    }

    public interface IContentRepository
    {
        Task Save<T>(T content, User user) where T : IAmContent;
        Task MakeReadyForQc<T>(T content, User user) where T : IAmContent;
        Task Qc<T>(T content, User user) where T : IAmContent;
        Task Suspend<T>(T content, User user) where T : IAmContent;
        Task Retire<T>(T content, User user) where T : IAmContent;
        Task Reactivate<T>(T content, User user) where T : IAmContent;
        Task Delete<T>(T content, User user) where T : IAmContent;
        Task Delete<T>(string id, User user) where T : IAmContent;
        Task<T> Get<T>(string id) where T : IAmContent;
        Task<T[]> Get<T>(params string[] ids) where T : IAmContent;
        Task<T[]> GetAll<T>() where T : IAmContent;
        Task<T> GetLive<T>(string id) where T : IAmContent;
        Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent;
    }

    internal class ContentRepository : IContentRepository
    {
        protected readonly IPersist persistence;
        protected readonly IDispatchDomainEvents dispatcher;

        internal ContentRepository(IPersist persistence, DomainEventDispatcher dispatcher)
        {
            this.persistence = persistence;
            this.dispatcher = dispatcher;
        }

        public async Task Save<T>(T content, User user) where T : IAmContent
        {
            var savedContent = await SavedContent(content);
            if (savedContent.IsTheSame)
                return;

            (content as IRespondToTransitions)!.EditedBy(user);

            content.Events.Enqueue(new Changed<T>(content));

            if (user.IsSystem && content.CreatedBy.IsSystem)
                await Qc(content, user);
            else
                await Persist(content);
        }

        public async Task MakeReadyForQc<T>(T content, User user) where T : IAmContent
        {
            (content as IRespondToTransitions)!.MakeReadyForQc(user);
            await Save(content, async () => await Persist(content, new MadeReadyForQc<T>(content)));
        }

        public async Task Qc<T>(T content, User user) where T : IAmContent
        {
            (content as IRespondToTransitions)!.Qc(user);
            await Save(content, async () =>
            {
                var live = await GetLive<T>(content.Id);
                await PersistAsLive(content);
                await Persist(content, new Qcd<T>(content, live));
            });
        }

        public async Task Suspend<T>(T content, User user) where T : IAmContent
        {
            (content as IRespondToTransitions)!.Suspend(content, user);
            await Save(content, async () =>
            {
                var live = await GetLive<T>(content.Id);
                await RemoveFromLive(live);
                await Persist(content, new Suspended<T>(content, live));
            });
        }

        public async Task Retire<T>(T content, User user) where T : IAmContent
        {
            (content as IRespondToTransitions)!.Retire(content, user);

            await Save(content, async () =>
            {
                var live = await GetLive<T>(content.Id);
                await RemoveFromLive(live);
                await Persist(content, new Retired<T>(content, live));
            });
        }

        public async Task Reactivate<T>(T content, User user) where T : IAmContent
        {
            (content as IRespondToTransitions)!.Reactivate(user);

            await Save(content, async () => await Persist(content, new Reactivated<T>(content)));
        }

        public async Task Delete<T>(T content, User user) where T : IAmContent
        {
            (content as IRespondToTransitions)!.Delete(user);
            await persistence.Remove(content).ConfigureAwait(false);
            content.Events.Enqueue(new Deleted<T>(content));
            dispatcher.Dispatch(content.Events);
        }

        public virtual async Task Delete<T>(string id, User user) where T : IAmContent
        {
            var aggregate = await persistence.Get<T>(id).ConfigureAwait(false);
            if (aggregate != null)
                await Delete(aggregate, user).ConfigureAwait(false);
        }

        public Task<T> Get<T>(string id) where T : IAmContent
        {
            return persistence.Get<T>(id);
        }

        public Task<T[]> Get<T>(params string[] ids) where T : IAmContent
        {
            return persistence.Get<T>(ids);
        }

        public Task<T[]> GetAll<T>() where T : IAmContent
        {
            return persistence.GetAll<T>();
        }

        public Task<T> GetLive<T>(string id) where T : IAmContent
        {
            return persistence.GetLive<T>(id);
        }

        public Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent
        {
            return persistence.GetAllLive<T>();
        }

        private async Task<(bool Exists, bool IsTheSame)> SavedContent<T>(T content) where T : IAmContent
        {
            var existingAggregate = await persistence.Get<T>(content.Id);
            if (existingAggregate == null!)
                return (false, false);

            return (true, existingAggregate.IsEqualTo(content));
        }

        private async Task Save<T>(T content, Func<Task> saveAction) where T : IAmContent
        {
            if ((await SavedContent(content)).IsTheSame)
                return;

            await saveAction();
        }

        private async Task Persist<T>(T content, DomainEvent? @event = null) where T : IAmContent
        {
            var registeredAggregate = persistence.GetTypeRegistration().Get<T>();
            var validation = registeredAggregate.Validator?.Validate(content, registeredAggregate.Collection());
            if (validation is { validationFailed: true }) throw validation.Value.exceptionToThrow;

            await persistence.Persist(content).ConfigureAwait(false);
            if (@event != null) content.Events.Enqueue(@event);

            dispatcher.Dispatch(content.Events);
        }

        private async Task PersistAsLive<T>(T content) where T : IAmContent
        {
            await persistence.PersistAsLive(content).ConfigureAwait(false);
        }

        private async Task RemoveFromLive<T>(T live) where T : IAmContent
        {
            if (live != null!)
                await persistence.RemoveFromLive(live).ConfigureAwait(false);
        }
    }
}

internal static class JsonAggregateExtensions
{
    internal static bool IsEqualTo<T>(this T aggregate, T otherAggregate) where T : IAmContent
    {
        return JToken.DeepEquals(aggregate.ToComparableJson(), otherAggregate.ToComparableJson());
    }

    internal static JObject ToComparableJson<T>(this T aggregate) where T : IAmContent
    {
        return JObject.Parse(aggregate.Serialize());
    }
}