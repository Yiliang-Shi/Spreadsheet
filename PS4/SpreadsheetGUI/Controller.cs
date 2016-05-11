using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SpreadsheetUtilities;
using Excel = Microsoft.Office.Interop.Excel;

//Authors: Yiliang Shi and Jet Vellinga
//Date: November 4, 2015
//PS6: Spreadsheet GUI
//References spreadsheet

namespace SpreadsheetGUI
{

    public class Controller
    {
        //Instance spreadsheet object - needs references to model and view
        private Spreadsheet spreadsheet;
        private View view;

        /// <summary>
        /// Initialize a controller that coordinates the gui and spreadsheet model
        /// </summary>
        /// <param name="filePath">determines if the new file has a save location</param>
        /// <param name="view">The view that the controller is cordinating</param>
        public Controller(string filePath, View view)
        {
            //Initialize the view and spreadsheet
            this.view = view;

            if (filePath == null)
            {
                //Creates a spreadsheet with version ps6 that accepts variables that start with A-Z and ends with 1-99
                spreadsheet = new Spreadsheet(x => Regex.IsMatch(x, @"^[A-Z][1-9][0-9]?$"), x => x.ToUpper(), "ps6");
                
            }
            else
            {
                //If a file name is passed in, open/replace the current spreadsheet with spreadsheet from the file
                spreadsheet = new Spreadsheet(filePath, x => Regex.IsMatch(x, @"^[A-Z][1-9][0-9]?$"), x => x.ToUpper(), "ps6");

                //After reading, update the view to show the values of all non-empty cells
                view.ClearPanel();
                foreach (string name in spreadsheet.GetNamesOfAllNonemptyCells())
                {
                    int col = (int)name[0] - 65;
                    int row;
                    int.TryParse(name.Substring(1), out row);
                    view.SetPanelCellValue(col, row - 1, GetCellValue(name));
                }
                view.SetPanelCellSelection(0, 0);
                updateSelectedView();
            }


        }

        /// <summary>
        /// updates the namebox, valuebox, contentbox in the gui when selection changes
        /// </summary>
        internal void updateSelectedView()
        {
            int col, row;
            view.GetPanelCellSelection(out col, out row);
            view.SetNameBox( GetCellName(col, row));
            view.SetValueBox(GetCellValue(GetCellName(col, row)));
            view.SetContentBox(GetCellContent(GetCellName(col, row)));

        }

        /// <summary>
        /// private method to calculate the name of a cell given its row and col (beginning index 0)
        /// </summary>
        /// <param name="col">col of spreadsheet starting with 0</param>
        /// <param name="row">row of spreadsheet starting with 0</param>
        /// <returns></returns>
        private string GetCellName(int col, int row)
        {
            string cellName = "";
            cellName += ((char)(65 + col));
            cellName += (1+ row);

            return cellName;
        }
        /// <summary>
        /// gets the value of a cell from the spreadsheet
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private string GetCellValue(string cellName)
        {
            //The value can be a string, double, or FormulaError
            //If the value is a FormulaError, display ERROR as the cell value
            object value = spreadsheet.GetCellValue(cellName);

            if (value is string)
            {
                return (string)value;
            }
            else if (value is double)
            {
                return value.ToString();
            }
            else
            {
                return "ERROR";
            }

        }
        /// <summary>
        /// get the content (formula etc) of the cell given the name
        /// </summary>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private string GetCellContent(string cellName)
        {   
            Object value = spreadsheet.GetCellContents(cellName);
            return (value is Formula) ? "=" + value.ToString() : value.ToString();
        }

