using GoogleSheetsToUnity.ThirdPary;
using System;
using System.Collections;
using System.Net;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Networking;

namespace GoogleSheetsToUnity
{
    public class GoogleAuthrisationHelper : MonoBehaviour
    {
        static string _authToken = "";

        static HttpListener _httpListener;
        static string _htmlResponseContent = "<h1>Google Sheets and Unity are now linked, you may close this window</h1>"; //message shown after connection has been set up

        private static object _notifyAuthTokenLock = new object();
        private static bool _shouldNotifyAuthTokenReceived = false;
        private static Action<string> _onComplete;

#if UNITY_EDITOR
        public static void BuildHttpListener()
        {
            if (_httpListener != null)
            {
                _httpListener.Abort();
                _httpListener = null;
            }
            _onComplete = null;

            string serverUrl = string.Format("http://127.0.0.1:{0}", SpreadsheetManager.Config.PORT);

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(serverUrl + "/");
            _httpListener.Start();
            _httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), _httpListener);

            _onComplete += GetAuthComplete;

            string request = "https://accounts.google.com/o/oauth2/v2/auth?";
            request += "client_id=" + Uri.EscapeDataString(SpreadsheetManager.Config.CLIENT_ID) + "&";
            request += "redirect_uri=" + Uri.EscapeDataString(serverUrl) + "&";
            request += "response_type=" + "code" + "&";
            request += "scope=" + Uri.EscapeDataString("https://www.googleapis.com/auth/spreadsheets") + "&";
            request += "access_type=" + "offline" + "&";
            request += "prompt=" + "consent" + "&";


            Application.OpenURL(request);
        }

        static void GetAuthComplete(string authToken)
        {
            string serverUrl = string.Format("http://127.0.0.1:{0}", SpreadsheetManager.Config.PORT);

            Debug.Log(authToken);
            Debug.Log("Auth Token = " + authToken);

            WWWForm f = new WWWForm();
            f.AddField("code", authToken);
            f.AddField("client_id", SpreadsheetManager.Config.CLIENT_ID);
            f.AddField("client_secret", SpreadsheetManager.Config.CLIENT_SECRET);
            f.AddField("redirect_uri", serverUrl);
            f.AddField("grant_type", "authorization_code");
            f.AddField("scope", "");

            EditorCoroutineRunner.StartCoroutine(GetToken(f));
        }

        static IEnumerator GetToken(WWWForm f)
        {
            using (UnityWebRequest request = UnityWebRequest.Post("https://accounts.google.com/o/oauth2/token", f))
            {
                yield return request.SendWebRequest();

                SpreadsheetManager.Config.gdr = JsonUtility.FromJson<GoogleDataResponse>(request.downloadHandler.text);
                SpreadsheetManager.Config.gdr.nextRefreshTime = DateTime.Now.AddSeconds(SpreadsheetManager.Config.gdr.expires_in);
                EditorUtility.SetDirty(SpreadsheetManager.Config);
                AssetDatabase.SaveAssets();
            }
        }

        static void ListenerCallback(IAsyncResult result)
        {
            if (_httpListener != null)
            {
                try
                {
                    HttpListenerContext context = _httpListener.EndGetContext(result);
                    HandleListenerContextResponse(context);
                    ProcessListenerContext(context);

                    context.Response.Close();
                    _httpListener.BeginGetContext(ListenerCallback, _httpListener); // EndGetContext above ends the async listener, so we need to start it up again to continue listening.
                }
                catch (ObjectDisposedException)
                {
                    // Intentionally ignoring this exception because it will be thrown when we stop listening.
                }
                catch (Exception exception)
                {
                    Debug.Log(exception.Message + " : " + exception.StackTrace); // Just in case...
                }
            }
        }

        static void ProcessListenerContext(HttpListenerContext context)
        {
            // Attempt to pull out the URI fragment as a part of the query string.
            string uriFragment = context.Request.QueryString["code"];
            if (uriFragment != null)
            { // If it worked, that means we're being passed the auth token from Instagram, so pull it out and notify that we received it.
                string authToken = uriFragment.Replace("access_token=", "");
                NotifyAuthTokenReceived(authToken);
            }
        }

        /// <summary>
        /// Child classes should call this once the auth token has been successfully retrieved.</summary>
        static void NotifyAuthTokenReceived(string authToken)
        {
            lock (_notifyAuthTokenLock)
            {
                // We're not directly calling _onComplete() here because we're still on HttpListener's async thread.
                // We need _onComplete() to be called on the main thread, so we store the auth token and set a flag
                // that will tell us when we should call _onComplete() in the Update() method, which always executes
                // on the main thread.
                _authToken = authToken;
                _shouldNotifyAuthTokenReceived = true;

                    EditorCoroutineRunner.StartCoroutine(CheckForTokenRecieve());
            }
        }


        //Background Processes....
        static IEnumerator CheckForTokenRecieve()
        {
            lock (_notifyAuthTokenLock)
            {
                // using a lock here because we'll be modifying _shouldNotifyAuthTokenReceived on both the main thread and on HttpListener's async thread.
                if (_shouldNotifyAuthTokenReceived)
                {
                    if (_onComplete != null)
                    {
                        _onComplete(_authToken);
                    }
                    _shouldNotifyAuthTokenReceived = false;
                }
                else
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Some HTML response content was passed in to the StartListening() method, and this is where we display it to the user.</summary>
        static void HandleListenerContextResponse(HttpListenerContext context)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(_htmlResponseContent);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

#endif

        /// <summary>
        /// checks if time has expired far enough that a new auth token needs to be issued
        /// </summary>
        /// <returns></returns>
        public static IEnumerator CheckForRefreshOfToken()
        {
            if (DateTime.Now > SpreadsheetManager.Config.gdr.nextRefreshTime)
            {
                Debug.Log("Refreshing Token");

                WWWForm f = new WWWForm();
                f.AddField("client_id", SpreadsheetManager.Config.CLIENT_ID);
                f.AddField("client_secret", SpreadsheetManager.Config.CLIENT_SECRET);
                f.AddField("refresh_token", SpreadsheetManager.Config.gdr.refresh_token);
                f.AddField("grant_type", "refresh_token");
                f.AddField("scope", "");

                using (UnityWebRequest request = UnityWebRequest.Post("https://www.googleapis.com/oauth2/v4/token", f))
                {
                    yield return request.SendWebRequest();

                    GoogleDataResponse newGdr = JsonUtility.FromJson<GoogleDataResponse>(request.downloadHandler.text);
                    SpreadsheetManager.Config.gdr.access_token = newGdr.access_token;
                    SpreadsheetManager.Config.gdr.nextRefreshTime = DateTime.Now.AddSeconds(newGdr.expires_in);

#if UNITY_EDITOR
                    EditorUtility.SetDirty(SpreadsheetManager.Config);
                    AssetDatabase.SaveAssets();
#endif
                }
            }
        }
    }
}
