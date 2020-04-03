using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class ConfigurationV1 : Configuration
    {
        public ConfigurationV1(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset, string headerId = "Fixture-v1")
            : base(userLoadTestHttpClient, "ConfiguratorServiceInternal/v3", tenant, rulesetNamespace,  ruleset, headerId)
        {        
        }
    }
}
