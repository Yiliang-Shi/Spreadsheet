using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SS;

//Authors: Yiliang Shi and Jet Vellinga
//Date: November 4, 2015
//PS6: Spreadsheet GUI
//References spreadsheet

namespace SpreadsheetGUI
{
    public partial class View : Form
    {
        //Instance variable
        //Reference to controller
        Controller controller;
       // BackgroundWorker worker;

        /// <summary>
        /// Initialze the view and instantiate the controller
        /// </summary>
        public View()
        {
            
            InitializeComponent();
            controller = new Controller(null, this);

            //Update built-in event handling
            this.FormClosing += View_FormClosing;
            spreadsheetPanel1.SelectionChanged += updateSelectedView;
            spreadsheetPanel1.PreviewKeyDown += spreadsheetpanel1_PreviewKeyDown;
            cellContentTextBox.PreviewKeyDown += cellContentTextBox_PreviewKeyDown;
        }

        /// <summary>
        /// If there are unsaved changes when closing the form, ask user if they want to save
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_FormClosing(object sender, FormClosingEventArgs e)
        {
           if (changedStatus.Text.Equals("Unsaved Changes"))
            {
                DialogResult warning = MessageBox.Show("Closing now will result in unsaved changes. Do you wish to save before closing?",
                                                        String.Empty, MessageBoxButtons.YesNoCancel);

                //If user selects cancel, cancel the close
                if (warning == DialogResult.Cancel)
                    e.Cancel = true;
                //If yes is selected, open save dialog
                else if (warning == DialogResult.Yes)
                {
                    if (Text == "Spreadsheet1")
                    {
                        newSave();
                    }
                    else
                    {
                        controller.SaveSpreadsheet(Text);
                    }
                }
            }
        }

        /// <summary>
        /// Redraw the spreadsheet panel
        /// </summary>
        /// <param name="spreadsheetPanel1"></param>
        private void updateSelectedView(SpreadsheetPanel spreadsheetPanel1)
        {
            controller.updateSelectedView();
        }

        /// <summary>
        /// Sets the view controller, can be called from the controller
        /// </summary>
        /// <param name="controller"></param>
        internal void SetController(Controller controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// When the enter button is clicked, the spreadsheet will be updated with whatever content has been entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentEnterButton_Click(object sender, EventArgs e)
        {
            //controller.SetContents(cellNameTextBox.Text, cellContentTextBox.Text);

            backgroundWorker1.RunWorkerAsync();
            spreadsheetPanel1.Focus();
        }

        /// <summary>
        /// Keeps track of the changed status and displays it to the user
        /// </summary>
        /// <param name="isChanged"></param>
        internal void SetChanged(bool isChanged)
        {
            if (isChanged)
                changedStatus.Text = "Unsaved Changes";
            else
                changedStatus.Text = "No New Changes";
        }


        /// <summary>
        /// Calls the controller to change selected cell
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetPanel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.Left && e.KeyCode <= Keys.Down ||e.KeyCode==Keys.Enter)
                controller.ChangeSelectionWithArrow(cellNameTextBox.Text, e);       
        }


