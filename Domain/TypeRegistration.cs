using System;
using System.Collections.Generic;

namespace Sensemaking.Domain
{
    internal class AggregateRegistration
    {
        private readonly IDictionary<Type, object> typeRegistration = new Dictionary<Type, object>();

        public void Register<T>(string collection, IValidateCollections<T>? collectionValidator) where T : IAggregate
        {
            if (typeRegistration.ContainsKey(typeof(T)))
                throw new Exception("Aggregate already registered.");

            typeRegistration.Add(typeof(T), new RegisteredAggregate<T>(collection, collectionValidator));
        }

        public RegisteredAggregate<T> Get<T>() where T : IAggregate
        {
            if (!typeRegistration.ContainsKey(typeof(T)))
                throw new Exception($"{typeof(T).FullName} has not been registered with document db.");

            return (typeRegistration[typeof(T)] as RegisteredAggregate<T>)!;
        }
    }

    public interface IValidateCollections<in T> where T : IAggregate
    {
        (bool validationFailed, ValidationException exceptionToThrow) Validate(T aggregate, string collection);
    }

    internal class RegisteredAggregate<T> where T : IAggregate
    {
        private readonly string collection;

        public RegisteredAggregate(string collection, IValidateCollections<T>? validator)
        {
            this.collection = collection;
            Validator = validator;
        }

        public string Collection(string suffix = "") => $"{collection}{suffix}";
        public IValidateCollections<T>? Validator { get; }
    }
}