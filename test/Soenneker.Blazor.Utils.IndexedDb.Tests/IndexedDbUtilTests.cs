using Soenneker.Blazor.Utils.IndexedDb.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Blazor.Utils.IndexedDb.Tests;

[Collection("Collection")]
public sealed class IndexedDbUtilTests : FixturedUnitTest
{
    private readonly IIndexedDbUtil _blazorlibrary;

    public IndexedDbUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _blazorlibrary = Resolve<IIndexedDbUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
