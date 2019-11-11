using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Infor.CPQ.Security.OAuth.Client.ZeroLegged;
using Zoxive.HttpLoadTesting.Framework.Core;
using Zoxive.HttpLoadTesting.Framework.Model;

namespace Infor.CPQ.ConfiguratorLoadTestFixtures
{
    public static class UserFactory
    {
        public static HttpUser CreateUser(string configuratorUrl, string key, string secret, string tenant, IReadOnlyList<ILoadTest> tests)
        {
            return new HttpUser(configuratorUrl, tests)
                {
                    AlterHttpClient = SetHttpClientProperties,
                    CreateHttpMessageHandler = SetHttpClientHandlerProperties,
                    AlterHttpRequestMessage = SetHttpRequestHeaders(key, secret, tenant)
                };
        }
        private static void SetHttpClientProperties(HttpClient httpClient)
        {
            httpClient.Timeout = new TimeSpan(0, 1, 0);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.ExpectContinue = false;
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        }
 
         private static HttpMessageHandler SetHttpClientHandlerProperties()
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = true,
                AutomaticDecompression = System.Net.DecompressionMethods.Deflate
            };
        }

        private static Action<HttpRequestMessage> SetHttpRequestHeaders(string key, string secret, string tenant)
        {
            return (request) => {
                request.SetOAuthHeader(new ZeroLeggedHandlerOptions()
                {
                    ConsumerKey = key,
                    ConsumerSecret = secret,
                    SignatureMethod = Infor.CPQ.Security.OAuth.Common.SignatureMethod.Hmacsha256,
                    SupportNonceValidation = true,
                    SupportVersionValidation = true
                });
                request.Headers.Add("X-Infor-TenantId", tenant);
                request.Headers.Add("Tenant", tenant);
                if (request.RequestUri.AbsoluteUri.Contains("DataImport"))
                {
                    request.Headers.Add("X-Infor-SecurityRoles", "CPQ-Designer");
                } 
            };
        }
    }
}