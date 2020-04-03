using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public sealed class ConfigurationCompare
    {
        internal readonly ConfigurationV1 _v1;
        internal readonly ConfigurationV2 _v2;

        public ConfigurationCompare(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string rulesetNamespace, string ruleset)
        {
            _v1=new ConfigurationV1(userLoadTestHttpClient, tenant, rulesetNamespace, ruleset);
            _v2=new ConfigurationV2(userLoadTestHttpClient, tenant, rulesetNamespace, ruleset);
        }
        public ConfigurationCompare WithIntegrationParameter(string name, string value, string dataType)
        {
            _v1.WithIntegrationParameter(name, value, dataType);
            _v2.WithIntegrationParameter(name, value, dataType);
            return this;
        }
        public async Task<ConfigurationCompare> StartAsync()
        {
            await _v2.StartAsync();
            await _v1.StartAsync();
            //CompareUI();
            return this;
        }

        public async Task<ConfigurationCompare> ConfigureAsync(string caption, string value, string stepName)
        {
            await _v1.ConfigureAsync(caption, value, stepName);
            await _v2.ConfigureAsync(caption, value, stepName);

            //CompareUI();
            return this;
        }
        
        public async Task<ConfigurationCompare> ConfigureWithRandomOptionAsync(string caption, string[] withoutValues, string stepName)
        {
            await _v1.ConfigureWithRandomOptionAsync(caption, withoutValues, stepName);
            await _v2.ConfigureWithRandomOptionAsync(caption, withoutValues, stepName);

            //CompareUI();
            return this;
        }
        
        public async Task<ConfigurationCompare> Continue()
        {
            await _v1.Continue();
            await _v2.Continue();

            //CompareUI();
            return this;
        }
        
        public async Task<ConfigurationCompare> Finalize()
        {
            await _v1.Finalize();
            await _v2.Finalize();
            //CompareUI();
            return this;
        }

        public async Task<ConfigurationCompare> FinishInteractive()
        {
            await _v1.FinishInteractive();
            await _v2.FinishInteractive();
            //CompareUI();
            return this;
        }

        public async Task<ConfigurationCompare> Cancel()
        {
            await _v1.Cancel();
            await _v2.Cancel();
            //CompareUI();
            return this;
        }

        public async Task<ConfigurationCompare> Delete()
        {
            await _v1.Delete();
            await _v2.Delete();
            //CompareUI();
            return this;
        }

        private void CompareUI()
        {
            string description = "";
            if (!JTokenComparer.Instance.CompareToken(_v1._ui, _v2._ui, ref description))
            {
                Console.WriteLine($"NOT EQUAL : {description}");
            }
        }

        

    }
}