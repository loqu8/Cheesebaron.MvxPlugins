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

using Cirrious.CrossCore;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Plugins;

namespace Cheesebaron.MvxPlugins.ModernHttpClient.Droid
{
    public class Plugin
        : IMvxPlugin
    {
        private DroidModernHttpClientConfiguration _config;
        private bool _loaded;

        public void Load()
        {
            if (_loaded) return;

            var instance = new HttpClientFactory
            {
                DefaultHandler = _config != null ? _config.HttpClientHandler : HttpClientHandlerType.OkHttpHandler
            };
            Mvx.RegisterSingleton<IHttpClientFactory>(instance);

            _loaded = true;
        }

        public void Configure(IMvxPluginConfiguration configuration)
        {
            if (configuration != null && !(configuration is DroidModernHttpClientConfiguration))
                throw new MvxException(
                    "Plugin configuration only supports instances of DroidModernHttpClientConfiguration, you provided {0}",
                    configuration.GetType().Name);

            _config = (DroidModernHttpClientConfiguration)configuration;
        }
    }

    public class DroidModernHttpClientConfiguration
        : IMvxPluginConfiguration
    {
        public DroidModernHttpClientConfiguration() 
        {
            HttpClientHandler = HttpClientHandlerType.OkHttpHandler;    
        }

        public HttpClientHandlerType HttpClientHandler { get; set; }
    }
}
