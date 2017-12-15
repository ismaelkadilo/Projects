// Implementation by Nathan Milot, u1063587

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Formulas;
using SS;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System;
using System.IO;

namespace SpreadsheetTests {
    [TestClass, ExcludeFromCodeCoverage]
    public class SpreadsheetTests {

        // PS6 Tests: 
        // New Constructors Tests: 
        /// <summary>
        /// Tests to see if constructors handle null parameters properly
        /// </summary>
        [TestMethod]
        public void NullTests() {
            // If passed null for regex value, set to accept anything
            AbstractSpreadsheet s1 = new Spreadsheet(null);

            // If passed null source, throw IOException
            try {
                AbstractSpreadsheet s2 = new Spreadsheet(null, new Regex("^.*$"));
            } catch (IOException) {
            }

            // If passed null for regex value, set to accept anything 
            AbstractSpreadsheet s3 = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SampleSavedSpreadsheet.xml"), null);
        }

        /// <summary>
        /// Tests the Constructor with the text reader
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest1() {
            AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SampleSavedSpreadsheet.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
            Assert.AreEqual(1.5, sheet.GetCellContents("A1"));
            Assert.AreEqual(8.0, sheet.GetCellContents("A2"));
            Assert.AreEqual("A1*A2+23", sheet.GetCellContents("A3").ToString());
            Assert.AreEqual(35.0, sheet.GetCellValue("A3"));
            Assert.AreEqual("Hello", sheet.GetCellContents("B2"));
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an invalid formula
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest2() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SavedSpreadsheetWithInvalidFormula.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetReadException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an old invalid cell name
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest3() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SavedSpreadsheetWithInvalidCellName.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetReadException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an new invalid cell name
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest4() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SampleSavedSpreadsheet.xml"), new Regex(@"^[a-zA-Z][a-zA-Z]+[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetVersionException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with a circular dependency
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest5() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SavedSpreadsheetWithCircularDependency.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetReadException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an invalid regex
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest6() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SavedSpreadsheetWithInvalidRegex.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetReadException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an invalid spreadsheet schema
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest7() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SavedSpreadsheetWithInvalidSpreadsheetSchema.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetReadException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an invalid source
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest8() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(null, new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (IOException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an invalid source
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest9() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test.txt"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (IOException) {
            }
        }

        /// <summary>
        /// Tests the Constructor with the text reader with an invalid source
        /// </summary>
        [TestMethod]
        public void ReadConstructorTest10() {
            try {
                AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SavedSpreadsheetWithDuplicateName.xml"), new Regex(@"^[a-zA-Z]*[0-9]*$"));
                Assert.Fail();
            } catch (SpreadsheetReadException) {
            }
        }

        // Changed Tests: 
        /// <summary>
        /// Checks to see if change is handled properly
        /// </summary>
        [TestMethod]
        public void ChangeTest1() {
            string SaveName = "../../../Spreadsheet/ChangeTest1.xml";
            AbstractSpreadsheet sheet = new Spreadsheet(new Regex(@"^[a-zA-Z]+[0-9]+$"));
            Assert.IsFalse(sheet.Changed);
            sheet.Save(File.CreateText(SaveName));
            Assert.IsFalse(sheet.Changed);
            sheet.SetContentsOfCell("A1", "Test");
            Assert.IsTrue(sheet.Changed);
            Assert.IsTrue(sheet.Changed);
            sheet.Save(File.CreateText(SaveName));
            Assert.IsFalse(sheet.Changed);
            sheet.SetContentsOfCell("A1", "");
            Assert.IsTrue(sheet.Changed);
            sheet.Save(File.CreateText(SaveName));
        }

        // Get Cell Value Tests: 
        /// <summary>
        /// Basic tests to see if GetCellValue is working properly to return strings, doubles, and formula values properly
        /// </summary>
        [TestMethod]
        public void GetCellValueTest() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "2.25");
            Assert.AreEqual(2.25, sheet.GetCellValue("A1"));
            sheet.SetContentsOfCell("B1", "=100*A1");
            Assert.AreEqual(225.0, sheet.GetCellValue("B1"));
            sheet.SetContentsOfCell("C1", "=A1+B1");
            Assert.AreEqual(227.25, sheet.GetCellValue("C1"));
            sheet.SetContentsOfCell("A2", "String Test");
            Assert.AreEqual("String Test", sheet.GetCellValue("A2"));
        }

