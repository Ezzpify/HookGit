using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HookAppDiscord.HookApp.DataHolders
{
    class ServerStats
    {
        public DateTime date { get; set; }

        public string error { get; set; }

        public long numOfUsers { get; set; }

        public long facebookUsers { get; set; }

        public long googleUsers { get; set; }

        public long numOfRates { get; set; }

        public long numOfMatches { get; set; }

        public long deletedUsers { get; set; }

        public long lastActiveUsers { get; set; }
    }
}
