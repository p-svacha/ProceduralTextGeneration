using Google.GData.Client;
using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using GoogleSheetsToUnity.Utils;

namespace GoogleSheetsToUnity.Legacy
{
#if GSTU_Legacy
    public class SpreadSheetManager
    {
        public static SpreadsheetsService service;
        GoogleSheetsToUnityConfig config;

        public Dictionary<string, Row> Data = new Dictionary<string, Row>();
        public class Row
        {
            public Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
        }

        public SpreadSheetManager(OAuth2Parameters parameters)
        {
            if (service == null)
            {
                GOAuth2RequestFactory requestFactory = new GOAuth2RequestFactory(null, "SpreadsheetIntegration", parameters);
                service = new SpreadsheetsService("SpreadsheetIntegration");
                service.RequestFactory = requestFactory;
            }
        }

        public SpreadSheetManager()
        {
            if (config == null)
            {
                config = (GoogleSheetsToUnityConfig)UnityEngine.Resources.Load("GSTU_Config");
            }

            ServicePointManager.ServerCertificateValidationCallback = Validator;
            OAuth2 oAuth2 = new OAuth2();
            var refreshToken = config.REFRESH_TOKEN;
            new SpreadSheetManager(oAuth2.GetOAuth2Parameter(refreshToken));
        }

        public static bool Validator(object in_sender, X509Certificate in_certificate, X509Chain in_chain, SslPolicyErrors in_sslPolicyErrors)
        {
            return true;
        }

        public List<GS2U_SpreadSheet> GetAllSheets()
        {
            // Instantiate a SpreadsheetQuery object to retrieve spreadsheets.
            SpreadsheetQuery query = new SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetFeed feed = service.Query(query) as SpreadsheetFeed;

            if (feed.Entries.Count == 0)
            {
                return null;
            }
            else
            {
                List<GS2U_SpreadSheet> returnList = new List<GS2U_SpreadSheet>();

                foreach (var sheet in feed.Entries)
                {
                    returnList.Add(new GS2U_SpreadSheet((SpreadsheetEntry)sheet));
                }

                return returnList;
            }
        }
    }



    public static class SpreadSheetExtensions
    {
        /// <summary>
        /// loads a spreadsheet using its name, if 2 or more spreadsheets share a name will load the first found instance
        /// </summary>
        /// <param name="managerV3"></param>
        /// <param name="spreadSheetName"></param>
        /// <returns></returns>
        public static GS2U_SpreadSheet LoadSpreadSheet(this SpreadSheetManager manager, string spreadSheetName)
        {
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Title = spreadSheetName;

            SpreadsheetFeed feed = SpreadSheetManager.service.Query(query) as SpreadsheetFeed;

            if(feed.Entries.Count == 0)
            {
                return null;
            }

            return new GS2U_SpreadSheet((SpreadsheetEntry)feed.Entries[0]);
        }

        /// <summary>
        /// Load a spreadsheet from its unique id, usefull if 2 or more spreadsheets have the same name
        /// </summary>
        /// <param name="managerV3"></param>
        /// <param name="spreadSheetID"></param>
        /// <param name="isSheetId"></param>
        /// <returns></returns>
        public static GS2U_SpreadSheet LoadSpreadSheet(this SpreadSheetManager manager, string spreadSheetID, bool isSheetId)
        {
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Uri = new Uri("https://spreadsheets.google.com/feeds/spreadsheets/private/full/" + spreadSheetID);

            SpreadsheetFeed feed = SpreadSheetManager.service.Query(query) as SpreadsheetFeed;

            if (feed.Entries.Count == 0)
            {
                return null;
            }

            return new GS2U_SpreadSheet((SpreadsheetEntry)feed.Entries[0]);
        }

        /// <summary>
        /// return a list of all worksheets on a spreadsheet
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static List<GS2U_Worksheet> GetAllWorkSheets(this GS2U_SpreadSheet sheet)
        {
            if (sheet.spreadsheetEntry.Worksheets.Entries.Count == 0)
            {
                return null;
            }
            else
            {
                List<GS2U_Worksheet> returnList = new List<GS2U_Worksheet>();

                foreach (var entry in sheet.spreadsheetEntry.Worksheets.Entries)
                {
                    returnList.Add(new GS2U_Worksheet((WorksheetEntry)entry));
                }

                return returnList;
            }
        }

