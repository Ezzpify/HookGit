using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using log4net;
using Newtonsoft.Json;

namespace HookAppDiscord.WCF
{
    [ServiceContract]
    public interface IWCFEntryPoint
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "payload")]
        string Payload(Stream stream);
    }

    class WCFEntryPoint : IWCFEntryPoint
    {
        class PayloadResponse
        {
            public bool Success { get; set; }

            public string EventName { get; set; }

            public PayloadResponse(bool success, string eventName)
            {
                Success = success;
                EventName = eventName;
            }
        }

        public string Payload(Stream stream)
        {
            try
            {
                var properties = (HttpRequestMessageProperty)OperationContext.Current.IncomingMessageProperties.Values.ToArray()[3];
                string eventName = properties.Headers.Get("X-GitHub-Event");

                if (!string.IsNullOrWhiteSpace(eventName))
                {
                    StreamReader reader = new StreamReader(stream);
                    string jsonBody = reader.ReadToEnd();

                    WCFServer.DeliveryCallback(eventName, jsonBody);

                    return JsonConvert.SerializeObject(new PayloadResponse(true, eventName), Formatting.Indented);
                }
            }
            catch
            {
                return JsonConvert.SerializeObject(new PayloadResponse(false, "exception"), Formatting.Indented);
            }

            return JsonConvert.SerializeObject(new PayloadResponse(false, "unknown"), Formatting.Indented);
        }
    }
}
