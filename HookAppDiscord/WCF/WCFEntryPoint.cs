using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace HookAppDiscord.WCF
{
    [ServiceContract]
    public interface IWCFEntryPoint
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "payload")]
        string Payload(string json);
    }

    class WCFEntryPoint : IWCFEntryPoint
    {
        public string Payload(string id)
        {
            return "";
        }
    }
}
