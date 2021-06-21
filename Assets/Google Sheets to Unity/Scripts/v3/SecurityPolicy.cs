using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace GoogleSheetsToUnity.Legacy
{
#if GSTU_Legacy
    public class SecurityPolicy
    {
        public static bool Validator(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors policyErrors)
        {

            //*** Just accept and move on...
            Debug.Log("Validation successful!");
            return true;
        }

        public static void Instate()
        {

            ServicePointManager.ServerCertificateValidationCallback = Validator;
        }
    }
#endif
}