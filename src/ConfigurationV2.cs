using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class ConfigurationV2: Configuration
    {
        internal override string HeaderId => "Fixture-v2";

        public ConfigurationV2(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset)
            :base(userLoadTestHttpClient, "api/v4", tenant, rulesetNamespace, ruleset)
        {
        }
    }
}