        /// <summary>
        /// updates and recalculates all cell necessary when the content box updates.
        /// </summary>
        /// <param name="cellName">name of cell that change</param>
        /// <param name="cellContent">new content of cell</param>
        internal void SetContents(string cellName, string cellContent)
        {
            ISet<string> changed=null;
            //Need to deal with when formulas contains variables with text or with invalid names - update spreadsheet
            try {
               changed = spreadsheet.SetContentsOfCell(cellName, cellContent);
            }
            catch (Exception e)
            {
                MessageBox.Show("Formula exception: "+e.Message, "Spreadsheet",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                return;
            }

            view.Invoke(new Action(() =>
            {
                try
                {
                    //Otherwise, we also need to update all cells that are dependant and may have been changed
                    foreach (string name in changed)
                    {
                       int col = (int)name[0] - 65;
                       int row;
                       int.TryParse(name.Substring(1), out row);
                       view.SetPanelCellValue(col, row - 1, GetCellValue(name));
                    }
                    view.SetValueBox(GetCellValue(cellName));

                    //Keep track of unsaved changes
                    view.SetChanged(true);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Formula exception: " + e.Message, "Spreadsheet",
                                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk);
                    return;
                }
                
            }));


        }
        /// <summary>
        /// Calculates and change the cell selected in the gui when arrow keys are pressed. Ignore the key if it would result in a 
        /// invalid cell
        /// </summary>
        /// <param name="currentCell">The name of the current cell</param>
        /// <param name="e">key event pressed</param>
        internal void ChangeSelectionWithArrow(String currentCell,KeyEventArgs e)
        {
            int col, row;
            view.GetPanelCellSelection(out col, out row);
            Tuple<int, int> newLocation=null;
            //Checks if the result of the arrow key results in valid location
            switch (e.KeyCode)
            {
                case Keys.Left:
                    newLocation = GetNewCellLocation(col,row,-1, 0);
                    break;
                case Keys.Right:
                    newLocation = GetNewCellLocation(col, row, 1, 0);
                    break;
                case Keys.Up:
                    newLocation = GetNewCellLocation(col, row, 0, -1);
                    break;
                case Keys.Down:
                case Keys.Enter:
                    newLocation = GetNewCellLocation(col, row, 0, 1);
                    break;
            }
            //sets new location if there's no error
            if(newLocation!=null &&newLocation.Item1!=-1)
            {
                col = newLocation.Item1;
                row = newLocation.Item2;
            }
            //change selection
            view.SetPanelCellSelection(col, row);
            //update the selected cell gui
            updateSelectedView();
        }
        /// <summary>
        /// Calculates the new location of selected cell. If new cells is invalid, sets tuple to -1,-1
        /// </summary>
        /// <param name="x">current col</param>
        /// <param name="y">current row</param>
        /// <param name="xChange">change in col</param>
        /// <param name="yChange">change in row</param>
        /// <returns></returns>
        private Tuple<int,int> GetNewCellLocation(int x, int y,int xChange, int yChange)
        {
            x += xChange;
            y+= yChange;
            if (x < 0 || x > 25 || y < 0 || y > 98)
                return new Tuple<int, int>(-1, -1);
            else
                return new Tuple<int, int>(x, y);
        }


        /// <summary>
        /// Save the current spreadsheet to a specified file path
        /// </summary>
        /// <param name="filePath">File name for the saved file</param>
        internal void SaveSpreadsheet(string filePath)
        {
            //Check for valid file name
            if (filePath != null)
            {
                try
                {
                    spreadsheet.Save(filePath);
                }
                catch(SpreadsheetReadWriteException e)
                {
                    MessageBox.Show("Save Error: " + e.Message, String.Empty, MessageBoxButtons.OK);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Save Error: No file name specified", String.Empty, MessageBoxButtons.OK);
                return;
            }

            //Now that it is saved, there are no unsaved changes 
            view.SetChanged(false);
            
        }
        /// <summary>
        /// Generates excel file according to spreadsheet model
        /// </summary>
        internal void ExportToExcel(String filename)
        {
            Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();

            if (xlApp == null)
            {
                MessageBox.Show("Excel is not properly installed!!");
                return;
            }

            //Creates a new excel workbook with spreadsheet1 open
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;
            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            //Store all non-empty cell
            foreach (string name in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                int col = (int)name[0] - 65;
                int row;
                int.TryParse(name.Substring(1), out row);

                xlWorkSheet.Cells[row,col+1] = spreadsheet.GetCellContents(name);
            }

            //Saves the file
            xlWorkBook.SaveAs(filename);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            //Release resources
            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);
        }
        
        /// <summary>
        /// Cleans and release resources used to access excel
        /// </summary>
        /// <param name="obj"></param>
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

    }
}
