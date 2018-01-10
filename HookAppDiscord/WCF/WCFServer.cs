using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Threading;
using HookAppDiscord.Github;

namespace HookAppDiscord.WCF
{
    class WCFServer
    {
        public static GithubWebhookDelivery DeliveryCallback;

        private WebServiceHost _host;
        private ServiceEndpoint _ep;
        private ServiceDebugBehavior _stp;

        public WCFServer(GithubWebhookDelivery delivery)
        {
            DeliveryCallback = delivery;
        }

        public void Start()
        {
            _host = new WebServiceHost(typeof(WCFEntryPoint), new Uri("http://localhost:9500"));
            _ep = _host.AddServiceEndpoint(typeof(IWCFEntryPoint), new WebHttpBinding(), "");
            _stp = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
            _stp.HttpHelpPageEnabled = false;
            _host.Open();

            while (_host.State != CommunicationState.Opened)
            {
                Console.WriteLine("Waiting for WCF server...");
                Thread.Sleep(1000);
            }
        }
    }
}