        // Save Tests: 
        /// <summary>
        /// Tests to see if spreadsheet is saved correctly
        /// </summary>
        [TestMethod]
        public void SaveTest1() {
            AbstractSpreadsheet sheet1 = new Spreadsheet(new Regex(@"^[a-zA-Z]+[0-9]+$"));
            sheet1.SetContentsOfCell("A1", "1.25");
            sheet1.SetContentsOfCell("A2", "=A1*2");
            sheet1.SetContentsOfCell("A3", "=A2+A1");
            sheet1.SetContentsOfCell("B1", "Test");
            sheet1.Save(File.CreateText("../../../Spreadsheet/SaveTest1.xml"));

            AbstractSpreadsheet sheet2 = new Spreadsheet(File.OpenText("../../../Spreadsheet/SaveTest1.xml"), new Regex(@"^[a-zA-Z]+[0-9]+$"));
            Assert.AreEqual(sheet1.GetCellValue("A1"), sheet2.GetCellValue("A1"));
            Assert.AreEqual(sheet1.GetCellValue("A2"), sheet2.GetCellValue("A2"));
            Assert.AreEqual(sheet1.GetCellValue("A3"), sheet2.GetCellValue("A3"));
            Assert.AreEqual(sheet1.GetCellValue("B1"), sheet2.GetCellValue("B1"));
        }

        /// <summary>
        /// Tests to see if the save method handles exceptions correctly
        /// </summary>
        [TestMethod]
        public void SaveTest2() {
            AbstractSpreadsheet sheet = new Spreadsheet(File.OpenText("../../../Spreadsheet/Test Files/SampleSavedSpreadsheet.xml"), null);
            try {
                sheet.Save(null);
                Assert.Fail();
            } catch (IOException) {
            }
        }

        // Set Content of Cell Tests: 
        /// <summary>
        /// Tests setting formulas
        /// </summary>
        [TestMethod]
        public void SetFormulaTests() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "=B1");
            try {
                sheet.SetContentsOfCell("A1", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                sheet.SetContentsOfCell(null, "=B2");
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.SetContentsOfCell("A1", "=.B2");
                Assert.Fail();
            } catch (FormulaFormatException) {
            }

            try {
                sheet.SetContentsOfCell("B1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }
        }

        /// <summary>
        /// Tests setting doubles
        /// </summary>
        [TestMethod]
        public void SetDoubleTests() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "25.1");
            try {
                sheet.SetContentsOfCell("A1", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                sheet.SetContentsOfCell(null, "10.1");
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.SetContentsOfCell("4A1", "5.1");
                Assert.Fail();
            } catch (InvalidNameException) {
            }
        }

        /// <summary>
        /// Tests setting strings
        /// </summary>
        [TestMethod]
        public void SetStringTests() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "Test 1");
            try {
                sheet.SetContentsOfCell("A1", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                sheet.SetContentsOfCell(null, "Test 3");
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.SetContentsOfCell(".A1", "Test 4");
                Assert.Fail();
            } catch (InvalidNameException) {
            }
        }