        /// <summary>
        /// returns a list of all worksheet names known by a particular worksheet
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public static List<string> GetAllWorkSheetsNames(this GS2U_SpreadSheet sheet)
        {
            if (sheet.spreadsheetEntry.Worksheets.Entries.Count == 0)
            {
                return null;
            }
            else
            {
                List<string> returnList = new List<string>();

                foreach (var entry in sheet.spreadsheetEntry.Worksheets.Entries)
                {
                    returnList.Add(entry.Title.Text);
                }
                return returnList;
            }
        }


        /// <summary>
        /// Load a worksheet from the selected spreadsheet
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="workSheetName"></param>
        /// <returns></returns>
        public static GS2U_Worksheet LoadWorkSheet(this GS2U_SpreadSheet spreadsheet, string workSheetName)
        {
            WorksheetFeed wsFeed = spreadsheet.spreadsheetEntry.Worksheets;

            foreach (WorksheetEntry ws in wsFeed.Entries)
            {
                if (ws.Title.Text == workSheetName)
                {
                    return new GS2U_Worksheet(ws);
                }
            }
            return null;
        }

        /// <summary>
        /// load a worksheet from the selected spreadsheet
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="worksheetNumber"></param>
        /// <returns></returns>
        public static GS2U_Worksheet LoadWorkSheet(this GS2U_SpreadSheet spreadsheet, int worksheetNumber)
        {
            WorksheetFeed wsFeed = spreadsheet.spreadsheetEntry.Worksheets;

            return new GS2U_Worksheet((WorksheetEntry)wsFeed.Entries[worksheetNumber]);
        }

        /// <summary>
        /// Creates a new worksheet on the selected spreadsheet of size 60 x 250
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="worksheetTitle"></param>
        /// <returns></returns>
        public static GS2U_Worksheet AddNewWorksheet(this GS2U_SpreadSheet sheet, string worksheetTitle)
        {
            WorksheetEntry worksheet = new WorksheetEntry();
            worksheet.Title.Text = worksheetTitle;
            worksheet.Cols = 60;
            worksheet.Rows = 250;
            WorksheetFeed wsFeed = sheet.spreadsheetEntry.Worksheets;

            try
            {
                return new GS2U_Worksheet(SpreadSheetManager.service.Insert(wsFeed, worksheet));
            }
            catch
            {
                Debug.Log("Error adding new worksheet, name already exists");
                return null;
            }
        }

        /// <summary>
        /// Creates a new worksheet on the selected spreadsheet of size columns x rows
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="worksheetTitle"></param>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static GS2U_Worksheet AddNewWorksheet(this GS2U_SpreadSheet sheet, string worksheetTitle, int columns, int rows)
        {
            WorksheetEntry worksheet = new WorksheetEntry();
            worksheet.Title.Text = worksheetTitle;
            worksheet.Cols = (uint)columns;
            worksheet.Rows = (uint)rows;
            WorksheetFeed wsFeed = sheet.spreadsheetEntry.Worksheets;

            try
            {
                return new GS2U_Worksheet(SpreadSheetManager.service.Insert(wsFeed, worksheet));
            }
            catch
            {
                Debug.Log("Error adding new worksheet, name already exists");
                return null;
            }
        }

        public static void UpdateWorksheetName(this GS2U_Worksheet worksheet, string newName)
        {
            worksheet.worksheetEntry.Title.Text = newName;
            worksheet.worksheetEntry.Update();
        }

        /// <summary>
        /// Set the size of a worksheet, WARNING!!! setting the size smaller than the orginal will delete the cells and its contents that do not exist in the new bounds
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="newColums"></param>
        /// <param name="newRows"></param>
        public static void SetWorksheetSize(this GS2U_Worksheet worksheet, int newColums, int newRows)
        {
            worksheet.worksheetEntry.Cols = (uint)newColums;
            worksheet.worksheetEntry.Rows = (uint)newRows;
            worksheet.worksheetEntry.Update();
        }

        /// <summary>
        /// Get the current columns and row count for a worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public static Vector2 GetWorksheetSize(this GS2U_Worksheet worksheet)
        {
            Vector2 returnValue = new Vector2(worksheet.worksheetEntry.Cols, worksheet.worksheetEntry.Rows);
            return returnValue;
        }

        /// <summary>
        /// Adds additional columns and rows to a worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="columnsToAdd"></param>
        /// <param name="rowsToAdd"></param>
        public static void AddRowsAndColumns(this GS2U_Worksheet worksheet, int columnsToAdd, int rowsToAdd)
        {
            int rows = (int)worksheet.worksheetEntry.Rows;
            int colums = (int)worksheet.worksheetEntry.Cols;
            worksheet.SetWorksheetSize(colums + columnsToAdd, rows + rowsToAdd);
        }

