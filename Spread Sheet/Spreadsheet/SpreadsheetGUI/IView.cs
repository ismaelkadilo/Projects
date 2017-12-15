// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using System;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    /// <summary>
    /// Controllable interface of View
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Event fired when the 'Close' button is clicked
        /// </summary>
        event Action CloseEvent;

        /// <summary>
        /// Event fired when X button is clicked
        /// </summary>
        event Action<FormClosingEventArgs> ClosingEvent;

        /// <summary>
        /// Event fired when the 'Open' button is clicked
        /// </summary>
        event Action<string> ChooseFileEvent;

        /// <summary>
        /// Event fired when a cell is clicked
        /// </summary>
        event Action<string> GetCellContentsEvent;

        /// <summary>
        /// Event fired when the 'New Sheet' button is clicked
        /// </summary>
        event Action NewEvent;

        /// <summary>
        /// Event fired when the 'Open' button is clicked
        /// </summary>
        event Action<string> OpenFileEvent;

        /// <summary>
        /// Event fired when the 'InNewWindow' button is clicked
        /// </summary>
        event Action OpenInNewEvent;

        /// <summary>
        /// Event fired when the 'Save' button is clicked
        /// </summary>
        event Action SaveEvent;

        /// <summary>
        /// Event fired when the 'Save As' button is clicked
        /// </summary>
        event Action SaveAsEvent;

        /// <summary>
        /// Event fired when a cell is updated
        /// </summary>
        event Action<int, int, string> UpdateCellEvent;

        /// <summary>
        /// Method Called to close the window
        /// </summary>
        void DoClose();

        /// <summary>
        /// Clears the SpreadsheetPanel
        /// </summary>
        void ClearSpreadsheetPanel();

        /// <summary>
        /// Method Called to open a new window
        /// </summary>
        void NewSpreadsheet();

        /// <summary>
        /// Method Called to open a file
        /// </summary>
        /// <param name="name"></param>
        void OpenFile(string name);

        /// <summary>
        /// Method Called to open a file in a new window
        /// </summary>
        void OpenInNew();

        /// <summary>
        /// Method Called to set cell value
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="v"></param>
        void SetCellValue(int col, int row, string v);

        /// <summary>
        /// Method Called to set cell contents
        /// </summary>
        /// <param name="contents"></param>
        void SetCellContents(string contents);

        /// <summary>
        /// Method Called to set the cell selected
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        void SetCellSelection(int col, int row);

    }
}
