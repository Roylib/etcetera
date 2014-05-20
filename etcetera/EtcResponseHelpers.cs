﻿namespace etcetera
{
    using System.Linq;
    using RestSharp;

    public static class EtcResponseHelpers
    {
        public static int EtcIndex(this IRestResponse response)
        {
            return int.Parse(response.Headers.First(x => x.Name == "X-Etcd-Index").Value.ToString());
        }

        public static int EtcRaftIndex(this IRestResponse response)
        {
            return int.Parse(response.Headers.First(x => x.Name == "X-Raft-Index").Value.ToString()); ;
        }

        public static int EtcRaftTerm(this IRestResponse response)
        {
            return int.Parse(response.Headers.First(x => x.Name == "X-Raft-Term").Value.ToString()); ;
        }
    }
}