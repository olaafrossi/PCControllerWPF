// Created by Three Byte Intemedia, Inc. | project: PCController |
// Created: 2021 03 31
// by Olaaf Rossi

using System;
using Serilog;

namespace PCController.Core.Managers
{
    public class ComboBoxSQLParseManager
    {
        public int GetLogs(string numOfItems)
        {
            int numOfMsgs = 20;
            try
            {
                if (numOfItems is null)
                {
                    numOfMsgs = 20;
                }
                else if (numOfItems.Contains("All"))
                {
                    // All
                    numOfMsgs = 100000000;
                }
                else if (numOfItems.Length == 40)
                {
                    // 20, 50
                    string logComboBoxSelected = numOfItems.Substring(38, 2);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (numOfItems.Length == 41)
                {
                    // hundred
                    string logComboBoxSelected = numOfItems.Substring(38, 3);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
                else if (numOfItems.Length == 42)
                {
                    // thousand
                    string logComboBoxSelected = numOfItems.Substring(38, 4);
                    numOfMsgs = int.Parse(logComboBoxSelected);
                }
            }
            catch (Exception e)
            {
                Log.Error("Didn't parse the number in the ComboBox {numOfMsgs}", numOfMsgs, e);
            }

            return numOfMsgs;
        }
    }
}