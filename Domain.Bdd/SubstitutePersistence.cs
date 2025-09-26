using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sensemaking.Domain;
using Sensemaking.Monitoring;
using System.Serialization;

namespace Sensemaking.Bdd.Domain
{
    internal class SubstitutePersistence : IPersist
    {
        private readonly IDictionary<(Type Type, string Id), string> database = new Dictionary<(Type, string), string>();
        private readonly IDictionary<(ActionType, Type, string), Action<object>> actions = new Dictionary<(ActionType, Type, string), Action<object>>();
        private readonly IDictionary<(Type Type, string Id), string> liveDatabase = new Dictionary<(Type, string), string>();
        private readonly AggregateRegistration registration = new AggregateRegistration();

        public IMonitor Monitor { get; } = new Monitor();

        public Task<T> Get<T>(string id) where T : IAggregate
        {
            return Task.FromResult(database.ContainsKey(id.GetKey<T>()) ? database[id.GetKey<T>()].Deserialize<T>() : default)!;
        }

        public Task<T[]> Get<T>(params string[] ids) where T : IAggregate
        {
            return Task.FromResult(database.GetAllOfType<T>().Where(x => ids.Contains(x.Id)).ToArray());
        }

        public Task<T[]> GetAll<T>() where T : IAggregate
        {
            return Task.FromResult(database.GetAllOfType<T>().ToArray());
        }

        public Task Persist<T>(T aggregate) where T : IAggregate
        {
            database.Persist(aggregate);

            if (actions.ContainsKey(aggregate.Id.GetSavingKey<T>()))
                actions[aggregate.Id.GetSavingKey<T>()](aggregate);

            return Task.CompletedTask;
        }

        public Task Remove<T>(T aggregate) where T : IAggregate
        {
            database.Remove(aggregate.Id.GetKey<T>());

            if (actions.ContainsKey(aggregate.Id.GetDeletingKey<T>())) actions[aggregate.Id.GetDeletingKey<T>()](aggregate);

            return Task.CompletedTask;
        }

        public AggregateRegistration GetTypeRegistration()
        {
            return registration;
        }

        public Task<T> GetLive<T>(string id) where T : IAmContent
        {
            return Task.FromResult(liveDatabase.ContainsKey(id.GetKey<T>()) ? liveDatabase[id.GetKey<T>()].Deserialize<T>() : default)!;
        }

        public Task<IReadOnlyCollection<T>> GetAllLive<T>() where T : IAmContent
        {
            return Task.FromResult((IReadOnlyCollection<T>)liveDatabase.GetAllOfType<T>().Where(x => x.IsLive));
        }

        public Task PersistAsLive<T>(T aggregate) where T : IAmContent
        {
            liveDatabase.Persist(aggregate);
            database.Persist(aggregate);
            return Task.CompletedTask;
        }

        public Task RemoveFromLive<T>(T aggregate) where T : IAmContent
        {
            liveDatabase.Remove(aggregate.Id.GetKey<T>());
            return Task.CompletedTask;
        }

        internal void ClearSubstitute()
        {
            liveDatabase.Clear();
            database.Clear();
        }

        internal void OnSaving<T>(string id, Action<T> onSave) where T : IAggregate
        {
            actions.Persist(id, ActionType.Save, onSave);
        }

        internal void OnDeleting<T>(string id, Action<T> onDelete) where T : IAggregate
        {
            actions.Persist(id, ActionType.Delete, onDelete);
        }

        internal enum ActionType { Save = 0, Delete = 1 }
    }

    internal static class DictionaryExtensions
    {
        internal static (Type, string) GetKey<T>(this string id) => (typeof(T), id);
        internal static (SubstitutePersistence.ActionType, Type, string) GetKey<T>(this string id, SubstitutePersistence.ActionType type) => (type, typeof(T), id);
        internal static (SubstitutePersistence.ActionType, Type, string) GetSavingKey<T>(this string id) => id.GetKey<T>(SubstitutePersistence.ActionType.Save);
        internal static (SubstitutePersistence.ActionType, Type, string) GetDeletingKey<T>(this string id) => id.GetKey<T>(SubstitutePersistence.ActionType.Delete);
        internal static IEnumerable<T> GetAllOfType<T>(this IDictionary<(Type Type, string Id), string> database) => database.Where(x => x.Key.Type == typeof(T)).Select(x => x.Value.Deserialize<T>());

        internal static void Persist<T>(this IDictionary<(Type Type, string Id), string> database, T aggregate) where T : IAggregate
        {
            if (database.ContainsKey(aggregate.Id.GetKey<T>()))
                database[aggregate.Id.GetKey<T>()] = aggregate.Serialize();
            else
                database.Add(aggregate.Id.GetKey<T>(), aggregate.Serialize());
        }

        internal static void Persist<T>(this IDictionary<(SubstitutePersistence.ActionType, Type, string), Action<object>> actions, string id, SubstitutePersistence.ActionType type, Action<T> action) where T : IAggregate
        {
            if (actions.ContainsKey(id.GetKey<T>(type)))
                actions[id.GetKey<T>(type)] = o => action((T)o);
            else
                actions.Add(id.GetKey<T>(type), o => action((T)o));
        }
    }
}