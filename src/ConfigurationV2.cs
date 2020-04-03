using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class ConfigurationV2: Configuration
    {
        public ConfigurationV2(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset, string headerId = "Fixture-v2")
            :base(userLoadTestHttpClient, "api/v4", tenant, rulesetNamespace, ruleset, headerId)
        {
        }
    }
}
