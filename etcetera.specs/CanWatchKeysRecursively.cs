using System;
using System.Collections.Generic;

namespace etcetera.specs
{
    using System.Threading;
    using Should;
    using Xunit;

    public class CanWatchKeysRecursively :
        EtcdBase
    {
        ManualResetEvent _wasHit;

        [Fact]
        public void ActionIsSet()
        {
            _wasHit = new ManualResetEvent(false);
            Client.Set("bob/" + AKey, "wassup");

            Client.Watch("bob", resp =>
            {
                _wasHit.Set();
            }, true);

            Client.Set("bob/" + AKey, "nope");
            _wasHit.WaitOne(1000).ShouldBeTrue();
        }
    }

    public class FireWatch : EtcdBase
    {
        public static int? Index = 0;
        private List<string> myList = new List<string>();

        public void ActionFired(EtcdResponse resp)
        {
            var response = Client.Get("bob");
            Index = response.Index;
            String sIndex = response.Index.ToString();
            String sTime = DateTime.Now.ToString("yy-mm-dd hh:mm:ss.fff");
            myList.Add(sIndex + " " + sTime + " ");
            
            //Client.Watch("bob", resp => ActionFired(), true, 20, response.Index + 1);
        }


        [Fact] 
        public void ActionIsSet()
        {
            Client.DeleteDir("/", true);
            Client.Set("bob/" + AKey, "wassup");

            var response = Client.Get("bob", true);
            Index = response.Index;
            String sIndex = response.Index.ToString();
            String sTime = DateTime.Now.ToString("yy-mm-dd hh:mm:ss.fff");
            myList.Add(sIndex + " " + sTime + " ");

            //Client.Watch("bob", resp => ActionFired(), true, null, EtcResponseHelpers.EtcIndex(response) + 1);

            Client.Watch("/", resp => ActionFired(resp), true, 2000, response.Index + 1);

//            Client.Set("bob/" + AKey, "nope");
//
//            Client.Set("bob/" + AKey, "nopenope");
//
//            Client.Set("bob/" + AKey, "nopenopenope");

//            Client.Delete("bob/" + AKey);


            var ResponseAfterChange = Client.Get("/", true);

            Thread.Sleep(3000);

            ResponseAfterChange.Index.ShouldEqual(Index);
        }
    }
}