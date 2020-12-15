using JosephM.Xrm.Sms.Plugins.Services;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;

namespace Didasko.Xrm.Plugins.Services.GlobalSms
{
    /// <summary>
    /// The rest api class.
    /// </summary>
    public class GlobalSmsService : ISmsService
    {
        private Uri uri;

        private string Host {  get { return "api.smsglobal.com"; } }

        private string Port {  get { return "443"; } }

        public IGlobalSmsSettings SmsSettings { get; set; }

        public GlobalSmsService(IGlobalSmsSettings settings)
        {
            SmsSettings = settings;
        }

        public void SendSms(string mobileNumber, string message, string source)
        {
            sendSms(new SmsRequest
            {
                destination = mobileNumber,
                message = message,
                origin = source
            });
        }

        /// <summary>
        /// Sends an sms message.
        /// </summary>
        /// <returns>Task</returns>
        public void sendSms(SmsRequest payload)
        {
            var jsonString = ObjectToJsonString(payload);
            Post("sms", jsonString);
        }

        private void Post(string path, string data)
        {
            using (var client = new HttpClient())
            {
                string credentials = Credentials(path, "POST");

                client.BaseAddress = new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host));
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("MAC", credentials);
                var stringContent = new StringContent(data, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(uri.PathAndQuery, stringContent).Result;

                response.EnsureSuccessStatusCode();
            }
        }

        public string ObjectToJsonString<T>(T objectValue)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, objectValue);
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Compiles the mac oauth2 credentials.
        /// </summary>
        /// <param name="path">The request path.</param>
        /// <param name="method">The request method.</param>
        /// <returns>The credential string.</returns>
        private string Credentials(string path, string method = "GET", string filter = "")
        {
            uri = new Uri(string.Format("https://{0}/{1}/{2}/?{3}", Host, "v2", path, filter));

            string timestamp = ((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
            string nonce = Guid.NewGuid().ToString("N");
            string mac = string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n\n", timestamp, nonce, method, uri.PathAndQuery, uri.Host, Port);

            mac = Convert.ToBase64String((new HMACSHA256(Encoding.ASCII.GetBytes(SmsSettings.secret))).ComputeHash(Encoding.ASCII.GetBytes(mac)));

            return string.Format("id=\"{0}\", ts=\"{1}\", nonce=\"{2}\", mac=\"{3}\"", SmsSettings.key, timestamp, nonce, mac);
        }
    }
}
