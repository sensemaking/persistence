using System;
using Fdb.Rx.Domain;
using Fdb.Rx.Persistence.Blob;
using NUnit.Framework;

namespace Sensemaking.Specs.Persistence.Blob.Domain;

[TestFixture]
public partial class PersistenceSteps() : PersistenceSpecsTemplate(() => RepositoryBuilder.For.Blob()
    .Register<AnAggregate>(container_name, null).Get().Repository)
{
    protected override void before_all()
    {
        base.before_all();
        Account.Configure(Settings.Storage.EmulatorConnectionString);
    }
}