        /// <summary>
        /// premently deletes the worksheet from Google sheets
        /// </summary>
        /// <param name="worksheet"></param>
        public static void DeleteWorksheet(this GS2U_Worksheet worksheet)
        {
            worksheet.worksheetEntry.Delete();
        }

        /// <summary>
        /// Loads all the information from the selected worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        /// 
        /// <summary>
        /// Loads all the information from the selected worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public static WorksheetData LoadAllWorksheetInformation(this GS2U_Worksheet worksheet)
        {
            ListFeed feed = LoadListFeedWorksheet(worksheet);
            WorksheetData returnData = new WorksheetData();

            foreach (ListEntry row in feed.Entries)
            {
                string rowTitle = row.Title.Text;
                RowData rowData = new RowData(rowTitle);

                foreach (ListEntry.Custom element in row.Elements)
                {
                    rowData.cells.Add(new CellData(element.Value, element.LocalName, rowTitle));
                }

                returnData.rows.Add(rowData);
            }

            return returnData;
        }

        /// <summary>
        /// Load worksheet as listfeed
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        private static ListFeed LoadListFeedWorksheet(this GS2U_Worksheet worksheet)
        {
            AtomLink listFeedLink = worksheet.worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());

            return SpreadSheetManager.service.Query(listQuery) as ListFeed;
        }

        /// <summary>
        /// Returns a list of all column titles
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public static List<string> GetColumnTitles(this GS2U_Worksheet worksheet)
        {
            CellQuery cellQuery = new CellQuery(worksheet.worksheetEntry.CellFeedLink);
            cellQuery.MinimumRow = 1;
            cellQuery.MaximumRow = 1;

            CellFeed cellFeed = SpreadSheetManager.service.Query(cellQuery) as CellFeed;

            List<string> list = new List<string>();

            foreach (CellEntry cell in cellFeed.Entries)
            {
                list.Add(cell.Value);
            }
            return list;
        }

        /// <summary>
        /// Returns a list of all row titles
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns></returns>
        public static List<string> GetRowTitles(this GS2U_Worksheet worksheet)
        {
            CellQuery cellQuery = new CellQuery(worksheet.worksheetEntry.CellFeedLink);
            cellQuery.MinimumColumn = 1;
            cellQuery.MaximumColumn = 1;

            CellFeed cellFeed = SpreadSheetManager.service.Query(cellQuery) as CellFeed;

            List<string> list = new List<string>();

            foreach (CellEntry cell in cellFeed.Entries)
            {
                list.Add(cell.Value);
            }
            return list;
        }


        /// <summary>
        /// Add a new row of data to the bottom of the worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="newData"></param>
        public static void AddRowData(this GS2U_Worksheet worksheet, Dictionary<string, string> newData)
        {
            ListFeed feed = worksheet.LoadListFeedWorksheet();

            ListEntry newRow = new ListEntry();

            foreach (var entry in newData)
            {
                newRow.Elements.Add(new ListEntry.Custom() { LocalName = entry.Key.ToLower(), Value = entry.Value.ToLower() });
            }

            SpreadSheetManager.service.Insert(feed, newRow);
        }


        /// <summary>
        /// updates a rows information based on the instance of dataname found in the worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="dataNameID"> the current name in the spreedsheet using the first colum as the identifier</param>
        /// <param name="newData"></param>
        public static void ModifyRowData(this GS2U_Worksheet worksheet, string dataNameID, Dictionary<string, string> newData)
        {
            List<string> ids = worksheet.GetRowTitles();

            int index = ids.IndexOf(dataNameID);

            if (index > -1)
            {
                worksheet.ModifyRowData(index, newData);
            }
            else
            {
                Debug.Log("no data found for entry " + dataNameID);
            }
        }