        // FormulaError Tests: 
        /// <summary>
        /// Tests to see if the value of a formula is set to a FormulaError when referencing a cell with a string as its value
        /// </summary>
        [TestMethod]
        public void FormulaErrorTest1() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "=B1+1");
            sheet.SetContentsOfCell("B1", "Test");
            Assert.AreEqual(new FormulaError().GetType(), sheet.GetCellValue("A1").GetType());
        }

        // Stress Tests: 
        /// <summary>
        /// Tests to see if spreadsheet can handle updating large amounts of data after calling get value
        /// </summary>
        [TestMethod]
        public void StressTest1() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            double total = 500;
            for (int i = 1; i <= total; i++) {
                sheet.SetContentsOfCell("A" + i, "=A" + (i + 1) + "+1");
            }
            sheet.SetContentsOfCell("A" + (total + 1), "0");
            Assert.AreEqual(500.0, sheet.GetCellValue("A1"));
        }

        /// <summary>
        /// Tests to see if spreadsheet can handle updating large amounts of data after calling get value
        /// </summary>
        [TestMethod]
        public void StressTest2() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            double total = 300;
            for (int i = 0; i <= total; i++) {
                sheet.SetContentsOfCell("A" + i, "=A" + (i + 1) + "+1");
            }
            sheet.SetContentsOfCell("A" + (total + 1), "0");
            Assert.AreEqual(total, sheet.GetCellValue("A1"));

            for (int i = 1; i <= total; i++) {
                sheet.SetContentsOfCell("B" + i, "=B" + (i + 1) + "+1");
            }
            sheet.SetContentsOfCell("B" + (total + 1), "0");
            Assert.AreEqual(total, sheet.GetCellValue("B1"));
        }

        /// <summary>
        /// Tests to see if spreadsheet can handle updating large amounts of data after calling get value
        /// </summary>
        [TestMethod]
        public void StressTest3() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            double total = 500;
            for (int i = 1; i <= total; i++) {
                sheet.SetContentsOfCell("A" + i, "=A" + (i + 1) + "+1");
            }
            sheet.SetContentsOfCell("A" + (total + 1), "0");
            for (int i = 1; i <= total; i++) {
                Assert.AreEqual(total + 1.0 - i, sheet.GetCellValue("A" + i));
            }

            // Tests if values are still valid after changes
            Assert.AreEqual(total, sheet.GetCellValue("A1"));
            Assert.AreEqual(total / 2, sheet.GetCellValue("A" + ((total / 2) + 1)));
            sheet.SetContentsOfCell("A" + total / 2, "50");
            Assert.AreEqual(50.0, sheet.GetCellValue("A" + total / 2));
            Assert.AreEqual(total / 2 + 49, sheet.GetCellValue("A1"));
            Assert.AreEqual(total / 2, sheet.GetCellValue("A" + ((total / 2) + 1)));
        }

        /// <summary>
        /// Tests to see if spreadsheet can handle large amounts of data after calling get value
        /// </summary>
        [TestMethod]
        public void StressTest4() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            double total = 10000;
            for (int i = 1; i <= total; i++) {
                sheet.SetContentsOfCell("A" + i, (10 * i) + "");
            }
            for (int i = 1; i <= total; i++) {
                Assert.AreEqual(10.0 * i, sheet.GetCellValue("A" + i));
            }
        }

        // PS5 Tests: 
        // Method Tests: 
        /// <summary>
        /// Tests the validity of the CellNamePattern Regex
        /// </summary>
        [TestMethod]
        public void RegexTests() {
            Regex CellNamePattern = new Regex(@"^[a-zA-Z]+[0-9]+$");
            Assert.IsTrue(CellNamePattern.IsMatch("A1"));
            Assert.IsTrue(CellNamePattern.IsMatch("xz678"));
            Assert.IsTrue(CellNamePattern.IsMatch("B100075"));

            Assert.IsFalse(CellNamePattern.IsMatch("A1S"));
            Assert.IsFalse(CellNamePattern.IsMatch("123"));
            Assert.IsFalse(CellNamePattern.IsMatch("test"));
        }

        /// <summary>
        /// Tests the SetContentsOfCell formula method
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellFormula() {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("B1", "=A1 * A1");
            sheet.SetContentsOfCell("C1", "=B1 + A1");
            ISet<string> d1result = sheet.SetContentsOfCell("D1", "=B1 - C1");
            ISet<string> result = sheet.SetContentsOfCell("A1", "=F1");
            Assert.AreEqual("A1, B1, C1, D1", string.Join(", ", result));
            Assert.AreEqual("D1", string.Join(", ", d1result));
        }

        /// <summary>
        /// Tests the SetContentsOfCell formula method for circular dependencies
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellFormulaCircular() {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("B1", "=A1 * A1");
            sheet.SetContentsOfCell("C1", "=B1 + A1");
            sheet.SetContentsOfCell("D1", "=B1 - C1");
            sheet.SetContentsOfCell("A1", "=F1");
            try {
                sheet.SetContentsOfCell("A1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }
            try {
                sheet.SetContentsOfCell("A1", "=D1");
                Assert.Fail();
            } catch (CircularException) {
            }
        }

        /// <summary>
        /// Tests the SetContentsOfCell formula method
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellText() {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("B1", "Test");
            sheet.SetContentsOfCell("C1", "A1");
            ISet<string> d1result = sheet.SetContentsOfCell("D1", "B1 - C1");
            ISet<string> result = sheet.SetContentsOfCell("A1", "Title");
            Assert.AreEqual("A1", string.Join(", ", result));
            Assert.AreEqual("D1", string.Join(", ", d1result));
        }

        /// <summary>
        /// Tests the SetContentsOfCell formula method
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellNumber() {
            Spreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("B1", "10");
            sheet.SetContentsOfCell("C1", "0.25");
            ISet<string> d1result = sheet.SetContentsOfCell("D1", "31654354613.16547651651");
            ISet<string> result = sheet.SetContentsOfCell("A1", "-1560.7231");
            Assert.AreEqual("A1", string.Join(", ", result));
            Assert.AreEqual("D1", string.Join(", ", d1result));
        }

        /// <summary>
        /// Builds a large spreadsheet and checks size, and a few other things
        /// </summary>
        [TestMethod]
        public void BuildLargeSpreadsheet() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            for (int i = 0; i < 26; i++) {
                char initial = (char)(65 + i);
                for (int j = 1; j < 1001; j++) {
                    sheet.SetContentsOfCell(initial + "" + j, initial + "" + j);
                }
            }
            HashSet<string> result = new HashSet<string>(sheet.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(26000, result.Count);
            Assert.AreEqual("Z37", sheet.GetCellContents("Z37"));
            try {
                Assert.AreEqual("1AB123", sheet.GetCellContents("1AB123"));
                Assert.Fail();
            } catch (InvalidNameException) {
            }
            Assert.AreEqual("", sheet.GetCellContents("ZZZZZZZZZ9999999999999999"));
            sheet.SetContentsOfCell("ZZZZZZZZZ9999999999999999", "=A1 + B1");
            Assert.AreEqual("A1+B1", sheet.GetCellContents("ZZZZZZZZZ9999999999999999").ToString());

        }

        /// <summary>
        /// Makes sure old dependencies are removed that would cause a circular exception when a cell is changed
        /// </summary>
        [TestMethod]
        public void RemoveDependenciesAfterChange() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "=B1");
            try {
                sheet.SetContentsOfCell("B1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }

            sheet.SetContentsOfCell("A1", "Test");
            sheet.SetContentsOfCell("B1", "=A1");

            try {
                sheet.SetContentsOfCell("A1", "=B1");
                Assert.Fail();
            } catch (CircularException) {
            }

            sheet.SetContentsOfCell("A1", "1205.654");
            sheet.SetContentsOfCell("A1", "=C1 + 54789");
            sheet.SetContentsOfCell("B1", "=A1 + C1");
        }

        /// <summary>
        /// Checks to see if cells reset to their previous state after a circular dependency exception
        /// </summary>
        [TestMethod]
        public void ResetAfterCircularDependencyException() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "=B1 + C1");
            try {
                sheet.SetContentsOfCell("B1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }
            List<string> NonEmpty = new List<string>(sheet.GetNamesOfAllNonemptyCells());
            Assert.IsFalse(NonEmpty.Contains("B1"));

            try {
                sheet.SetContentsOfCell("A1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }

            sheet.SetContentsOfCell("B1", "Test");
            sheet.SetContentsOfCell("C1", "564.8794");

            try {
                sheet.SetContentsOfCell("B1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }

            try {
                sheet.SetContentsOfCell("C1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }

        }

        // Parameter Validity Tests: 
        /// <summary>
        /// Tests to make sure inputs throw the right exceptions
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellFormulaValidity() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            try {
                sheet.SetContentsOfCell(null, "=A3");
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.SetContentsOfCell("78964ADFA2", "=A3");
                Assert.Fail();
            } catch (InvalidNameException) {
            }
        }

        /// <summary>
        /// Tests to make sure inputs throw the right exceptions
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellStringValidity() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            try {
                sheet.SetContentsOfCell(null, "Text");
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.SetContentsOfCell("A1", null);
                Assert.Fail();
            } catch (ArgumentNullException) {
            }

            try {
                sheet.SetContentsOfCell("78964ADFA2", "Text");
                Assert.Fail();
            } catch (InvalidNameException) {
            }
        }

        /// <summary>
        /// Tests to make sure inputs throw the right exceptions
        /// </summary>
        [TestMethod]
        public void SetContentsOfCellDoubleValidity() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            try {
                sheet.SetContentsOfCell(null, "10.275");
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.SetContentsOfCell("78964ADFA2", "315.1654980");
                Assert.Fail();
            } catch (InvalidNameException) {
            }
        }

        /// <summary>
        /// Tests to make sure inputs throw the right exceptions
        /// </summary>
        [TestMethod]
        public void GetCellContentsValidity() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "=B1");
            try {
                sheet.GetCellContents(null);
                Assert.Fail();
            } catch (InvalidNameException) {
            }

            try {
                sheet.GetCellContents("78964ADFA2");
                Assert.Fail();
            } catch (InvalidNameException) {
            }
        }

        // Miscellaneous Tests: 
        /// <summary>
        /// Tests to see if setting cell to "" resets the cell to empty
        /// </summary>
        [TestMethod]
        public void ResetCellToEmpty() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "Test");
            Assert.IsTrue(new HashSet<string>(sheet.GetNamesOfAllNonemptyCells()).Contains("A1"));
            sheet.SetContentsOfCell("A1", "");
            Assert.IsFalse(new HashSet<string>(sheet.GetNamesOfAllNonemptyCells()).Contains("A1"));
        }

        /// <summary>
        /// Tests to make sure cells are reset properly after a circular exception
        /// </summary>
        [TestMethod]
        public void ResetCell() {
            AbstractSpreadsheet sheet = new Spreadsheet();
            sheet.SetContentsOfCell("A1", "Test");
            try {
                sheet.SetContentsOfCell("A1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }
            Assert.AreEqual("Test", sheet.GetCellContents("A1"));

            sheet.SetContentsOfCell("A1", "2.75");
            try {
                sheet.SetContentsOfCell("A1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }
            Assert.AreEqual(2.75, sheet.GetCellContents("A1"));

            sheet.SetContentsOfCell("A1", "=B1");
            try {
                sheet.SetContentsOfCell("A1", "=A1");
                Assert.Fail();
            } catch (CircularException) {
            }
            Assert.AreEqual("B1", sheet.GetCellContents("A1").ToString());
        }

    }
}
