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
        internal List<string> _integrationParameters = new List<string>();
        internal abstract string StartConfigurationUrl {get;}
        internal abstract string ConfigureUrl {get;}
        internal abstract string FinalizeConfigurationUrl {get;}
        internal abstract string CancelConfigurationUrl {get;}
        internal abstract string SessionIdCaption {get;}
        internal abstract HttpContent GetSessionId();
        internal abstract HttpContent GetInputParameters();

        public Configuration(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset)
        {
            _userLoadTestHttpClient = userLoadTestHttpClient;
            _tenant = tenant;
            _rulesetNamespace = rulesetNamespace;
            _ruleset = ruleset;
        }
        public Configuration WithIntegrationParameter(string name, string value, string dataType)
        {
            var dataTypeNumber = (dataType=="number")? 1 : (dataType=="boolean") ? 2 : 0;
                
            _integrationParameters.Add($"{{ \"Name\": \"{name}\", \"SimpleValue\": \"{value}\", \"IsNull\": false, \"Type\": \"{dataTypeNumber}\" }}"); //isNull vs IsNull?
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
            _ui = (await _userLoadTestHttpClient.Post(ConfigureUrl, ChangeOption(caption, value))).AsJson();
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
            return ConfigureBody($"{{ \"ID\": \"{screenId}\",\"Value\": \"{value}\" }}");
        }

        internal virtual HttpContent ConfigureBody(string selections)
        {
            return new StringContent($@"{{
                ""{SessionIdCaption}"": ""{SessionId()}"",
                ""selections"": [ {selections} ]
                }}", Encoding.UTF8, "application/json");
        }


    }
}