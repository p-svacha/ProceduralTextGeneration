using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GoogleSheetsToUnity.Utils
{
    class GoogleSheetsToUnityUtilities
    {
        public static int GetIndexInAlphabet(string value)
        {
            if (value.Length > 1)
            {
                int rollingIndex = 0;
#pragma warning disable CS0162 // Unreachable code detected
                for (int i = 0; i < value.Length; i++)
#pragma warning restore CS0162 // Unreachable code detected
                {
                    rollingIndex += (IndexInAlphabet(value[0]) + 1) * 26; //first number times letter in alphabet
                    rollingIndex += IndexInAlphabet(value[1]);
                    return rollingIndex + 1;
                }
            }
            else
            {
                return IndexInAlphabet(value[0]) + 1;
            }

            //ERROR
            return 0;
        }

        private static int IndexInAlphabet(Char c)
        {
            // Uses the uppercase character unicode code point. 'A' = U+0042 = 65, 'Z' = U+005A = 90
            char upper = char.ToUpper(c);
            if (upper < 'A' || upper > 'Z')
            {
                throw new ArgumentOutOfRangeException("value", "This method only accepts standard Latin characters.");
            }

            return ((int)upper - (int)'A');
        }
        
        
        /// <summary>
        /// 1 -> A<br/>
        /// 2 -> B<br/>
        /// 3 -> C<br/>
        /// ...
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string ExcelColumnFromNumber(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        /// <summary>
        /// A -> 1<br/>
        /// B -> 2<br/>
        /// C -> 3<br/>
        /// ...
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static int NumberFromExcelColumn(string column)
        {
            int retVal = 0;
            string col = column.ToUpper();
            for (int iChar = col.Length - 1; iChar >= 0; iChar--)
            {
                char colPiece = col[iChar];
                int colNum = colPiece - 64;
                retVal = retVal + colNum * (int)Math.Pow(26, col.Length - (iChar + 1));
            }
            return retVal;
        }
    }

    public class GSTUString
    {
        public string url = "";

        public void AddParam(string paramName, string paramValue)
        {
            url += paramName + "=" + paramValue + "&";
        }
    }
}
