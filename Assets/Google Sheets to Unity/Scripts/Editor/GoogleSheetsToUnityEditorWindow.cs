using System.Net;
using UnityEditor;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
#if GSTU_Legacy
GoogleSheetsToUnity.Legacy
#endif

namespace GoogleSheetsToUnity.Editor
{
    public class GoogleSheetsToUnityEditorWindow : EditorWindow
    {
#if GSTU_Legacy
        OAuth2 oAuth2 = new OAuth2();
        SpreadSheetManager spreadSheet;
#endif

        const float DarkGray = 0.4f;
        const float LightGray = 0.9f;

        GoogleSheetsToUnityConfig config;
        private bool showSecret = false;

        int tabID = 0;

#if GSTU_Legacy
        class KnownData
        {
            public GS2U_SpreadSheet entry;
            public List<string> worksheets = new List<string>();

            public KnownData(GS2U_SpreadSheet _entry, List<string> _worksheets)
            {
                entry = _entry;
                worksheets = _worksheets;
            }
        }

        List<KnownData> knownData = new List<KnownData>();
        List<string> spreadsheetNames = new List<string>();

        int spreedsheetIndex = 0;
        private int lastSpreedsheetIndex;
        private bool isDebugOn;
#endif

        [MenuItem("Window/GSTU/Open Config")]
        static void Open()
        {
            GoogleSheetsToUnityEditorWindow win = EditorWindow.GetWindow<GoogleSheetsToUnityEditorWindow>("Google Sheets To Unity");
            ServicePointManager.ServerCertificateValidationCallback = Validator;

            win.Init();
        }

        public static bool Validator(object in_sender, X509Certificate in_certificate, X509Chain in_chain, SslPolicyErrors in_sslPolicyErrors)
        {
            return true;
        }

        public void Init()
        {
            config = (GoogleSheetsToUnityConfig)Resources.Load("GSTU_Config");
        }

        void OnGUI()
        {
            tabID = GUILayout.Toolbar(tabID, new string[] {"Private", "Private (Legacy)", "Public"});

            if (config == null)
            {
                Debug.LogError("Error: no config file");
                return;
            }

            switch (tabID)
            {
                case 0:
                    {
                        config.CLIENT_ID = EditorGUILayout.TextField("Client ID", config.CLIENT_ID);

                        GUILayout.BeginHorizontal();
                        if (showSecret)
                        {
                            config.CLIENT_SECRET = EditorGUILayout.TextField("Client Secret Code", config.CLIENT_SECRET);
                        }
                        else
                        {
                            config.CLIENT_SECRET = EditorGUILayout.PasswordField("Client Secret Code", config.CLIENT_SECRET);

                        }
                        showSecret = GUILayout.Toggle(showSecret, "Show");
                        GUILayout.EndHorizontal();

                        config.PORT = EditorGUILayout.IntField("Port number", config.PORT);

                        if (GUILayout.Button("Build Connection"))
                        {
                            GoogleAuthrisationHelper.BuildHttpListener();
                        }

                        break;
                    }
                case 1:
                    {
#if GSTU_Legacy
                        config.CLIENT_ID = EditorGUILayout.TextField("Client ID", config.CLIENT_ID);

                        GUILayout.BeginHorizontal();
                        if (showSecret)
                        {
                            config.CLIENT_SECRET = EditorGUILayout.TextField("Client Secret Code", config.CLIENT_SECRET);
                        }
                        else
                        {
                            config.CLIENT_SECRET = EditorGUILayout.PasswordField("Client Secret Code", config.CLIENT_SECRET);

                        }
                        showSecret = GUILayout.Toggle(showSecret, "Show");
                        GUILayout.EndHorizontal();

                        if (GUILayout.Button("Get Access Code"))
                        {
                            string authUrl = oAuth2.GetAuthURL();
                            Application.OpenURL(authUrl);
                        }

                        config.ACCESS_TOKEN = EditorGUILayout.TextField("Access Code", config.ACCESS_TOKEN);

                        if (GUILayout.Button("Authentication with Acceess Code"))
                        {
                            SecurityPolicy.Instate();
                            config.REFRESH_TOKEN = oAuth2.AuthWithAccessCode(config.ACCESS_TOKEN);
                        }

                        if (GUILayout.Button("Debug Information (WARNING: This may take some time)"))
                        {
                            knownData.Clear();
                            spreadsheetNames.Clear();
                            isDebugOn = true;

                            if (config.REFRESH_TOKEN != "")
                            {
                                if (spreadSheet == null)
                                {
                                    spreadSheet = new SpreadSheetManager();
                                }

                                var tempData = spreadSheet.GetAllSheets();
                                for (int i = 0; i < tempData.Count; i++)
                                {
                                    knownData.Add(new KnownData(tempData[i], tempData[i].GetAllWorkSheetsNames()));
                                    spreadsheetNames.Add(tempData[i].spreadsheetEntry.Title.Text);
                                }
                            }
                        }

                        if (isDebugOn)
                        {
                            DrawPreview();
                        }
#else
                        GUILayout.Label("This is the legacy version of GSTU and will be removed at a future date, if you wish to use it please press the button below");
                        if(GUILayout.Button("Use Legacy Version"))
                        {
                            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, (defines + ";" + "GSTU_Legacy"));
                        }
#endif
                        break;
                    }

                case 2:
                    {
                        config.API_Key = EditorGUILayout.TextField("API Key", config.API_Key);
                        break;
                    }
            }


            EditorUtility.SetDirty(config);
        }
#if GSTU_Legacy
        void DrawPreview()
        {
            if (spreadsheetNames.Count > 0)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Spreadsheets");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        spreedsheetIndex = GUILayout.SelectionGrid(spreedsheetIndex, spreadsheetNames.ToArray(), 1, GUILayout.Width(Screen.width / 2));
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Worksheets");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();

                        for (int i = 0; i < knownData[spreedsheetIndex].worksheets.Count; i++)
                        {
                            GUILayout.Label(knownData[spreedsheetIndex].worksheets[i]);
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
        }
#endif
    }
}