        /// <summary>
        /// Previews key pressed as to determine if it is considered an input key by spreadsheetpanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetpanel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if ((e.KeyCode >= Keys.Left && e.KeyCode <= Keys.Down) //If key is an arrow key
                || (e.KeyCode==Keys.Enter)) //If key is enter
                e.IsInputKey = true;//Consider them input keys
        }

        /// <summary>
        /// When the enter key is pressed, update the spreadsheet based on the text in the content box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cellContentTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //controller.SetContents(cellNameTextBox.Text, cellContentTextBox.Text);
                backgroundWorker1.RunWorkerAsync();
                spreadsheetPanel1.Focus();
            }
        }

        /// <summary>
        /// Determines which key to pass through the operating system to the inputkey event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cellContentTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) //If key is enter
                e.IsInputKey = true;//Consider them input keys
        }

        /// <summary>
        /// When user types, enter the text in the cell content box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetPanel1_KeyPress(object sender, KeyPressEventArgs e)
        {
            cellContentTextBox.Focus();
            SendKeys.Send(e.KeyChar.ToString());
        }

        //---------------------------Tool Strip Menu Items---------------------------

        /// <summary>
        /// Closes the spreadsheet when the exit menu item is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Creates a new spreadsheet form when the new menu item is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpreadsheetApplicationContext.getAppContext().RunForm(new View());  
        }

        /// <summary>
        /// This method will save the current spreadsheet when the save button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Text == "Spreadsheet1")
            {
                //If the file has not previously been saved, call the newSave helper method so the user can specify a filename
                newSave();
            }
            else
            {
                //Decide how to save the file
                if (Text.EndsWith(".xls"))
                {
                    controller.ExportToExcel(Text);
                }
                else
                {
                    controller.SaveSpreadsheet(Text);
                }       
            }

        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Call the newSave helper method
            newSave();
        }

        /// <summary>
        /// Helper method for the Save and Save As click events. Opens the save file dialog and passes the specified file path to the correct save method
        /// </summary>
        private void newSave()
        {
            //Use a SaveFileDialog to allow the user to browse to their desired save location
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "Spreadsheet File (*.sprd)|*.sprd";
            saveFile.Filter = "Spreadsheet File (*.sprd)|*.sprd|Excel File(*.xls)|*.xls|All files (*.*)|*.*";
            saveFile.Title = "Save a Spreadsheet file";
            saveFile.FileName = Text;
            saveFile.ShowDialog();

            string filePath = saveFile.FileName;

            //If save is cancelled, simply return to spreadsheet
            if (filePath == "")
            {
                return;
            }

            //Determine how to save
            if (filePath.EndsWith(".xls"))
            {
                controller.ExportToExcel(filePath);
            }
            else
            {
                controller.SaveSpreadsheet(filePath);
            }
            
            //Update the spreadsheet name using the file name, not file path
            String[] temp = filePath.Split(new char[1] { '\\' });
            Text = temp[temp.Length - 1];
            
        }

        /// <summary>
        /// Replace the current spreadsheet with a previously saved file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Use the open file dialog to specify the open file path
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "Spreadsheet File (*.sprd)|*.sprd|All files (*.*)|*.*";
            openFile.Title = "Open a Spreadsheet file";
            openFile.ShowDialog();

            string filePath = openFile.FileName;

            //If open is cancelled, return to spreadsheet
            if (filePath == "")
            {
                return;
            }

            //Initialize to be a new spreadsheet from the saved file
            controller = new Controller(filePath, this);

            //Update the spreadsheet name
            String[] temp = filePath.Split(new char[1] { '\\' });
            Text = temp[temp.Length - 1];
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            controller.SetContents(cellNameTextBox.Text, cellContentTextBox.Text);
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show("Error: " + e.Error.Message);
        }






        //--The follow methods will display message boxes containing functionality help when the Help menu items are clicked---------------- 
        private void newToolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("The NEW option will allow you to open a new, empty spreadsheet. It will not close the current spreadsheet or save any changes. NEW can be called by pressing Ctrl+N",
                            "New", MessageBoxButtons.OK);
        }

        private void openToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The OPEN option will allow you to open a saved spreadsheet file in the current window. OPEN can be called by pressing Ctrl+O",
                            "Open", MessageBoxButtons.OK);
        }

        private void saveToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The SAVE option will allow you to save the current spreadsheet. If the current spreadsheet has already been saved," 
                          + " the old file will be overwritten. If it has not be saved, you will be asked to specify a file name. " 
                          + "SAVE can be called by pressing Ctrl+S",
                           "Save", MessageBoxButtons.OK);
        }

        private void saveAsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The SAVE AS option will allow you to save the current spreadsheet file. You will be asked to specify a file name. " 
                          + "SAVE AS can be called by pressing Ctrl+A",
                            "Save As", MessageBoxButtons.OK);
        }

        private void exitToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The EXIT option will close the current spreadsheet. If there are unsaved changes, " 
                          + "you will be asked to confirm if you want to close without saving. EXIT can be called by pressing Ctrl+X "
                          + "or by clicking the X at the top right corner of the screen.",
                            "Exit", MessageBoxButtons.OK);
        }

        private void displayToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("There are a few key components of the display.\n\nIn the top left corner the spreadsheet name is displayed. "
                          + "This will change when the spreadsheet is saved.\n\nIn the top right corner you will find the minimize and"
                          + " maximize buttons as well as the exit button.\n\nUnder the spreadsheet name you will find the file menu and help menu."
                          + "\n\nUnder that, there are three cells that display the name, value, and content of the selected cell."
                          + "\n\nThe grid has numbers along the left edge and letters along the top edge. "
                          + "These will allow you to identify the position and name of the cells. The cells range from A-Z and 1-99."
                          + "\n\nIn the bottom left corner you can see if there are any unsaved changes.",
                            "Display", MessageBoxButtons.OK);
        }

        private void cellSelctionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("There are two ways to select a cell. You can click on the desired cell with the mouse or "
                          + "you can use the arrow keys to move between cells.",
                            "Cell Selection", MessageBoxButtons.OK);
        }

        private void enterContentToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("There are three types of values that can be entered into the cells: words, numbers, and formulas. "
                          + "\n\nIf you wish to enter a formula, type \"=\" before entering your formula. Formulas can reference other cells as variables. "
                          + "Cell names are not case-sensitive.\n\nTo save the content in the cell, either click the enter button or press the enter key.",
                            "Enter Content", MessageBoxButtons.OK);
        }

        private void errorMessagesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Circular Dependency:\nThe formula you entered cannot be evaluated because it refers to itself. "
                          + "Check the contents of the variables you reference.\n\nVariable not valid:\nFormulas can only contain variables between A1 and Z99."
                          + " Check that all variables are valid cell names.\n\nInvalid Formula Syntax:\nFormulas must have a least one charater. "
                          + "They must begin with a number, variable, or opening paratheses and end with a number, variable, or closing paratheses. "
                          + "Operators must be proceeded and followed by numbers, variables, or expressions in parantheses. Numbers, variables, operators, "
                          + "and paratheses are the only valid formula characters. Please ensure that your formula is valid.\n\n"
                          + "ERROR:\nThe ERROR message appears when you try to divide by zero. Check your formula and try again.",
                            "Error Messages", MessageBoxButtons.OK);
        }

        private void exportToExcelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("In order to save your spreadsheet as an Excel readable file, go to the file menu and select the \"Save As\" option. "
                          + "Specify a file name and then use the dropdown menu to select \"Excel File\" as the file type.",
                            "Export to Excel", MessageBoxButtons.OK);
        }






        //-------------------------Convenience methods for the controller--------------------------

        internal void SetPanelCellValue(int col, int row, string value)
        {
            spreadsheetPanel1.SetValue(col, row, value);
        }

        internal void GetPanelCellValue(int col, int row, out string value)
        {
            spreadsheetPanel1.GetValue(col, row, out value);
        }

        internal void SetPanelCellSelection(int col, int row)
        {
            spreadsheetPanel1.SetSelection(col, row);
        }

        internal void GetPanelCellSelection(out int col, out int row)
        {
            spreadsheetPanel1.GetSelection(out col, out row);
        }

        internal void ClearPanel()
        {
            spreadsheetPanel1.Clear();
        }

        internal void SetNameBox(string name)
        {
            cellNameTextBox.Text = name;
        }

        internal void SetContentBox(string content)
        {
            cellContentTextBox.Text = content;
        }

        internal void SetValueBox(string value)
        {
            cellValueTextBox.Text = value;
        }

        internal string getValueBox()
        {
            return cellValueTextBox.Text;
        }

        internal string getNameBoxText()
        {
            return cellNameTextBox.Text;
        }

        internal string getContentBoxText()
        {
            return cellContentTextBox.Text;
        }

    }
}
