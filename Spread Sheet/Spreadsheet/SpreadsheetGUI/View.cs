// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using System;
using System.Windows.Forms;
using SSGui;
using System.Diagnostics.CodeAnalysis;

namespace SpreadsheetGUI {
    [ExcludeFromCodeCoverage]
    public partial class View : Form, IView
    {
        /// <summary>
        /// The GUI for a Spreadsheet
        /// </summary>
        public View()
        {
            InitializeComponent();
            spreadsheetPanel1.SelectionChanged += DisplaySelection;
            TextBox_CellName.Text = "A1";
            spreadsheetPanel1.SetSelection(0, 0);
        }

        // Public Events: 
        /// <summary>
        /// Event fired when 'Close' is clicked
        /// </summary>
        public event Action CloseEvent;

        /// <summary>
        /// Event fired when X button is clicked
        /// </summary>
        public event Action<FormClosingEventArgs> ClosingEvent;

        /// <summary>
        /// Event fired when 'Open' is clicked
        /// </summary>
        public event Action<string> ChooseFileEvent;

        /// <summary>
        /// Event fired when a cell is selected
        /// </summary>
        public event Action<string> GetCellContentsEvent;

        /// <summary>
        /// Event fired when 'New Sheet' is clicked
        /// </summary>
        public event Action NewEvent;

        /// <summary>
        /// Event fired when 'In New Window' is clicked
        /// </summary>
        public event Action OpenInNewEvent;

        /// <summary>
        /// Event fired when 'Open' is clicked
        /// </summary>
        public event Action<string> OpenFileEvent;

        /// <summary>
        /// Event fired when 'Save' is clicked
        /// </summary>
        public event Action SaveEvent;

        /// <summary>
        /// Event fired when 'Save As' is clicked
        /// </summary>
        public event Action SaveAsEvent;

        /// <summary>
        /// Event fired when entering data into the spreadsheet
        /// </summary>
        public event Action<int, int, string> UpdateCellEvent;

        // Public IView Methods: 
        /// <summary>
        /// Closes this window
        /// </summary>
        public void DoClose()
        {
            Close();
        }

        /// <summary>
        /// Clears the SpreadsheetPanel
        /// </summary>
        public void ClearSpreadsheetPanel() {
            spreadsheetPanel1.Clear();
        }

        /// <summary>
        /// Opens new window
        /// </summary>
        public void NewSpreadsheet()
        {
            ViewApplicationContext.GetContext().RunNew();
        }

        /// <summary>
        /// Opens a spreadsheet file
        /// </summary>
        public void OpenFile(string name) {
            if(OpenFileEvent != null) {
                OpenFileEvent(name);
            }
        }

        /// <summary>
        /// Opens file in a new window
        /// </summary>
        public void OpenInNew()
        {
            if(OpenInNewEvent != null) {
                OpenInNewEvent();
            }
        }

        /// <summary>
        /// Sets the cell value, or the value that is shown on the spreadsheet
        /// </summary>
        /// <param name="col">The column of the cell</param>
        /// <param name="row">The row of the cell</param>
        /// <param name="value">The value of the cell</param>
        public void SetCellValue(int col, int row, string value)
        {
            spreadsheetPanel1.SetValue(col, row, value);
        }

        /// <summary>
        /// Sets the contents of the cell, or what is shown in the Contents text box
        /// </summary>
        /// <param name="contents">The contents of the cell</param>
        public void SetCellContents(string contents)
        {
            TextBox_Contents.Text = contents;
        }

        /// <summary>
        /// Sets the current selection
        /// </summary>
        /// <param name="col">The column of the cell</param>
        /// <param name="row">The row of the cell</param>
        public void SetCellSelection(int col, int row)
        {
            spreadsheetPanel1.SetSelection(col, row);
            GetCellContentsEvent(((Convert.ToChar(col + 65)) + "" + (row + 1)));
            DisplaySelection(spreadsheetPanel1);
        }

        // Helper Methods: 
        /// <summary>
        /// Updates the displayed values based on the selection
        /// </summary>
        /// <param name="ss"></param>
        private void DisplaySelection(SpreadsheetPanel ss)
        {
            int col, row;
            ss.GetSelection(out col, out row);
            string tmp = ((Convert.ToChar(col + 65)) + "" + (row + 1));
            TextBox_CellName.Text = tmp;
            GetCellContentsEvent(tmp);
            TextBox_Contents.SelectAll();
        }

        /// <summary>
        /// Lets the context throw the ChooseFileEvent
        /// </summary>
        public void ChooseFile(string from)
        {
            if(ChooseFileEvent != null) {
                ChooseFileEvent(from);
            }
        }

        // Private View Event Methods: 
        /// <summary>
        /// Fires the CloseEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CloseEvent != null)
            {
                CloseEvent();
            }
        }

