﻿namespace etcetera
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using RestSharp;

    public class EtcdClient
    {
        readonly IRestClient _client;
        readonly Uri _root;
        readonly Uri _keysRoot;

        public EtcdClient(Uri etcdLocation)
        {
            var uriBuilder = new UriBuilder(etcdLocation)
            {
                Path = ""
            };
            _root = uriBuilder.Uri;
            _keysRoot = new Uri(_root, "/v2/keys");
            _client = new RestClient(_root.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ttl">Time to live in seconds</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public EtcdResponse Set(string key, int ttl, object value)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var putRequest = new RestRequest(requestUrl, Method.PUT);
            putRequest.AddParameter("value", value);
            putRequest.AddParameter("ttl", ttl);

            var response = _client.Execute<EtcdResponse>(putRequest);
            return response.Data;
        }

        public EtcdResponse Set(string key, object value)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var putRequest = new RestRequest(requestUrl, Method.PUT);
            putRequest.AddParameter("value", value);

            var response = _client.Execute<EtcdResponse>(putRequest);
            return response.Data;
        }

        public EtcdResponse Get(string key)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var getRequest = new RestRequest(requestUrl, Method.GET);

            //needed due to issue 469 - https://github.com/coreos/etcd/issues/469
            getRequest.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };

            var response = _client.Execute<EtcdResponse>(getRequest);
            return response.Data;
        }


        public EtcdResponse Queue(string key, object value)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var postRequest = new RestRequest(requestUrl, Method.POST);
            postRequest.AddParameter("value", value);

            var response = _client.Execute<EtcdResponse>(postRequest);
            return response.Data;
        }

        public EtcdResponse Delete(string key)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var getRequest = new RestRequest(requestUrl, Method.DELETE);

            var response = _client.Execute<EtcdResponse>(getRequest);
            return response.Data;
        }

        public void Watch(string key, Action<EtcdResponse> followUp, bool recursive = false)
        {
            var requestUrl = _keysRoot.AppendPath(key);
            var getRequest = new RestRequest(requestUrl, Method.GET);
            getRequest.AddParameter("wait", true);
            if (recursive)
            {
                getRequest.AddParameter("recursive", recursive);
            }

            Task.Run(() =>
            {
                var response = _client.Execute<EtcdResponse>(getRequest);
                followUp(response.Data);
            });
            
        }

        //TODO: dry this up
        //TODO: add sorted to GET
        //TODO: create directories
        //TODO: listing directory
        //TODO: deleting directory
        //TODO: hidden nodes?
        //TODO: add / expose directory TTL
        //TODO: compare and swap
        //TODO: stats
    }

    public class EtcdResponse
    {
        public string Action { get; set; }
        public Node Node { get; set; }

        //ttl error
        public int? ErrorCode { get; set; }
        public string Cause { get; set; }
        public int? Index { get; set; }
        public string Message { get; set; }
    }
    public static class EtcResponseHelpers
    {
        public static int EtcIndex(this IRestResponse response)
        {
            return (int)response.Headers.First(x=>x.Name == "X-Etcd-Index").Value;
        }

        public static int EtcRaftIndex(this IRestResponse response)
        {
            return (int)response.Headers.First(x=>x.Name == "X-Raft-Index").Value;
        }

        public static int EtcRaftTerm(this IRestResponse response)
        {
            return (int)response.Headers.First(x => x.Name == "X-Raft-Term").Value;
        }
    }
    public class Node
    {
        public int CreatedIndex { get; set; }
        public string Key { get; set; }
        public int ModifiedIndex { get; set; }
        public string Value { get; set; }
        public int? Ttl { get; set; }
        public DateTime? Expiration { get; set; }
    }
    public static class UriHelpers
    {
        public static Uri AppendPath(this Uri uri, string path)
        {
            var path1 = uri.AbsolutePath.TrimEnd(new []
            {
                '/'
            }) + "/" + path;
            return new UriBuilder(uri.Scheme, uri.Host, uri.Port, path1, uri.Query).Uri;
        }
    }
}