using System.Collections;
using System.Text;
using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using TinyJSON;
using UnityEngine;

public delegate void OnSpreedSheetLoaded(GstuSpreadSheet sheet);
namespace GoogleSheetsToUnity
{
    /// <summary>
    /// Partial class for the spreadsheet manager to handle all Public functions
    /// </summary>
    public partial class SpreadsheetManager
    {      
        static GoogleSheetsToUnityConfig _config;
        /// <summary>
        /// Reference to the config for access to the auth details
        /// </summary>
        public static GoogleSheetsToUnityConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = (GoogleSheetsToUnityConfig) Resources.Load("GSTU_Config");
                }

                return _config;
            }
            set { _config = value; }
        }
        
        /// <summary>
        /// Read a public accessable spreadsheet
        /// </summary>
        /// <param name="searchDetails"></param>
        /// <param name="callback">event that will fire after reading is complete</param>
        public static void ReadPublicSpreadsheet(GSTU_Search searchDetails, OnSpreedSheetLoaded callback)
        {
            if (string.IsNullOrEmpty(Config.API_Key))
            {
                Debug.Log("Missing API Key, please enter this in the confie settings");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + searchDetails.sheetId);
            sb.Append("/values");
            sb.Append("/" + searchDetails.worksheetName + "!" + searchDetails.startCell + ":" + searchDetails.endCell);
            sb.Append("?key=" + Config.API_Key);
            
            if (Application.isPlaying)
            {
                new Task(Read(new WWW(sb.ToString()), searchDetails.titleColumn, searchDetails.titleRow, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(Read(new WWW(sb.ToString()), searchDetails.titleColumn, searchDetails.titleRow, callback));
            }
#endif
        }

        /// <summary>
        /// Wait for the Web request to complete and then process the results
        /// </summary>
        /// <param name="www"></param>
        /// <param name="titleColumn"></param>
        /// <param name="titleRow"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static IEnumerator Read(WWW www, string titleColumn, int titleRow, OnSpreedSheetLoaded callback)
        {
            yield return www;

            ValueRange rawData = JSON.Load(www.text).Make<ValueRange>();
            GSTU_SpreadsheetResponce responce = new GSTU_SpreadsheetResponce(rawData);

            GstuSpreadSheet spreadSheet = new GstuSpreadSheet(responce, titleColumn, titleRow);

            if (callback != null)
            {
                callback(spreadSheet);
            }
        }
    }
}