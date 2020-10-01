using System.Collections.Generic;

namespace Sensemaking.Domain
{
    public interface IValidateCollections<in T> where T : class, IAggregate
    {
        void Validate(IEnumerable<T> aggregates);
    }

    public class TypeRegistration<T> where T : class, IAggregate
    {
        public TypeRegistration(string collection, IValidateCollections<T> collectionValidator)
        {
            Collection = collection;
            CollectionValidator = collectionValidator;
        }

        public string Collection { get; }
        public IValidateCollections<T> CollectionValidator { get; }
    }
}