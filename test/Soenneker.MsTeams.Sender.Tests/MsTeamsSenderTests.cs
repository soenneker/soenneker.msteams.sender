using Soenneker.MsTeams.Sender.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.MsTeams.Sender.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class MsTeamsSenderTests : HostedUnitTest
{
    private readonly IMsTeamsSender _util;

    public MsTeamsSenderTests(Host host) : base(host)
    {
        _util = Resolve<IMsTeamsSender>(true);
    }

    [Test]
    public void Default()
    {

    }
}