        /// <summary>
        /// updates a rows information based on the instance of dataname found in the worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="rowNumber">The number of the row to be modified</param>
        /// <param name="newData"></param>
        public static void ModifyRowData(this GS2U_Worksheet worksheet, int rowNumber, Dictionary<string, string> newData)
        {
            if (worksheet.GetWorksheetSize().y < rowNumber)
            {
                Debug.Log("Worksheet is smaller than rowNumber to edit");
                return;
            }

            ListFeed feed = worksheet.LoadListFeedWorksheet();
            ListEntry row = (ListEntry)feed.Entries[rowNumber - 1];

            foreach (var entry in newData)
            {
                bool found = false;
                foreach (ListEntry.Custom element in row.Elements)
                {
                    if (element.LocalName.ToLower() == entry.Key.ToLower())
                    {
                        element.Value = entry.Value;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Debug.LogError("No field found for " + entry.Key);
                }
            }

            row.Update();
        }

        /// <summary>
        /// Deletes the first row found using nameID
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="dataNameID"></param>
        public static void DeleteRowData(this GS2U_Worksheet worksheet, string dataNameID)
        {
            List<string> ids = worksheet.GetRowTitles();

            int index = ids.IndexOf(dataNameID);

            if (index > -1)
            {
                worksheet.DeleteRowData(index);
            }
            else
            {
                Debug.Log("no data found for entry " + dataNameID);
            }
        }

        /// <summary>
        /// Deletes the first row found using nameID
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="rowNumber">Row number to be removed</param>
        public static void DeleteRowData(this GS2U_Worksheet worksheet, int rowNumber)
        {
            if (worksheet.GetWorksheetSize().y < rowNumber)
            {
                Debug.Log("Worksheet is smaller than rowNumber to edit");
                return;
            }

            if (rowNumber > 0)
            {
                ListFeed feed = worksheet.LoadListFeedWorksheet();

                ListEntry row = (ListEntry)feed.Entries[rowNumber - 1];

                row.Delete();
            }
            else
            {
                Debug.Log("Invalid Row Number");
            }
        }

        /// <summary>
        /// Get the cell entry
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CellEntry GetCellEntry(this GS2U_Worksheet worksheet, string column, int row)
        {
            CellQuery cellQuery = new CellQuery(worksheet.worksheetEntry.CellFeedLink);
            int colInt = GoogleSheetsToUnityUtilities.GetIndexInAlphabet(column);
            cellQuery.MinimumRow = (uint)row;
            cellQuery.MaximumRow = (uint)row;
            cellQuery.MinimumColumn = (uint)colInt;
            cellQuery.MaximumColumn = (uint)colInt;

            CellFeed cellFeed = SpreadSheetManager.service.Query(cellQuery) as CellFeed;

            return (CellEntry)cellFeed.Entries[0];
        }

        /// <summary>
        /// get the cell entry
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static CellEntry GetCellEntry(this GS2U_Worksheet worksheet, int column, int row)
        {
            CellQuery cellQuery = new CellQuery(worksheet.worksheetEntry.CellFeedLink);
            cellQuery.MinimumRow = (uint)row;
            cellQuery.MaximumRow = (uint)row;
            cellQuery.MinimumColumn = (uint)column;
            cellQuery.MaximumColumn = (uint)column;

            CellFeed cellFeed = SpreadSheetManager.service.Query(cellQuery) as CellFeed;

            return (CellEntry)cellFeed.Entries[0];
        }

        /// <summary>
        /// Gets the data from an exact cell reference
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public static CellData GetCellData(this GS2U_Worksheet worksheet, string column, int row)
        {
            int colInt = GoogleSheetsToUnityUtilities.GetIndexInAlphabet(column);

            CellEntry entry = worksheet.GetCellEntry(colInt, row);

            List<string> rows = worksheet.GetRowTitles();
            List<string> cols = worksheet.GetColumnTitles();

            CellData cellData = new CellData(entry.InputValue, rows[row - 1], cols[colInt - 1]);
            return cellData;
        }

        public static void ModifyCellData(this GS2U_Worksheet worksheet, string colum, int row, string newData)
        {
            CellEntry cell = worksheet.GetCellEntry(colum, row);
            cell.InputValue = newData;
            cell.Update();
        }
    }

    public class GS2U_SpreadSheet
    {
        public SpreadsheetEntry spreadsheetEntry;

        public GS2U_SpreadSheet(SpreadsheetEntry newEntry)
        {
            spreadsheetEntry = newEntry;
        }
    }

    public class GS2U_Worksheet
    {
        public WorksheetEntry worksheetEntry;

        public GS2U_Worksheet(WorksheetEntry newEntry)
        {
            worksheetEntry = newEntry;
        }
    }

    public class WorksheetData
    {
        public List<RowData> rows = new List<RowData>();
    }

    public class RowData
    {
        public List<CellData> cells = new List<CellData>();
        public string rowTitle;

        public RowData(string rowTitle)
        {
            this.rowTitle = rowTitle;
        }
    }

    public class CellData
    {
        public string value;
        public string cellColumTitle;
        public string cellRowTitle;

        public CellData(string _name, string _cellColumTitle, string _cellRowTitle)
        {
            value = _name;
            cellColumTitle = _cellColumTitle;
            cellRowTitle = _cellRowTitle;
        }
    }
#endif
}

