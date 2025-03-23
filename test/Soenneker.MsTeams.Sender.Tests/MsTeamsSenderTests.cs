using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.MsTeams.Sender.Tests;

[Collection("Collection")]
public class MsTeamsSenderTests : FixturedUnitTest
{
    private readonly IMsTeamsSender _util;

    public MsTeamsSenderTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IMsTeamsSender>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
