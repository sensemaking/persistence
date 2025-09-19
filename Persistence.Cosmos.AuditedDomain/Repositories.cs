using Fdb.Rx.AuditedDomain;
using Fdb.Rx.Domain;
using Fdb.Rx.Messaging;

namespace Fdb.Rx.Persistence.Cosmos.AuditedDomain;

public static class Repositories
{
    public static IContentRepository WithAuditing(this IContentRepository repository, IPublishEvents publisher)
    {
        return new Repository(repository, publisher);
    }
}