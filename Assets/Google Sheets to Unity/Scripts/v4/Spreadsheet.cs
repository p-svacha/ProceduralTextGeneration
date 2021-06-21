using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GoogleSheetsToUnity.Utils;
using GreenerGames;
using UnityEngine;
using UnityEngine.Events;

namespace GoogleSheetsToUnity
{
    /// <summary>
    /// Search class for accessing a database
    /// </summary>
    public class GSTU_Search
    {
        public readonly string sheetId = "";
        public readonly string worksheetName = "Sheet1";

        public readonly string startCell = "A1";
        public readonly string endCell = "Z100";

        public readonly string titleColumn = "A";
        public readonly int titleRow = 1;

        public GSTU_Search(string sheetId, string worksheetName)
        {
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
        }

        public GSTU_Search(string sheetId, string worksheetName, string startCell)
        {
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
            this.startCell = startCell;
        }

        public GSTU_Search(string sheetId, string worksheetName, string startCell, string endCell)
        {
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
            this.startCell = startCell;
            this.endCell = endCell;
        }

        public GSTU_Search(string sheetId, string worksheetName, string startCell, string endCell, string titleColumn, int titleRow)
        {
            this.sheetId = sheetId;
            this.worksheetName = worksheetName;
            this.startCell = startCell;
            this.endCell = endCell;
            this.titleColumn = titleColumn;
            this.titleRow = titleRow;
        }
    }

    [Serializable]
    public class GSTU_Cell
    {
        string column = string.Empty;
        public string columnId = string.Empty;

        int row = -1;
        public string rowId = string.Empty;

        public string value = string.Empty;

        internal List<string> titleConnectedCells = new List<string>();

        public GSTU_Cell(string value, string column, int row)
        {
            this.value = value;
            this.column = column;
            this.row = row;
        }

        public GSTU_Cell(string value)
        {
            this.value = value;
        }

        public string Column()
        {
            return column;
        }
        public int Row()
        {
            return row;
        }
        public string CellRef()
        {
            return column + row;
        }

        //TODO: store the sheetId and worksheet in the spreadsheet so dont have to pass these through
        internal void UpdateCellValue(string sheetID, string worksheet, string value, UnityAction callback = null)
        {
            this.value = value;
            List<string> list = new List<string>();
            list.Add(value);
            SpreadsheetManager.Write(new GSTU_Search(sheetID, worksheet, CellRef()), new ValueRange(list), callback);
        }
        //TODO: store the sheetId and worksheet in the spreadsheet so dont have to pass these through
        internal ValueRange AddCellToBatchUpdate(string sheetID, string worksheet, string value)
        {
            this.value = value;
            List<string> list = new List<string>();
            list.Add(value);
            ValueRange data = new ValueRange(list);
            data.range = CellRef();
            return data;
        }
    }

    [Serializable]
    public class GSTU_SpreadsheetResponce
    {
        public ValueRange valueRange;
        internal Sheet sheetInfo = null;

        public string WorkSheet()
        {
            return valueRange.range.Substring(0, valueRange.range.IndexOf("!") - 1);
        }

        public string StartCell()
        {
            int start = valueRange.range.IndexOf("!") + 1;
            int end = valueRange.range.IndexOf(":", start);
            return valueRange.range.Substring(start, end - start);
        }

        public string EndCell()
        {
            return valueRange.range.Substring(valueRange.range.IndexOf(":") + 1);
        }

        public GSTU_SpreadsheetResponce() { }

        public GSTU_SpreadsheetResponce(ValueRange data)
        {
            valueRange = data;
        }
    }

    [Serializable]
    public class ValueRange
    {
        public string range = "";
        public string majorDimension;
        public List<List<string>> values = new List<List<string>>();

        public ValueRange() { }

        /// <summary>
        /// Used to create a spreadshhet that can be returned to google sheets
        /// </summary>
        /// <param name="data"></param>
        public ValueRange(List<List<string>> data)
        {
            values = data;
        }