        /// <summary>
        /// Fires the ChooseFileEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // calling chooseFile() method
            fileToolStripMenuItem.HideDropDown();
            if(ChooseFileEvent != null && OpenFileEvent != null) {
                ChooseFileEvent(null);
            }
        }

        /// <summary>
        /// Fires the NewEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newSheetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // calling newEvent() method
            if (NewEvent != null)
            {
                NewEvent();
            }
        }

        /// <summary>
        /// Fires the SaveAsEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileToolStripMenuItem.HideDropDown();
            if (SaveEvent != null) {
                SaveEvent();
            }

        }

        /// <summary>
        /// Fires the OpenInNewEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewWindow_Click(object sender, EventArgs e)
        {
            if (OpenInNewEvent != null)
            {
                OpenInNewEvent();
            }
        }

        /// <summary>
        /// Fires the SaveAsEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // calling SaveAsEvent() method
            fileToolStripMenuItem.HideDropDown();
            if(SaveAsEvent != null) {
                SaveAsEvent();
            }
        }

        /// <summary>
        /// Sets focus to TextBox_Contents on key press
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetPanel1_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox_Contents.Focus();
            if(e.KeyChar > 13) {
                TextBox_Contents.AppendText(e.KeyChar + "");
            }
        }

        /// <summary>
        /// Displays Help Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contactsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // displaying the help box
            String text = "A Spreadsheet is an electronic document in which data is arranged in the rows and columns " +
                        "of a grid and can be manipulated and used in calculations. " +
                        "\n"+
                        "\n" +
                        "This Spreadsheet Program allow a user to Enter words ('Strings'), numbers ('Double') " +
                        "and formulas into the spreadsheet using contents box. To change the cell you have to " +
                        "click on the targeted cell with a mouse and enter the 'strings','numbers', or 'formula'" +
                        "of your choice and finally click enter or selecting another cell to make it appear " +
                        "in your box. You can also navigate by pressing Tab to go one cell to the right, and Shift+Tab " +
                        " to go one cell to the left. Additionally, you can press Enter to go one cell down, and Shift+Enter to go one cell up. " +
                        "If you want to make a calculation of a number put equal(=) at the beginning. " +
                        " Examples: '= 5 + 4' then press enter. "+
                         "\n" +
                        "\n" +
                        " If you close the Spreadsheet without saving, it will pop up a text box asking you to save " +
                        "or if you are sure you do not want to save. ";
            MessageBox.Show(text, "Help!");
        }

        /// <summary>
        /// Displays Help Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tehnicalSupportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // displaying the Technical Support box
            String text = "If you need any Assistance contact:  " +
                          "\n"+
                          "Ismael Kadilo: u1120347@utah.edu   \n" +
                          "Nathan Milot: nathan.c.milot@gmail.com ";
            MessageBox.Show(text, "Technical Support");
        }

        /// <summary>
        /// Displays Help Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // displaying the about box
            string text = "This is a Spreadsheet Project 'PS7' Designed by Ismael Kadilo and Nathan Milot "+
                          "\n"+
                          "\n"+
                          " A Spreadsheet is an electronic document in which data is arranged in the rows and columns " +
                          "of a grid and can be manipulated and used in calculations. ";

            MessageBox.Show(text, "About!");
        }

        /// <summary>
        /// Handles Typing in the TextBox_Contents object, and fires the UpdateCellEvent on pressing enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Contents_KeyUp(object sender, KeyEventArgs e) {
            int col, row;
            // Shift+Tab moves one to the left and tab moves one to the right
            if (e.KeyData.ToString().Equals("Tab, Shift")) {
                spreadsheetPanel1.GetSelection(out col, out row);
                UpdateCellEvent(col, row, TextBox_Contents.Text);
                spreadsheetPanel1.SetSelection(col - 1, row);
                DisplaySelection(spreadsheetPanel1);
            } else if (e.KeyValue.Equals(9)) {
                spreadsheetPanel1.GetSelection(out col, out row);
                UpdateCellEvent(col, row, TextBox_Contents.Text);
                spreadsheetPanel1.SetSelection(col + 1, row);
                DisplaySelection(spreadsheetPanel1);
            }

            // Shift+Enter moves up one and Enter moves down one
            if (e.KeyData.ToString().Equals("Return, Shift")) {
                spreadsheetPanel1.GetSelection(out col, out row);
                UpdateCellEvent(col, row, TextBox_Contents.Text);
                spreadsheetPanel1.SetSelection(col, row - 1);
                DisplaySelection(spreadsheetPanel1);
            } else if (e.KeyValue.Equals(13)) {
                spreadsheetPanel1.GetSelection(out col, out row);
                UpdateCellEvent(col, row, TextBox_Contents.Text);
                spreadsheetPanel1.SetSelection(col, row + 1);
                DisplaySelection(spreadsheetPanel1);
            } else {
                spreadsheetPanel1.GetSelection(out col, out row);
                SetCellValue(col, row, TextBox_Contents.Text);
            }

        }

        /// <summary>
        /// Allows data to be updated as it is entered
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Contents_KeyPress(object sender, KeyPressEventArgs e) {
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            SetCellValue(col, row, TextBox_Contents.Text);
        }

        /// <summary>
        /// Fires the ClosingEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_FormClosing(object sender, FormClosingEventArgs e) {
            if(ClosingEvent != null) {
                ClosingEvent(e);
            }
        }

        /// <summary>
        /// Handles BeforeSelectionChangedEvents from the SpreadsheetPanel and fires UpdateCellEvent as needed
        /// </summary>
        /// <param name="e"></param>
        private void spreadsheetPanel1_BeforeSelectionChanged(MouseEventArgs e) {
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            if(UpdateCellEvent != null) {
                UpdateCellEvent(col, row, TextBox_Contents.Text);
            }
            TextBox_Contents.Focus();
        }

        private void View_Load(object sender, EventArgs e)
        {

        }

    }
}
