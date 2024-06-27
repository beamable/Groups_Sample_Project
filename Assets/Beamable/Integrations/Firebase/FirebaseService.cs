using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Beamable.Server.Api.RealmConfig;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using UnityEngine;

namespace Beamable.Integrations.Firebase
{
    public sealed class FirebaseService
    {
        private const string Namespace = "firebase";
        private const string BaseURL = "base_url";
        private const string ProjectId = "project_id";
        private const string Scope = "scope";
        private const string ServiceAccount = "service_account";
            
        private readonly IMicroserviceRealmConfigService _realmConfigService;
        private string _baseURL;
        private string _projectId;
        private string _scope;
        private string _serviceAccount;
        private RealmConfig _settings;
        private HttpClient _httpClient;
        private ICredential _googleCredential;

        public FirebaseService(IMicroserviceRealmConfigService realmConfigService)
        {
            _realmConfigService = realmConfigService;
            _httpClient = new HttpClient();
        }

        public async Task Init()
        {
            _settings = await _realmConfigService.GetRealmConfigSettings();

            _baseURL = _settings.GetSetting(Namespace, BaseURL);
            _projectId = _settings.GetSetting(Namespace, ProjectId);
            _scope = _settings.GetSetting(Namespace, Scope);
            _serviceAccount = _settings.GetSetting(Namespace, ServiceAccount);
            
            _httpClient.BaseAddress = new Uri(_baseURL);
            _googleCredential = GoogleCredential.FromJson(_serviceAccount).CreateScoped(_scope).UnderlyingCredential;
        }
        
        public async Task<FirebaseMessageResponse> SendMessage(string token, string title, string body)
        {
            var accessToken = await _googleCredential.GetAccessTokenForRequestAsync();
            var messageContent = new { message = new { token, notification = new { title, body } } };
            var requestUrl = $"/v1/projects/{_projectId}/messages:send";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(messageContent), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<FirebaseMessageResponse>(responseContent);
                return result;
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return new FirebaseMessageResponse
            {
                name = string.Empty,
                errorMessage = errorMessage
            };
        }
    }

    [Serializable]
    public class FirebaseMessageResponse
    {
        public string name;
        public string errorMessage;
    }
}
