using System.Collections;
using System.Collections.Generic;
using System.Text;
using GoogleSheetsToUnity.ThirdPary;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using TinyJSON;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Linq;

namespace GoogleSheetsToUnity
{
    /// <summary>
    /// Partial class for the spreadsheet manager to handle all private functions
    /// </summary>
    public partial class SpreadsheetManager
    {   
        /// <summary>
        /// Chekcs for a valid token and if its out of date attempt to refresh it
        /// </summary>
        /// <returns></returns>
        static IEnumerator CheckForRefreshToken()
        {
            if (Application.isPlaying)
            {
                yield return new Task(GoogleAuthrisationHelper.CheckForRefreshOfToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return EditorCoroutineRunner.StartCoroutine(GoogleAuthrisationHelper.CheckForRefreshOfToken());
            }
#endif
        }    

        /// <summary>
        /// Reads information from a spreadsheet
        /// </summary>
        /// <param name="search"></param>
        /// <param name="callback"></param>
        /// <param name="containsMergedCells"> does the spreadsheet contain merged cells, will attempt to group these by titles</param>
        public static void Read(GSTU_Search search, UnityAction<GstuSpreadSheet> callback, bool containsMergedCells = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell + ":" + search.endCell);
            sb.Append("?access_token=" + Config.gdr.access_token);

            UnityWebRequest request = UnityWebRequest.Get(sb.ToString());

            if (Application.isPlaying)
            {
                new Task(Read(request, search, containsMergedCells, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(Read(request,  search, containsMergedCells, callback));
            }
#endif
        }
        
        /// <summary>
        /// Reads the spread sheet and callback with the results
        /// </summary>
        /// <param name="request"></param>
        /// <param name="search"></param>
        /// <param name="containsMergedCells"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static IEnumerator Read(UnityWebRequest request, GSTU_Search search, bool containsMergedCells, UnityAction<GstuSpreadSheet> callback)
        {
            if (Application.isPlaying)
            {
                yield return new Task(CheckForRefreshToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return EditorCoroutineRunner.StartCoroutine(CheckForRefreshToken());
            }
#endif

            using (request)
            {
                yield return request.SendWebRequest();

                if(string.IsNullOrEmpty(request.downloadHandler.text) || request.downloadHandler.text == "{}")
                {
                    Debug.LogWarning("Unable to Retreive data from google sheets");
                    yield break;
                }


                ValueRange rawData = JSON.Load(request.downloadHandler.text).Make<ValueRange>();
                GSTU_SpreadsheetResponce responce = new GSTU_SpreadsheetResponce(rawData);

                //if it contains merged cells then process a second set of json data to know what these cells are
                if (containsMergedCells)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
                    sb.Append("/" + search.sheetId);
                    sb.Append("?access_token=" + Config.gdr.access_token);

                    UnityWebRequest request2 = UnityWebRequest.Get(sb.ToString());

                    yield return request2.SendWebRequest();

                    SheetsRootObject root = JSON.Load(request2.downloadHandler.text).Make<SheetsRootObject>();
                    responce.sheetInfo = root.sheets.FirstOrDefault(x => x.properties.title == search.worksheetName);
                }

                if (callback != null)
                {
                    callback(new GstuSpreadSheet(responce, search.titleColumn,search.titleRow));
                }
            }
        }


        /// <summary>
        /// Updates just the cell pased in as the startCell value of the search parameters
        /// </summary>
        /// <param name="search"></param>
        /// <param name="inputData"></param>
        /// <param name="callback"></param>
        public static void Write(GSTU_Search search, string inputData, UnityAction callback)
        {
            Write(search, new ValueRange(inputData), callback);
        }

        /// <summary>
        /// Writes data to a spreadsheet
        /// </summary>
        /// <param name="search"></param>
        /// <param name="inputData"></param>
        /// <param name="callback"></param>
        public static void Write(GSTU_Search search, ValueRange inputData, UnityAction callback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell + ":" + search.endCell);
            sb.Append("?valueInputOption=USER_ENTERED");
            sb.Append("&access_token=" + Config.gdr.access_token);

            string json = JSON.Dump(inputData, EncodeOptions.NoTypeHints);
            byte[] bodyRaw = new UTF8Encoding().GetBytes(json);

            UnityWebRequest request = UnityWebRequest.Put(sb.ToString(), bodyRaw);

            if (Application.isPlaying)
            {
                new Task(Write(request, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(Write(request, callback));
            }
#endif
        }

        static IEnumerator Write(UnityWebRequest request, UnityAction callback)
        {
            if (Application.isPlaying)
            {
                yield return new Task(CheckForRefreshToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return EditorCoroutineRunner.StartCoroutine(CheckForRefreshToken());
            }
#endif

            using (request)
            {
                yield return request.SendWebRequest();

                if (callback != null)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// Writes a batch update to a spreadsheet
        /// </summary>
        /// <param name="search"></param>
        /// <param name="requestData"></param>
        /// <param name="callback"></param>
        public static void WriteBatch(GSTU_Search search, BatchRequestBody requestData, UnityAction callback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values:batchUpdate");
            sb.Append("?access_token=" + Config.gdr.access_token);


            string json = JSON.Dump(requestData, EncodeOptions.NoTypeHints);
            UnityWebRequest request = UnityWebRequest.Post(sb.ToString(), "");
            byte[] bodyRaw = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            if (Application.isPlaying)
            {
                new Task(WriteBatch(request, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(WriteBatch(request, callback));
            }
#endif
        }

        static IEnumerator WriteBatch(UnityWebRequest request, UnityAction callback)
        {
            if (Application.isPlaying)
            {
                yield return new Task(CheckForRefreshToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return EditorCoroutineRunner.StartCoroutine(CheckForRefreshToken());
            }
#endif

            using (request)
            {
                yield return request.SendWebRequest();

                if (callback != null)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// Adds the data to the next avaiable space to write it after the startcell
        /// </summary>
        /// <param name="search"></param>
        /// <param name="inputData"></param>
        /// <param name="callback"></param>
        public static void Append(GSTU_Search search, ValueRange inputData, UnityAction callback)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("https://sheets.googleapis.com/v4/spreadsheets");
            sb.Append("/" + search.sheetId);
            sb.Append("/values");
            sb.Append("/" + search.worksheetName + "!" + search.startCell);
            sb.Append(":append");
            sb.Append("?valueInputOption=USER_ENTERED");
            sb.Append("&access_token=" + Config.gdr.access_token);

            string json = JSON.Dump(inputData, EncodeOptions.NoTypeHints);

            UnityWebRequest request = UnityWebRequest.Post(sb.ToString(), "");

            //have to do this cause unitywebrequest post will nto accept json data corrently...
            byte[] bodyRaw = new UTF8Encoding().GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            if (Application.isPlaying)
            {
                new Task(Append(request, callback));
            }
#if UNITY_EDITOR
            else
            {
                EditorCoroutineRunner.StartCoroutine(Append(request, callback));
            }
#endif
        }

        static IEnumerator Append(UnityWebRequest request, UnityAction callback)
        {
            if (Application.isPlaying)
            {
                yield return new Task(CheckForRefreshToken());
            }
#if UNITY_EDITOR
            else
            {
                yield return EditorCoroutineRunner.StartCoroutine(CheckForRefreshToken());
            }
#endif

            using (request)
            {
                yield return request.SendWebRequest();

                if (callback != null)
                {
                    callback();
                }
            }
        }
    }
}