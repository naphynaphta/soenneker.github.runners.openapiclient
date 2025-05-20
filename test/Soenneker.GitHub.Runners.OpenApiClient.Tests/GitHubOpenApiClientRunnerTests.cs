using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.GitHub.Runners.OpenApiClient.Tests;

[Collection("Collection")]
public class GitHubOpenApiClientRunnerTests : FixturedUnitTest
{

    public GitHubOpenApiClientRunnerTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public void Default()
    {

    }
}