        /// <summary>
        /// Used to create a spreadshhet that can be returned to google sheets
        /// </summary>
        /// <param name="data"></param>
        public ValueRange(List<string> data)
        {
            values.Add (data);
        }

        public ValueRange(string data)
        {
            values.Add(new List<string>() { data });
        }

        public void Add(List<string> data)
        {
            values.Add(data);
        }
    }

    [Serializable]
    public class BatchRequestBody
    {
        public ValueInputOption valueInputOption = ValueInputOption.USER_ENTERED;
        public List<ValueRange> data = new List<ValueRange>();

        public void Add(ValueRange data)
        {
            this.data.Add(data);
        }

        public void Send(string spreadsheetId, string worksheet, UnityAction callback)
        {
            SpreadsheetManager.WriteBatch(new GSTU_Search(spreadsheetId, worksheet), this, callback);
        }
    }

    [Serializable]
    public class SheetsRootObject
    {
        public List<Sheet> sheets;
    }
    [Serializable]
    public class Sheet
    {
        public Properties properties;
        public List<Merge> merges;
    }
    [Serializable]

    public class Properties
    {
        public int sheetId;
        public string title;
        public int index;
        public string sheetType;
    }
    [Serializable]
    public class Merge
    {
        public int sheetId;
        public int startRowIndex;
        public int endRowIndex;
        public int startColumnIndex;
        public int endColumnIndex;
    }

    public enum Dimension
    {
        Rows,
        Columns
    }

    public enum ValueRenderOption
    {
        UnformattedValue,
        FormattedValue,
        Formula
    }

    public enum ValueInputOption
    {
        RAW,
        USER_ENTERED
    }

    [Serializable]
    public class GstuSpreadSheet
    {
        /// <summary>
        ///     All the cells that the spreadsheet loaded
        ///     Index is Cell ID IE "A2"
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, GSTU_Cell> Cells = new Dictionary<string, GSTU_Cell>();

        public SecondaryKeyDictionary<string, List<GSTU_Cell>> columns =
            new SecondaryKeyDictionary<string, List<GSTU_Cell>>();

        public SecondaryKeyDictionary<int, string, List<GSTU_Cell>> rows =
            new SecondaryKeyDictionary<int, string, List<GSTU_Cell>>();

   /*     public GstuSpreadSheet(GSTU_SpreadsheetResponce data)
        {
            string startColumn = Regex.Replace(data.StartCell(), "[^a-zA-Z]", "");
            int startRow = int.Parse(Regex.Replace(data.StartCell(), "[^0-9]", ""));

            int startColumnAsInt = GoogleSheetsToUnityUtilities.NumberFromExcelColumn(startColumn);
            int currentRow = startRow;

            foreach (List<string> dataValue in data.valueRange.values)
            {
                int currentColumn = startColumnAsInt;

                foreach (string entry in dataValue)
                {
                    string realColumn = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(currentColumn);
                    GSTU_Cell cell = new GSTU_Cell(entry, realColumn, currentRow);

                    Cells.Add(realColumn + currentRow, cell);

                    if (!rows.ContainsKey(currentRow))
                    {
                        rows.Add(currentRow, new List<GSTU_Cell>());
                    }

                    rows[currentRow].Add(cell);

                    if (!columns.ContainsPrimaryKey(realColumn))
                    {
                        columns.Add(realColumn, new List<GSTU_Cell>());
                    }

                    columns[realColumn].Add(cell);

                    currentColumn++;
                }

                currentRow++;
            }

            if(data.sheetInfo != null)
            {
                foreach(var merge in data.sheetInfo.merges)
                {
                    Debug.Log("Merge starts at : " + merge.startRowIndex + " " + GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(merge.startColumnIndex));
                }
            }
        }*/

