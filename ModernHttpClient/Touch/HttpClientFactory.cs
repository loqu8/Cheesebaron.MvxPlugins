//---------------------------------------------------------------------------------
// Copyright 2013 Tomasz Cielecki (tomasz@ostebaronen.dk)
// Licensed under the Apache License, Version 2.0 (the "License"); 
// You may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 

// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, 
// INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR 
// CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
// MERCHANTABLITY OR NON-INFRINGEMENT. 

// See the Apache 2 License for the specific language governing 
// permissions and limitations under the License.
//---------------------------------------------------------------------------------

using System;
using System.Net.Http;
using Cirrious.CrossCore;
using ModernHttpClient;

namespace Cheesebaron.MvxPlugins.ModernHttpClient.Touch
{
    public class HttpClientFactory : IHttpClientFactory
    {
        public HttpClientHandlerType DefaultHandler { get; set; }

        public HttpClient Get()
        {
            var handler = GetHandler();
            var client = Get(handler);
            return client;
        }

        public HttpMessageHandler GetHandler()
        {
            return GetHandler(DefaultHandler);
        }

        public HttpMessageHandler GetHandler(HttpClientHandlerType handlerType)
        {
            switch (handlerType)
            {
                case HttpClientHandlerType.CFNetworkHandler:
                    return new CFNetworkHandler();
                case HttpClientHandlerType.HttpClientHandler:
                    return new HttpClientHandler();
                case HttpClientHandlerType.OkHttpHandler:
                    Mvx.TaggedTrace("HttpClientFactory", "Cannot use OkHttpNetworkHandler on iOS defaulting to AFNetworkHandler");
                    return new AFNetworkHandler();
                default:
                    return new AFNetworkHandler();
            }
        }

        public HttpClient Get(HttpMessageHandler handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");
            var client = new HttpClient(handler);
            return client;
        }
    }
}