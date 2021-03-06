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
using Android.App;
using Android.OS;
using Android.Views;
using Android.Webkit;
using AppCompatExtensions.Droid.v7;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Plugins.Messenger;
using Java.Interop;

namespace Cheesebaron.MvxPlugins.AzureAccessControl.Droid.Views
{
    [Activity(Label = "Web Log In")]
    public class AccessControlWebAuthActivity 
        : MvxActionBarCompatAcitivity
    {
        private IMvxMessenger _messageHub;
        private WebView _webView;
        private external _notify;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _messageHub = Mvx.Resolve<IMvxMessenger>();

            var url = Intent.GetStringExtra("cheesebaron.mvxplugins.azureaccesscontrol.droid.Url");

            Window.RequestFeature(WindowFeatures.Progress);

            _webView = new WebView(this)
            {
                VerticalScrollBarEnabled = true,
                HorizontalScrollBarEnabled = true,
                ScrollbarFadingEnabled = false,
                ScrollBarStyle = ScrollbarStyles.OutsideOverlay
            };

            _webView.Settings.JavaScriptEnabled = true;
            _webView.Settings.SetSupportZoom(true);
            _webView.Settings.BuiltInZoomControls = true;
            _webView.Settings.LoadWithOverviewMode = true;
            _webView.Settings.UseWideViewPort = true;

            _notify = new external();
            _notify.GotSecurityTokenResponse += GotSecurityTokenResponse;

            _webView.AddJavascriptInterface(_notify, "external");
            _webView.SetWebViewClient(new AuthWebViewClient());
            _webView.SetWebChromeClient(new AuthWebChromeClient(this));

            _webView.LoadUrl(url);

            AddContentView(_webView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }

        private async void GotSecurityTokenResponse(object sender, RequestSecurityTokenResponseEventArgs e)
        {
            _notify.GotSecurityTokenResponse -= GotSecurityTokenResponse;
            if (e.Error == null)
            {
                var token = await RequestSecurityTokenResponse.FromJSONAsync(e.Response);
                _messageHub.Publish(new RequestTokenMessage(this) {TokenResponse = token});

                if (Parent == null)
                    SetResult(Result.Ok);
                else
                    Parent.SetResult(Result.Ok);
            }
            else
            {
                _messageHub.Publish(new RequestTokenMessage(this) { TokenResponse = null });

                if (Parent == null)
                    SetResult(Result.Canceled);
                else
                    Parent.SetResult(Result.Canceled);
            }
            Finish();
        }

        public class external : Java.Lang.Object
        {
            public event EventHandler<RequestSecurityTokenResponseEventArgs> GotSecurityTokenResponse;

            [Export("Notify")]
            [JavascriptInterface]
            public void Notify(string securityTokenResponse)
            {
                Exception ex = null;

                if (string.IsNullOrEmpty(securityTokenResponse))
                    ex = new ArgumentNullException("securityTokenResponse", "Did not recieve a Token Response");

                if (GotSecurityTokenResponse != null)
                    GotSecurityTokenResponse(this, new RequestSecurityTokenResponseEventArgs(securityTokenResponse, ex));
            }

            [Export("notify")]
            [JavascriptInterface]
            public void notify(string securityTokenResponse)
            {
                Exception ex = null;

                if (string.IsNullOrEmpty(securityTokenResponse))
                    ex = new ArgumentNullException("securityTokenResponse", "Did not recieve a Token Response");

                if (GotSecurityTokenResponse != null)
                    GotSecurityTokenResponse(this, new RequestSecurityTokenResponseEventArgs(securityTokenResponse, ex));
            }
        }

        private class AuthWebViewClient : WebViewClient { }

        private class AuthWebChromeClient : WebChromeClient
        {
            private readonly Activity _parentActivity;
            private readonly string _title;

            public AuthWebChromeClient(Activity parentActivity)
            {
                _parentActivity = parentActivity;
                _title = parentActivity.Title;
            }

            public override void OnProgressChanged(WebView view, int newProgress)
            {
                _parentActivity.Title = string.Format("Loading {0}%", newProgress);
                _parentActivity.SetProgress(newProgress * 100);

                if (newProgress == 100) _parentActivity.Title = _title;
            }
        }
    }
}