using Sensemaking.AuditedDomain;
using Sensemaking.Domain;
using Sensemaking.Messaging;

namespace Sensemaking.Persistence.Cosmos.AuditedDomain;

public static class Repositories
{
    public static IContentRepository WithAuditing(this IContentRepository repository, IPublishEvents publisher)
    {
        return new Repository(repository, publisher);
    }
}