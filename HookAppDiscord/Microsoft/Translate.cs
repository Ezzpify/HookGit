using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Web;
using RestSharp;
using RestSharp.Authenticators;
using Discord;
using Discord.WebSocket;

namespace HookAppDiscord.Microsoft
{
    class Translate
    {
        private string _azureKey;
        private string _translateTo;

        private string _accessToken;
        private DateTime _accessTokenAge;

        public Translate(string azureKey, string translateTo)
        {
            _azureKey = azureKey;
            _translateTo = translateTo;
            _accessTokenAge = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        }

        public string GetTranslatedMessage(SocketMessage arg)
        {
            string author = arg.Author.Username;
            string channel = arg.Channel.Name;
            string originalText = arg.Content;
            string translated = TranslateText(arg.Content);

            if (translated.Length > 0)
                return string.Format("```#{0}\n{1}: {2}\n\n({3})```", channel, author, translated, originalText);

            return string.Empty;
        }

        private string TranslateText(string text)
        {
            TimeSpan span = DateTime.Now.Subtract(_accessTokenAge);
            if (string.IsNullOrEmpty(_accessToken) || span.Minutes >= 5)
                _accessToken = GetAuthenticationToken(_azureKey);

            XmlDocument xmlDoc = new XmlDocument();

            string language = DetectLanguage(text, _accessToken);
            xmlDoc.LoadXml(language);

            if (xmlDoc.InnerText != "en")
            {
                string xml = GetTranslation(text, _translateTo, _accessToken);
                xmlDoc.LoadXml(xml);

                string innerText = Regex.Replace(xmlDoc.InnerText, @"\s", "");
                return HttpUtility.UrlDecode(innerText).Replace("+", " ");
            }

            return string.Empty;
        }

        private string DetectLanguage(string textToTranslate, string accessToken)
        {
            var client = new RestClient("http://api.microsofttranslator.com/V2/Http.svc/Detect");

            var request = new RestRequest(Method.GET);
            request.AddParameter("Authorization", "Bearer " + accessToken, ParameterType.HttpHeader);
            request.AddParameter("text", System.Net.WebUtility.UrlEncode(textToTranslate));

            var response = client.Execute(request);
            return response.Content;
        }

        private string GetTranslation(string textToTranslate, string language, string accessToken)
        {
            var client = new RestClient("http://api.microsofttranslator.com/v2/Http.svc/Translate");

            var request = new RestRequest(Method.GET);
            request.AddParameter("Authorization", "Bearer " + accessToken, ParameterType.HttpHeader);
            request.AddParameter("text", System.Net.WebUtility.UrlEncode(textToTranslate));
            request.AddParameter("to", language);
            request.AddParameter("contentType", "text/plain");

            var response = client.Execute(request);
            if (!response.IsSuccessful)
                return string.Empty;

            return response.Content;
        }

        private string GetAuthenticationToken(string key)
        {
            var client = new RestClient("https://api.cognitive.microsoft.com/sts/v1.0/issueToken");
            client.AddDefaultHeader("Ocp-Apim-Subscription-Key", key);

            var request = new RestRequest(Method.POST);
            var token = client.Execute(request);

            return token.Content;
        }
    }
}
