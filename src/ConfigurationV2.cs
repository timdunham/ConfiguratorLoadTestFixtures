using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class ConfigurationV2: Configuration
    {
        internal override string StartConfigurationUrl => "api/v4/ui/start";
        internal override string ConfigureUrl => "api/v4/ui/configure";
        internal override string FinalizeConfigurationUrl => "api/v4/ui/finalize";
        internal override string CancelConfigurationUrl => "api/v4/ui/cancel";
        internal override string SessionIdCaption => "sessionId";

        public ConfigurationV2(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset)
            :base(userLoadTestHttpClient, tenant, rulesetNamespace, ruleset)
        {
        }

        internal override HttpContent GetInputParameters()
        {
            var inputParams = $@"{{
                ""Application"": {{ ""Instance"": ""{_tenant}"",""Name"": ""{_tenant}"" }},
                ""Part"": {{ ""Namespace"": ""{_rulesetNamespace}"", ""Name"": ""{_ruleset}""}},
                ""Mode"": 0,
                ""Profile"": ""default"",
                ""HeaderDetail"" : {{ ""HeaderId"": ""Simulator"", ""DetailId"": ""{Guid.NewGuid()}"" }},
                ""SourceHeaderDetail"" : {{ ""HeaderId"": """", ""DetailId"": """" }},
                ""VariantKey"" : """",
                ""IntegrationParameters"" : [{string.Join(',', _integrationParameters)}],
                ""RapidOptions"" : []
            }}";
            return new StringContent(inputParams, Encoding.UTF8, "application/json");
        }

        internal override HttpContent GetSessionId()
        {
            return new StringContent(JsonConvert.SerializeObject(SessionId()), Encoding.UTF8, "application/json");
        }
    }
}
