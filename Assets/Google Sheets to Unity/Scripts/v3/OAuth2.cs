using Google.GData.Client;
using System;
using UnityEngine;

namespace GoogleSheetsToUnity.Legacy
{
#if GSTU_Legacy
    public class OAuth2
  {

    string SCOPE = "https://spreadsheets.google.com/feeds";
    string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";

   GoogleSheetsToUnityConfig config;

    public string GetAuthURL()
    {
            if(config == null)
            {
                config = (GoogleSheetsToUnityConfig)UnityEngine.Resources.Load("GSTU_Config");
            }

      OAuth2Parameters parameters = new OAuth2Parameters();
      parameters.ClientId = config.CLIENT_ID;
      parameters.ClientSecret = config.CLIENT_SECRET;
      parameters.RedirectUri = REDIRECT_URI;
      parameters.Scope = SCOPE;
      return OAuthUtil.CreateOAuth2AuthorizationUrl(parameters);
    }

    public string AuthWithAccessCode(string accessCode)
    {
            if (config == null)
            {
                config = (GoogleSheetsToUnityConfig)UnityEngine.Resources.Load("GSTU_Config");
            }

            OAuth2Parameters parameters = new OAuth2Parameters();
      parameters.ClientId = config.CLIENT_ID;
      parameters.ClientSecret = config.CLIENT_SECRET;
      parameters.RedirectUri = REDIRECT_URI;
      parameters.Scope = SCOPE;
      parameters.AccessCode = accessCode;
      
      OAuthUtil.GetAccessToken(parameters);
      return parameters.RefreshToken;
    }

    public OAuth2Parameters GetOAuth2Parameter(string refreshToken)
    {
            if (config == null)
            {
                config = (GoogleSheetsToUnityConfig)UnityEngine.Resources.Load("GSTU_Config");
            }

            OAuth2Parameters parameters = new OAuth2Parameters();
      parameters.ClientId = config.CLIENT_ID;
      parameters.ClientSecret = config.CLIENT_SECRET;
      parameters.RefreshToken = refreshToken;
      OAuthUtil.RefreshAccessToken(parameters);
      return parameters;
    }
  }
#endif
}
