using Soenneker.Blazor.Utils.IndexedDb.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Blazor.Utils.IndexedDb.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class IndexedDbUtilTests : HostedUnitTest
{
    private readonly IIndexedDbUtil _blazorlibrary;

    public IndexedDbUtilTests(Host host) : base(host)
    {
        _blazorlibrary = Resolve<IIndexedDbUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
