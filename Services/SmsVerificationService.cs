using Newtonsoft.Json;
using Ord.Accounts.Models.Sms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ord.Accounts.Services
{
    public class SmsVerificationService : ISmsVerificationService
    {
        private readonly string authUrl = "https://rest.smsportal.com/v1/Authentication";
        private readonly string smsUrl = "https://rest.smsportal.com/v1/BulkMessages";

        private async Task<string> GetAuthToken()
        {
            using (var httpClient = new System.Net.Http.HttpClient {  })
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", 
                    Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"ca72d377-a5ce-4e63-9cc9-534460c0680d:" +
                    $"/UYC/VE65TzakkLe7Csy36D13flQQDWW")));

                var response = await httpClient.GetAsync(authUrl);
                var httpStringContent = await response.Content.ReadAsStringAsync();

                var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(httpStringContent);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    if (responseObject != null)
                        return responseObject.token;
            }

            return null;
        }

        public async Task<bool> SendVerificationCode(string mobileNumber, string verificationCode)
        {
            var authToken = await GetAuthToken();
            if (authToken != null || authToken != string.Empty)
            {
                using (var httpClient = new System.Net.Http.HttpClient { })
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                    var httpContent = new System.Net.Http.StringContent(JsonConvert.SerializeObject(new
                    {
                        Messages = new List<SmsMessage>
                        {
                            new SmsMessage
                            {
                                Content = $"Verification code: {verificationCode}",
                                Destination = mobileNumber
                            }
                        }
                    }), Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(smsUrl, httpContent);
                    var httpStringContent = await response.Content.ReadAsStringAsync();

                    var responseObject = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(httpStringContent);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return true;
                }
            }
            return false;
        }
    }
}
