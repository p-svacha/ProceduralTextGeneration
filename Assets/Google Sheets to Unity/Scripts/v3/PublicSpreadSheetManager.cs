using Google.GData.Client;
using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using GoogleSheetsToUnity;
using System.Collections;
using GoogleSheetsToUnity.ThirdPary;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GoogleSheetsToUnity.Legacy
{
#if GSTU_Legacy

    public class PublicSpreadSheetManager
    {
        public class Row
        {
            public Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
        }

        GoogleSheetsToUnityConfig _config;
        private GoogleSheetsToUnityConfig config
        {
            get
            {
                if (_config == null)
                {
                    _config = (GoogleSheetsToUnityConfig)Resources.Load("GSTU_Config");
                }

                return _config;
            }
            set
            {
                _config = value;
            }
        }

        public List<string> titles = new List<string>();
        public Dictionary<string, Row> WorkSheetData = new Dictionary<string, Row>();

        //row titles are stored on
        public int titleRow
        {
            get
            {
                return titleRowActual + 2;
            }
            set
            {
                titleRowActual = value - 2;
            }
        }

        int titleRowActual;

        /// <summary>
        /// Loads a public spreadsheet and worksheet(worksheets start at 1 not 0)
        /// </summary>
        /// <param name="spreadsheetID"></param>
        /// <param name="worksheetNumber"></param>
        /// <returns></returns>
        public WorksheetData LoadPublicWorkSheet(string spreadsheetID, int worksheetNumber)
        {
            SecurityPolicy.Instate();

            SpreadsheetsService publicService = new SpreadsheetsService("Unity");

            ListQuery listQuery = new ListQuery("https://spreadsheets.google.com/feeds/list/" + spreadsheetID + "/" + worksheetNumber + "/public/values");

            ListFeed feed = publicService.Query(listQuery) as ListFeed;
            WorksheetData returnData = new WorksheetData();

            List<string> titles = GetColumnTitles(feed);

            if (titleRowActual > 0)
            {
                //remove all rows above the title row
                for (int i = 0; i <= titleRowActual; i++)
                {
                    feed.Entries.RemoveAt(0);
                }
            }

            foreach (ListEntry row in feed.Entries)
            {
                string rowTitle = row.Title.Text;
                RowData rowData = new RowData(rowTitle);

                int rowId = 0;
                foreach (ListEntry.Custom element in row.Elements)
                {
                    rowData.cells.Add(new CellData(element.Value, titles[rowId], rowTitle));
                    rowId++;
                }

                returnData.rows.Add(rowData);
            }

            return returnData;
        }

        List<string> GetColumnTitles(ListFeed feed)
        {
            List<string> titles = new List<string>();

            if (titleRowActual < 0)
            {
                ListEntry row = feed.Entries[0] as ListEntry;

                foreach (ListEntry.Custom element in row.Elements)
                {
                    titles.Add(element.LocalName);
                }
            }
            else
            {
                ListEntry row = feed.Entries[titleRowActual] as ListEntry;

                foreach (ListEntry.Custom element in row.Elements)
                {
                    titles.Add(element.Value);
                }
            }

            return titles;
        }
    }
#endif
}
