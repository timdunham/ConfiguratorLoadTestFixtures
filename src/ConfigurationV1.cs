using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class ConfigurationV1 : Configuration
    {
        private const string BaseUrl = "ConfiguratorServiceInternal/v3/ProductConfiguratorUI.svc/json/";
        internal override string StartConfigurationUrl => BaseUrl + "StartConfiguration";
        internal override string ConfigureUrl => BaseUrl + "Configure";
        internal override string FinalizeConfigurationUrl => BaseUrl + "FinalizeConfiguration";
        internal override string CancelConfigurationUrl => BaseUrl + "CancelConfiguration";
        internal override string SessionIdCaption => "sessionID";

        public ConfigurationV1(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset)
            : base(userLoadTestHttpClient, tenant, rulesetNamespace,  ruleset)
        {        
        }
        
        internal override HttpContent GetInputParameters()
        {
            var inputParams = $@"{{ ""inputParameters"" : {{
                ""Application"": {{ ""Instance"": ""{_tenant}"",""Name"": ""{_tenant}"",""User"": ""test"" }},
                ""Part"": {{ ""Namespace"": ""{_rulesetNamespace}"", ""Name"": ""{_ruleset}""}},
                ""Mode"": 0,
                ""Profile"": ""default"",
                ""HeaderDetail"" : {{ ""HeaderId"": ""Simulator"", ""DetailId"": ""{Guid.NewGuid()}"" }},
                ""SourceHeaderDetail"" : {{ ""HeaderId"": """", ""DetailId"": """" }},
                ""VariantKey"" : null,
                ""IntegrationParameters"" : [{string.Join(',', _integrationParameters)}],
                ""RapidOptions"" : []
            }} }}";
            return new StringContent( inputParams, Encoding.UTF8, "application/json");
        }

        internal override HttpContent GetSessionId()
        {
            return new StringContent($@"{{ ""{SessionIdCaption}"": ""{SessionId()}"" }}", Encoding.UTF8, "application/json");
        }
    }
}
