using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HookAppDiscord.Github
{
    public delegate void GithubWebhookDelivery(string eventName, string jsonBody);
}
