using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Web;
using log4net;
using RestSharp;
using RestSharp.Authenticators;
using Discord;
using Discord.WebSocket;
using HookAppDiscord.Discord;

namespace HookAppDiscord.Microsoft
{
    class Translate
    {
        private ILog _log;

        private string _azureKey;
        private string _translateTo;

        private string _accessToken;
        private DateTime _accessTokenAge;

        public Translate(ILog log, string azureKey, string translateTo)
        {
            _log = log;
            _azureKey = azureKey;
            _translateTo = translateTo;
            _accessTokenAge = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        }

        public EmbedBuilder GetTranslatedMessage(SocketMessage arg, string message)
        {
            _log.Info($"Getting translation from Azure service for message: {arg.Content}");

            string author = arg.Author.Username;
            string channel = arg.Channel.Name;
            string originalText = arg.Content;
            string translated = TranslateText(message);

            if (translated.Length > 0)
            {
                _log.Info($"Translation successful. Returning formatted response.");
                return DiscordMessageFormatter.GetTranslationMessage
                    (new DataHolders.Translation()
                    {
                        Author = author,
                        Channel = channel,
                        OriginalText = originalText,
                        TranslatedText = translated
                    });
            }

            return null;
        }

        private string TranslateText(string text)
        {
            TimeSpan span = DateTime.Now.Subtract(_accessTokenAge);
            if (string.IsNullOrEmpty(_accessToken) || span.Minutes >= 5)
            {
                _log.Info($"Azure accesstoken expired. Getting new token.");
                _accessToken = GetAuthenticationToken(_azureKey);
                _accessTokenAge = DateTime.Now;
            }

            XmlDocument xmlDoc = new XmlDocument();

            string language = DetectLanguage(text, _accessToken);
            xmlDoc.LoadXml(language);

            if (xmlDoc.InnerText != "en")
            {
                string xml = GetTranslation(text, _translateTo, xmlDoc.InnerText, _accessToken);
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    xmlDoc.LoadXml(xml);
                    string innerText = Regex.Replace(xmlDoc.InnerText, @"\s", "");
                    return HttpUtility.UrlDecode(innerText).Replace("+", " ");
                }
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

        private string GetTranslation(string textToTranslate, string language, string to,  string accessToken)
        {
            var client = new RestClient("http://api.microsofttranslator.com/v2/Http.svc/Translate");

            var request = new RestRequest(Method.GET);
            request.AddParameter("Authorization", "Bearer " + accessToken, ParameterType.HttpHeader);
            request.AddParameter("text", System.Net.WebUtility.UrlEncode(textToTranslate));
            request.AddParameter("to", language);
            request.AddParameter("from", language);
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