        public GstuSpreadSheet(GSTU_SpreadsheetResponce data, string titleColumn, int titleRow)
        {
            string startColumn = Regex.Replace(data.StartCell(), "[^a-zA-Z]", "");
            int startRow = int.Parse(Regex.Replace(data.StartCell(), "[^0-9]", ""));

            int startColumnAsInt = GoogleSheetsToUnityUtilities.NumberFromExcelColumn(startColumn);
            int currentRow = startRow;

            Dictionary<string, string> mergeCellRedirect = new Dictionary<string, string>();
            if (data.sheetInfo != null)
            {
                foreach (var merge in data.sheetInfo.merges)
                {
                    string cell = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(merge.startColumnIndex + 1) + (merge.startRowIndex + 1);

                    for (int r = merge.startRowIndex; r < merge.endRowIndex; r++)
                    {
                        for (int c = merge.startColumnIndex; c < merge.endColumnIndex; c++)
                        {
                            string mergeCell = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(c + 1) + (r + 1);
                            mergeCellRedirect.Add(mergeCell, cell);
                        }
                    }
                }
            }


            foreach (List<string> dataValue in data.valueRange.values)
            {
                int currentColumn = startColumnAsInt;

                foreach (string entry in dataValue)
                {
                    string realColumn = GoogleSheetsToUnityUtilities.ExcelColumnFromNumber(currentColumn);
                    string cellID = realColumn + currentRow;

                    GSTU_Cell cell = null;
                    if (mergeCellRedirect.ContainsKey(cellID) && Cells.ContainsKey(mergeCellRedirect[cellID]))
                    {
                        cell = Cells[mergeCellRedirect[cellID]];
                    }
                    else
                    {
                        cell = new GSTU_Cell(entry, realColumn, currentRow);

                        //check the title row and column exist, if not create them
                        if (!rows.ContainsKey(currentRow))
                        {
                            rows.Add(currentRow, new List<GSTU_Cell>());
                        }
                        if (!columns.ContainsPrimaryKey(realColumn))
                        {
                            columns.Add(realColumn, new List<GSTU_Cell>());
                        }

                        rows[currentRow].Add(cell);
                        columns[realColumn].Add(cell);


                        //build a series of seconard keys for the rows and columns
                        if (realColumn == titleColumn)
                        {
                            rows.LinkSecondaryKey(currentRow, cell.value);
                        }
                        if (currentRow == titleRow)
                        {
                            columns.LinkSecondaryKey(realColumn, cell.value);
                        }
                    }

                    Cells.Add(cellID, cell);

                    currentColumn++;
                }

                currentRow++;
            }

            //build the column and row string Id's from titles
            foreach(GSTU_Cell cell in Cells.Values)
            {
                cell.columnId = Cells[cell.Column() + titleRow].value;
                cell.rowId = Cells[titleColumn + cell.Row()].value;
            }

            //build all links to row and columns for cells that are handled by merged title fields.
            foreach(GSTU_Cell cell in Cells.Values)
            {
                foreach(KeyValuePair<string,GSTU_Cell> cell2 in Cells)
                {
                    if (cell.columnId == cell2.Value.columnId && cell.rowId == cell2.Value.rowId)
                    {
                        if (!cell.titleConnectedCells.Contains(cell2.Key))
                        {
                            cell.titleConnectedCells.Add(cell2.Key);
                        }
                    }
                }
            }
        }

        public GSTU_Cell this[string cellRef]
        {
            get
            {
                return Cells[cellRef];
            }
        }

        public GSTU_Cell this[string rowId, string columnId]
        {
            get
            {
                    string columnIndex = columns.secondaryKeyLink[columnId];
                    int rowIndex = rows.secondaryKeyLink[rowId];

                    return Cells[columnIndex + rowIndex];
            }
        }

        public List<GSTU_Cell> this [string rowID, string columnID, bool mergedCells]
        {
            get
            {
                string columnIndex = columns.secondaryKeyLink[columnID];
                int rowIndex = rows.secondaryKeyLink[rowID];
                List < string > actualCells = Cells[columnIndex + rowIndex].titleConnectedCells;

                List<GSTU_Cell> returnCells = new List<GSTU_Cell>();
                foreach(string s in actualCells)
                {
                    returnCells.Add(Cells[s]);
                }

                return returnCells;
            }
        }
    }
}