using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Zoxive.HttpLoadTesting.Framework.Http;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public class DataImport
    {
        private readonly IUserLoadTestHttpClient _userLoadTestHttpClient;
        private readonly string _tenant;
        private readonly string _matrixName;
        private readonly List<string> _matrixValues;
        public DataImport(IUserLoadTestHttpClient userLoadTestHttpClient, string tenant, string matrixName)
        {
            _matrixName = matrixName;
            _tenant = tenant;
            _userLoadTestHttpClient = userLoadTestHttpClient;
            _matrixValues = new List<string>();
        }
        public DataImport WithValue(string name, string value)
        {
            _matrixValues.Add($"\"{name}\": \"{value}\"");
            return this;
        }
        public async Task<DataImport> UpdateRow(string id, int version)
        {                        
            var result =(await _userLoadTestHttpClient.Patch($"DataImport/v1/Matrices/{_matrixName}", GetData(id, version), null));
            return this;
        }

        private HttpContent GetData(string id, int version)
        {
            return new StringContent($@"{{
                ""rows"": [
                    {{
                        ""Id"" : ""{id}"",
                        ""Version"" : {version},
                        ""Values"" : {{ {string.Join(',', _matrixValues)} }}
                    }}
                ]  
            }}", Encoding.UTF8, "application/json");
        }
 
    }
}