using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Zoxive.HttpLoadTesting.Framework.Http;
using Zoxive.HttpLoadTesting.Framework.Http.Json;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public abstract class Configuration
    {
        internal readonly IUserLoadTestHttpClient _userLoadTestHttpClient;
        internal readonly string _tenant;
        internal readonly string _rulesetNamespace;
        internal readonly string _ruleset;
        internal JToken _ui;
        internal Guid _configurationId;
        internal List<string> _integrationParameters = new List<string>();
        internal List<string> _rapidOptions = new List<string>();
        private string _baseUrl {get;}
        private string _headerId {get;}
        internal virtual string StartConfigurationUrl => _baseUrl + "/ProductConfiguratorUI.svc/json/StartConfiguration";
        internal virtual string ConfigureUrl => _baseUrl + "/ProductConfiguratorUI.svc/json/Configure";
        internal virtual string FinalizeConfigurationUrl => _baseUrl + "/ProductConfiguratorUI.svc/json/FinalizeConfiguration";
        internal virtual string FinishInteractiveUrl => _baseUrl + "/ProductConfigurator.svc/json/FinishInteractiveConfiguration";
        internal virtual string DeleteConfigurationUrl => _baseUrl + "/ProductConfigurator.svc/json/DeleteConfiguration";
        internal virtual string CancelConfigurationUrl => _baseUrl + "/ProductConfiguratorUI.svc/json/CancelConfiguration";
        internal virtual string SessionIdCaption => "sessionID";

        public Configuration(IUserLoadTestHttpClient userLoadTestHttpClient, string baseUrl, string tenant, string rulesetNamespace, string ruleset, string headerId)
        {
            _userLoadTestHttpClient = userLoadTestHttpClient;
            _baseUrl = baseUrl;
            _tenant = tenant;
            _rulesetNamespace = rulesetNamespace;
            _ruleset = ruleset;
            _configurationId = Guid.NewGuid();
            _headerId = headerId;
        }
        public Configuration WithIntegrationParameter(string name, string value, string dataType)
        {
            var dataTypeNumber = (dataType=="number")? 1 : (dataType=="boolean") ? 2 : 0;
                
            _integrationParameters.Add($"{{ \"Name\": \"{name}\", \"SimpleValue\": \"{value}\", \"IsNull\": false, \"Type\": \"{dataTypeNumber}\" }}"); //isNull vs IsNull?
            return this;
        }
        public Configuration WithRapidOption(string name, string valueExpression)
        {
            _rapidOptions.Add($"{{ \"VariableName\":\"{name}\", \"ValueExpression\":\"{valueExpression}\" }}");
            return this;
        }
        public async Task<Configuration> StartAsync()
        {
            var result =(await _userLoadTestHttpClient.Post(StartConfigurationUrl, GetInputParameters(), null));
            _ui = result.AsJson();
            return this;
        }

        public async Task<Configuration> ConfigureAsync(string caption, string value, string stepName)
        {
            var result = await _userLoadTestHttpClient.Post(ConfigureUrl, ChangeOption(caption, value));
            _ui = result.AsJson();
            return this;
        }
        
        public async Task<Configuration> ConfigureWithRandomOptionAsync(string caption, string[] withoutValues, string stepName)
        {
            var value = FindRandomScreenValue(caption, withoutValues);
            Console.Write($"{value}-");
            _ui = (await _userLoadTestHttpClient.Post(ConfigureUrl, ChangeOption(caption, value))).AsJson();
            return this;
        }
        
        public async Task<Configuration> Continue()
        {
            _ui = (await _userLoadTestHttpClient.Post(ConfigureUrl, ConfigureBody(""))).AsJson();
            return this;
        }
        
        public async Task<Configuration> Finalize()
        {
            var result = await _userLoadTestHttpClient.Post(FinalizeConfigurationUrl, GetSessionId());
            return this;
        }
        public async Task<Configuration> FinishInteractive()
        {
            var result = await _userLoadTestHttpClient.Post(FinishInteractiveUrl, GetApplicationAndHeaderDetail());
            return this;
        }

        public async Task<Configuration> Delete()
        {
            var result = await _userLoadTestHttpClient.Post(DeleteConfigurationUrl, GetApplicationAndHeaderDetail());
            return this;
        }
        public async Task<Configuration> Cancel()
        {
            var result = await _userLoadTestHttpClient.Post(CancelConfigurationUrl, GetSessionId());
            return this;
        }

        internal virtual string FindScreenId(string screenOptionCaption)
        {
            var screen = FindScreen(screenOptionCaption);
            return screen.Value<string>("ID");
        }

        internal virtual string FindRandomScreenValue(string screenOptionCaption, IEnumerable<string> withoutValues)
        {
            var screen = FindScreen(screenOptionCaption);
            var selectableValues = screen.SelectToken("SelectableValues").Select(x=>x.Value<string>("Value")).Except(withoutValues).ToArray();
            var index = new Random().Next(selectableValues.Length);
            return selectableValues[index];
        }

        internal virtual HttpContent GetInputParameters()
        {
            var inputParams = $@"{{ ""inputParameters"" : {{
                ""Application"": {{ ""Instance"": ""{_tenant}"",""Name"": ""{_tenant}"",""User"": ""test"" }},
                ""Part"": {{ ""Namespace"": ""{_rulesetNamespace}"", ""Name"": ""{_ruleset}""}},
                ""Mode"": 0,
                ""Profile"": ""default"",
                ""HeaderDetail"" : {{ ""HeaderId"": ""{_headerId}"", ""DetailId"": ""{_configurationId}"" }},
                ""SourceHeaderDetail"" : {{ ""HeaderId"": """", ""DetailId"": """" }},
                ""VariantKey"" : null,
                ""IntegrationParameters"" : [{string.Join(',', _integrationParameters)}],
                ""RapidOptions"" : [ {string.Join(',', _rapidOptions)} ]
            }} }}";
            return new StringContent( inputParams, Encoding.UTF8, "application/json");        
        }

        internal virtual HttpContent GetSessionId()
        {
            return new StringContent($@"{{ ""{SessionIdCaption}"": ""{SessionId()}"" }}", Encoding.UTF8, "application/json");
        }

        private JToken FindScreen(string screenOptionCaption)
        {
            var screen = _ui.SelectToken($"$...ScreenOptions[?(@.Caption=='{screenOptionCaption}')]");
            if (screen==null)
                throw new ApplicationException($"Unable to find page {screenOptionCaption}");
            return screen;
        }

        internal virtual string SessionId()
        {
            return _ui.Value<string>("SessionID");
        }

        internal virtual HttpContent ChangeOption(string screenOptionCaption, string value)
        {
            var screenId = FindScreenId(screenOptionCaption);
            //return ConfigureBody($"{{ \"ID\": \"\",\"Value\": \"{value}\" }}");
            return ConfigureBody($"{{ \"ID\": \"{screenId}\",\"Value\": \"{value}\" }}");
        }

        internal virtual HttpContent ConfigureBody(string selections)
        {
            return new StringContent($@"{{
                ""{SessionIdCaption}"": ""{SessionId()}"",
                ""selections"": [ {selections} ]
                }}", Encoding.UTF8, "application/json");
        }

        internal virtual HttpContent GetApplicationAndHeaderDetail()
        {
            var applicationHeaderDetail = $@"{{ 
                ""application"": {{ ""Instance"": ""{_tenant}"",""Name"": ""{_tenant}"",""User"": ""test"" }},
                ""headerDetail"" : {{ ""HeaderId"": ""{_headerId}"", ""DetailId"": ""{_configurationId}"" }}
            }} ";
            return new StringContent(applicationHeaderDetail, Encoding.UTF8, "application/json");
        }

    }
}