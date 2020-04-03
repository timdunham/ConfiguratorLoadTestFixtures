using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class ConfigurationV1 : Configuration
    {
        internal override string HeaderId => "Fixture-v1";
        public ConfigurationV1(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset)
            : base(userLoadTestHttpClient, "ConfiguratorServiceInternal/v3", tenant, rulesetNamespace,  ruleset)
        {        
        }
        
    }
}
