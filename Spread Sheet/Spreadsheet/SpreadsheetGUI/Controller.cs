// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using Formulas;
using SS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SpreadsheetGUI {
    /// <summary>
    /// Controls a GUI IView window by handling events
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// The window being controlled
        /// </summary>
        private IView window;

        /// <summary>
        /// The backing spreadsheet model
        /// </summary>
        private Spreadsheet model;

        /// <summary>
        /// The last save location
        /// </summary>
        private string SaveLocation;

        /// <summary>
        /// Initial Constructor that controls the IView window
        /// </summary>
        /// <param name="window"></param>
        public Controller(IView window)
        {
            this.window = window;
            this.model = new Spreadsheet();

            window.ChooseFileEvent += HandleChooseFile;
            window.CloseEvent += HandleClose;
            window.ClosingEvent += HandleClosing;
            window.GetCellContentsEvent += HandleGetCellContents;
            window.NewEvent += HandleNew;
            window.OpenFileEvent += HandleOpenFile;
            window.OpenInNewEvent += HandleOpenInNew;
            window.SaveEvent += HandleSave;
            window.SaveAsEvent += HandleSaveAs;
            window.UpdateCellEvent += HandleUpdateCell;
        }

        /// <summary>
        /// Handles choosing a file
        /// </summary>
        private void HandleChooseFile(string from)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Open SpreadSheet Files";
            fileDialog.DefaultExt = ".ss";
            fileDialog.Filter = "SpreadSheet files|*.ss|All files|*.*";
            DialogResult result = fileDialog.ShowDialog();
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                window.OpenFile(fileDialog.FileName.Replace("\\", "/"));
            } else if (from != null) {
                throw new Exception("User canceled the selection");
            }
            fileDialog.Dispose();
        }

        /// <summary>
        /// Handles a request to update Cell
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="content"></param>
        private void HandleUpdateCell(int col, int row, string content)
        {
            string name;
            ConvertCellName(col, row, out name);
            if (model.GetCellContents(name).Equals("") && content.Equals(""))
            {
                return;
            }
            try
            {
                HashSet<string> CellsToUpdate = new HashSet<string>(model.SetContentsOfCell(name, content));
                foreach (string s in CellsToUpdate)
                {
                    ConvertCellName(out col, out row, s);
                    window.SetCellValue(col, row, model.GetCellValue(s).ToString());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error updating the cell to \"" + content + "\"");
                window.SetCellValue(col, row, model.GetCellValue(name).ToString());
            }
        }

        /// <summary>
        /// Handles a request to Get CellContents
        /// </summary>
        /// <param name="name"></param>
        public void HandleGetCellContents(string name)
        {
            string value = model.GetCellContents(name).ToString();
            if (model.GetCellContents(name).GetType().Equals(typeof(Formula)))
            {
                value = "=" + value;
            }
            window.SetCellContents(value);
        }

        /// <summary>
        /// Handles a request to open file
        /// </summary>
        /// <param name="filename"></param>
        public void HandleOpenFile(string filename)
        {
            StreamReader inputFile = null;
            try
            {
                inputFile = File.OpenText(filename.Replace("\\", "/"));
                model = new Spreadsheet(inputFile, new Regex("^[A-Z][1-9]|[A-Z][1-9]$"));
                HashSet<string> cells = new HashSet<string>(model.GetNamesOfAllNonemptyCells());
                window.ClearSpreadsheetPanel();
                foreach (string name in cells)
                {
                    object content = model.GetCellValue(name);
                    int col, row;
                    ConvertCellName(out col, out row, name);
                    window.SetCellValue(col, row, content.ToString());
                }
                SaveLocation = filename.Replace("\\", "/");
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error loading the file");
                SaveLocation = null;
            }
            inputFile.Close();
            window.SetCellSelection(0, 0);
            HandleGetCellContents("A1");
        }

        /// <summary>
        /// Handles a request to save file
        /// </summary>
        public void HandleSave()
        {
            try
            {
                if (SaveLocation == null)
                {
                    // save file must save a "ss" format
                    SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                    saveFileDialog1.Filter = "SpreadSheet files|*.ss|All files|*.*";
                    saveFileDialog1.Title = "Save SpreadSheet File";
                    saveFileDialog1.ShowDialog();
                    //save the file
                    if (saveFileDialog1.FileName != "")
                    {
                        SaveLocation = saveFileDialog1.FileName;
                        model.Save(File.CreateText(SaveLocation.Replace("\\", "/")));
                    }
                }
                else
                {
                    model.Save(File.CreateText(SaveLocation.Replace("\\", "/")));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred", e.Message);
            }
        }

        /// <summary>
        /// Handles a request to save file as
        /// </summary>
        public void HandleSaveAs()
        {
            try
            {// save file must save a "ss" format
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "SpreadSheet files|*.ss|All files|*.*";
                saveFileDialog1.Title = "Save SpreadSheet File";
                saveFileDialog1.ShowDialog();

                //save the file
                if (saveFileDialog1.FileName != "")
                {
                    SaveLocation = saveFileDialog1.FileName;
                    model.Save(File.CreateText(SaveLocation.Replace("\\", "/")));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occur", e.Message);
            }
        }

        /// <summary>
        /// Handles a request to open a new window
        /// </summary>
        public void HandleNew()
        {
            ViewApplicationContext.GetContext().RunNew();
            window.SetCellSelection(0, 0);
            HandleGetCellContents("A1");
        }

        /// <summary>
        /// Handles a request to open a new window in a different sheet
        /// </summary>
        public void HandleOpenInNew()
        {
            ViewApplicationContext.GetContext().RunAndOpenNew();
            window.SetCellSelection(0, 0);
            HandleGetCellContents("A1");
        }

        /// <summary>
        /// Handles a request to close the window from the 'Close' button in the file menu
        /// </summary>
        public void HandleClose()
        {
            if (!model.Changed)
            {
                window.DoClose();
            }
            else
            {
                string message = "You have unsaved changes, are you sure you want to exit?";
                string caption = "Warning: Unsaved Changes";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);
                if (result.Equals(DialogResult.Yes))
                {
                    window.DoClose();
                }
            }
        }

        /// <summary>
        /// Handles closing when the X button is clicked
        /// </summary>
        /// <param name="e"></param>
        public void HandleClosing(FormClosingEventArgs e)
        {
            if (!model.Changed)
            {
            }
            else
            {
                string message = "You have unsaved changes, are you sure you want to exit?";
                string caption = "Warning: Unsaved Changes";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                result = MessageBox.Show(message, caption, buttons);
                if (!result.Equals(DialogResult.Yes))
                {
                    e.Cancel = true;
                }
            }
        }

        // Helper Methods: 
        /// <summary>
        /// Sets the column and row values to the corresponding cell name
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="name"></param>
        public void ConvertCellName(out int col, out int row, string name)
        {
            col = name.Substring(0, 1).ToCharArray()[0] - 65;
            int.TryParse(name.Substring(1), out row);
            row -= 1;
        }

        /// <summary>
        /// Sets the cell name to the corresponding column and row values
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="name"></param>
        public void ConvertCellName(int col, int row, out string name)
        {
            name = ((Convert.ToChar(col + 65)) + "" + (row + 1));
        }
    }
}
