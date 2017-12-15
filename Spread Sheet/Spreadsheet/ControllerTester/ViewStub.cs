// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using SpreadsheetGUI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace ControllerTester {
    [ExcludeFromCodeCoverage]
    class ViewStub : IView
    {
        // These nine events implement the interface
        public event Action CloseEvent;
        public event Action<FormClosingEventArgs> ClosingEvent;
        public event Action<string> ChooseFileEvent;
        public event Action<string> OpenFileEvent;
        public event Action<string> GetCellContentsEvent;
        public event Action NewEvent;
        public event Action OpenInNewEvent;
        public event Action SaveAsEvent;
        public event Action SaveEvent;
        public event Action<int, int, string> UpdateCellEvent;

        //These properties record whether a method has been called
        /// <summary>
        /// True if FireChooseFileEvent is called
        /// </summary>
        public bool FileChosen 
        {
           get; private set;
        }

        /// <summary>
        /// True if DoClose is called
        /// </summary>
        public bool CalledDoClose
        {
            get; private set;
        }

        /// <summary>
        /// True if OpenNew is called
        /// </summary>
        public bool CalledOpenNew
        {
            get; private set;
        }

        /// <summary>
        /// True if OpenInNew is called
        /// </summary>
        public bool CalledOpenInNew
        {
            get; private set;
        }

        /// <summary>
        /// True if SetCellValue is called
        /// </summary>
        public bool CalledSetCellValue
        {
            get; private set;
        }

        /// <summary>
        /// True if SetCellSelection is called
        /// </summary>
        public bool CalledSetCellSelection
        {
            get; private set;
        }

        /// <summary>
        /// True if SetCellContents is called
        /// </summary>
        public bool CalledSetCellContents
        {
            get; private set;
        }

        /// <summary>
        /// The contents of the cell
        /// </summary>
        public object GetCellContents
        {
            set; get;
        }

        /// <summary>
        /// True if ClosingEvent is fired
        /// </summary>
        public bool CalledClosing
        {
            get; private set;
        }
        
        /// <summary>
        /// True if UpdateCellEvent is fired
        /// </summary>
        public bool CellUpdated { get; internal set; }

        /// <summary>
        /// True if SaveAs is called
        /// </summary>
        public bool SaveAs { get; internal set; }

        /// <summary>
        /// True if NewEvent is fired
        /// </summary>
        public bool New { get; internal set; }

        /// <summary>
        /// True if OpenInNew is fired
        /// </summary>
        public bool OpenInNewSpread { get; internal set; }

        //these methods cause events to be fired
        /// <summary>
        /// Fires SaveEvent
        /// </summary>
        internal void FireSaveEvent() {
            SaveEvent();
        }

        /// <summary>
        /// Fires SaveAsEvent
        /// </summary>
        internal void FireSaveAsEvent() {
            if (SaveAsEvent != null) {
                SaveAsEvent();
                SaveAs = true;
            }
        }

        /// <summary>
        /// Fires OpenFileEvent
        /// </summary>
        internal void FireOpenFileEvent() {
            OpenFileEvent("../../../SpreadsheetGUI/Test Files/Test4.ss");
        }

        /// <summary>
        /// Fires CloseEvent
        /// </summary>
        public void FireCloseEvent()
        {
            if (CloseEvent != null)
            {
                CloseEvent();
            }
        }

        /// <summary>
        /// Fires ClosingEvent
        /// </summary>
        /// <param name="e"></param>
        public void FireClosingEvent(FormClosingEventArgs e)
        {
            if (ClosingEvent != null)
            {
                ClosingEvent(e);
                CalledClosing = true;
            }
        }

        /// <summary>
        /// Fires GetCellContentsEvent 
        /// </summary>
        internal void FireGetCellContentsEvent() {
            if(GetCellContentsEvent != null) {
                GetCellContentsEvent("A1");
                GetCellContents = "Test";
            }
        }

        /// <summary>
        /// Fires OpenFileEvent
        /// </summary>
        /// <param name="filename"></param>
        public void FireFileChosenEvent(string filename)
        {
            if (OpenFileEvent != null)
            {
                OpenFileEvent(filename);
            }
        }

        /// <summary>
        /// Fires UpdateCellEvent
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="content"></param>
        public void FireUpdateCellEvent(int col, int row, string content)
        {
            if(UpdateCellEvent!= null)
            {
                UpdateCellEvent(col, row, content);
                CellUpdated = true;
            }
        }

        /// <summary>
        /// Fires NewEvent
        /// </summary>
        internal void FireNew()
        {
         if  ( NewEvent != null)
            {
                NewEvent();
                New = true;
            }
        }

        /// <summary>
        /// Fires OpenInNewEvent
        /// </summary>
        internal void FireOpenInNew()
        {
            if(OpenInNewEvent != null)
            {
                OpenInNewEvent();
                OpenInNewSpread = true;
            }
        }

        /// <summary>
        /// Fires ChooseFileEvent
        /// </summary>
        internal void FireChooseFileEvent() {
            ChooseFileEvent(null);
            FileChosen = true;
        }

        // these seven methods implements the interface
        /// <summary>
        /// Sets CalledDoClose and CalledClosing
        /// </summary>
        public void DoClose()
        {
            CalledDoClose = true;
            CalledClosing = true;
        }

        /// <summary>
        /// Unused
        /// </summary>
        /// <param name="filename"></param>
        public void OpenFile(string filename) {
            // Intentionally Empty
        }
        
        /// <summary>
        /// Sets CalledOpenInNew
        /// </summary>
        public void OpenInNew()
        {
            CalledOpenInNew = true;
        }

        /// <summary>
        /// Sets CalledOpenNew
        /// </summary>
        public void NewSpreadsheet()
        {
            CalledOpenNew = true;
        }

        /// <summary>
        /// Sets CalledSetCellContents
        /// </summary>
        /// <param name="contents"></param>
        public void SetCellContents(string contents)
        {
            CalledSetCellContents = true;
        }

        /// <summary>
        /// Sets CalledSetCellSelection
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        public void SetCellSelection(int col, int row)
        {
            CalledSetCellSelection = true;
        }

        /// <summary>
        /// Sets CalledSetCellValue
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="v"></param>
        public void SetCellValue(int col, int row, string v)
        {
            CalledSetCellValue = true;
        }

        /// <summary>
        /// Unused
        /// </summary>
        public void ClearSpreadsheetPanel() {
           // Intentionally Empty
        }
    }
}
