// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControllerTester;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace SpreadsheetGUI {
    [TestClass, ExcludeFromCodeCoverage]
    public class ControllerTester
    {
        /// <summary>
        /// Tests the CloseEvent
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireCloseEvent();
            Assert.IsTrue(window.CalledDoClose);
        }

        /// <summary>
        /// Tests the CloseEvent with changes
        /// </summary>
        [TestMethod]
        public void TestMethod1_2()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireOpenFileEvent();
            window.FireUpdateCellEvent(9, 0, "Test");
            window.FireSaveEvent();
            window.FireCloseEvent();
            Assert.IsTrue(window.CalledDoClose);
        }

        /// <summary>
        /// Tests the CloseEvent with after save
        /// </summary>
        [TestMethod]
        public void TestMethod1_3()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireUpdateCellEvent(0, 0, "Test");
            window.FireSaveEvent();
            window.FireCloseEvent();
            Assert.IsTrue(window.CalledDoClose);
        }

        /// <summary>
        /// Tests the OpenInNewEvent
        /// </summary>
        [TestMethod]
        public void TestMethod2()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.NewSpreadsheet();
            window.OpenInNew();
            Assert.IsTrue(window.CalledOpenNew);
            Assert.IsTrue(window.CalledOpenInNew);
        }

        /// <summary>
        /// Tests SetCellValue
        /// </summary>
        [TestMethod]
        public void TestMethod3()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.SetCellValue(1, 2, "b");
            Assert.IsTrue(window.CalledSetCellValue);
        }

        /// <summary>
        /// Tests SetCellSelection
        /// </summary>
        [TestMethod]
        public void TestMethod4()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.SetCellSelection(1, 2);
            Assert.IsTrue(window.CalledSetCellSelection);
        }

        /// <summary>
        /// Tests SetCellContents
        /// </summary>
        [TestMethod]
        public void TestMethod5()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.SetCellContents("ismael");
            Assert.IsTrue(window.CalledSetCellContents);
        }

        /// <summary>
        /// Tests the FileChosenEvent
        /// </summary>
        [TestMethod]
        public void TestMethod6()
        {

            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireFileChosenEvent("../../../SpreadsheetGUI/Test Files/Test1.ss");
            window.FireGetCellContentsEvent();
            Assert.AreEqual("Test", window.GetCellContents.ToString());
        }

        /// <summary>
        /// Tests the ClosingEvent
        /// </summary>
        [TestMethod]
        public void TestMethod7()
        {
            FormClosingEventArgs e = new FormClosingEventArgs(new CloseReason(), false);
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireClosingEvent(e);
            Assert.IsTrue(window.CalledClosing);
        }

        /// <summary>
        /// Tests the ClosingEvent with changes
        /// </summary>
        [TestMethod]
        public void TestMethod7_2()
        {
            FormClosingEventArgs e = new FormClosingEventArgs(new CloseReason(), false);
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireClosingEvent(e);
            window.FireUpdateCellEvent(0, 0, "Test");
            Assert.IsTrue(window.CalledClosing);
        }

        /// <summary>
        /// Tests SetCellContents
        /// </summary>
        [TestMethod]
        public void TestMethod8()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.SetCellContents("a1");
            Assert.IsTrue(window.CalledSetCellContents);

        }

        /// <summary>
        /// Tests ChooseFileEvent
        /// </summary>
        [TestMethod]
        public void TestMethod9()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireChooseFileEvent();
            Assert.IsTrue(window.FileChosen);
        }

        /// <summary>
        /// Tests UpdateCell Event with empty value
        /// </summary>
        [TestMethod]
        public void TestMethod10()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireUpdateCellEvent(10, 10, "");
            Assert.IsTrue(window.CellUpdated);
        }

        /// <summary>
        /// Tests UpdateCell Event with error
        /// </summary>
        [TestMethod]
        public void TestMethod10_2()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireUpdateCellEvent(10, 10, "=a");
            Assert.IsTrue(window.CellUpdated);
        }

        /// <summary>
        /// Tests SaveAs Event 
        /// </summary>
        [TestMethod]
        public void TestMethod11()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireSaveAsEvent();
            Assert.IsTrue(window.SaveAs);
        }

        /// <summary>
        /// Tests NewEvent
        /// </summary>
        [TestMethod]
        public void TestMethod12()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireNew();
            Assert.IsTrue(window.New);
        }

        /// <summary>
        /// Tests OpenInNewEvent
        /// </summary>
        [TestMethod]
        public void TestMethod13()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireOpenInNew();
            Assert.IsTrue(window.OpenInNewSpread);
        }

        /// <summary>
        /// Tests CloseEvent with changes
        /// </summary>
        [TestMethod]
        public void TestMethod14()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireCloseEvent();
            window.FireUpdateCellEvent(10, 10, "Test");
            window.FireCloseEvent();
            Assert.IsTrue(window.CalledDoClose);
            Assert.IsTrue(window.CellUpdated);
            window.SetCellContents("ismael");
            Assert.IsTrue(window.CalledSetCellContents);
        }

        /// <summary>
        /// Tests ClosingEvent with changes
        /// </summary>
        [TestMethod]
        public void TestMethod15()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireUpdateCellEvent(10, 10, "Test");
            window.FireClosingEvent(null);
            Assert.IsTrue(window.CellUpdated);
            Assert.IsTrue(window.CalledClosing);
        }
        /// <summary>
        /// Tests All the events 
        /// </summary>
        [TestMethod]
        public void TestMethod16()
        {
            ViewStub window = new ViewStub();
            Controller controller = new Controller(window);
            window.FireNew();        
            window.SetCellContents("ismael");
            window.SetCellSelection(1, 2);
            window.SetCellValue(1, 2,"c");
            window.FireUpdateCellEvent(10, 10, "=a");
            window.FireCloseEvent();

            Assert.IsTrue(window.New);        
            Assert.IsTrue(window.CalledSetCellContents);
            Assert.IsTrue(window.CalledSetCellSelection);
            Assert.IsTrue(window.CalledSetCellValue);
            Assert.IsTrue(window.CellUpdated);
            Assert.IsTrue(window.CalledDoClose);
        }
    }
}