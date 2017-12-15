// Implementation by Nathan Milot, u1063587

using System;
using System.Collections.Generic;
using Formulas;
using System.Text.RegularExpressions;
using Dependencies;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics.CodeAnalysis;

namespace SS
{

    /// <summary>
    /// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
    /// spreadsheet consists of a regular expression (called IsValid below) and an infinite 
    /// number of named cells.
    /// 
    /// A string is a valid cell name if and only if (1) s consists of one or more letters, 
    /// followed by a non-zero digit, followed by zero or more digits AND (2) the C#
    /// expression IsValid.IsMatch(s.ToUpper()) is true.
    /// 
    /// For example, "A15", "a15", "XY32", and "BC7" are valid cell names, so long as they also
    /// are accepted by IsValid.  On the other hand, "Z", "X07", and "hello" are not valid cell 
    /// names, regardless of IsValid.
    /// 
    /// Any valid incoming cell name, whether passed as a parameter or embedded in a formula,
    /// must be normalized by converting all letters to upper case before it is used by this 
    /// this spreadsheet.  For example, the Formula "x3+a5" should be normalize to "X3+A5" before 
    /// use.  Similarly, all cell names and Formulas that are returned or written to a file must also
    /// be normalized.
    /// 
    /// A spreadsheet contains a cell corresponding to every possible cell name.  
    /// In addition to a name, each cell has a contents and a value.  The distinction is
    /// important, and it is important that you understand the distinction and use
    /// the right term when writing code, writing comments, and asking questions.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In an empty spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError.
    /// The value of a Formula, of course, can depend on the values of variables.  The value 
    /// of a Formula variable is the value of the spreadsheet cell it names (if that cell's 
    /// value is a double) or is undefined (otherwise).  If a Formula depends on an undefined
    /// variable or on a division by zero, its value is a FormulaError.  Otherwise, its value
    /// is a double, as specified in Formula.Evaluate.
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    /// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
    /// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
    /// dependency.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Spreadsheet : AbstractSpreadsheet
    {

        // Private Variables: 
        /// <summary>
        /// Regex pattern to test validity of cell names based on user input
        /// </summary>
        private Regex isValid;

        /// <summary>
        /// Regex pattern to test cell names
        /// </summary>
        private Regex CellNamePattern = new Regex(@"^[a-zA-Z]+[0-9]+$");

        /// <summary>
        /// Stores cell information
        /// </summary>
        private Dictionary<string, Cell> Cells;

        /// <summary>
        /// Stores dependencies
        /// </summary>
        private DependencyGraph dependencies;

        /// <summary>
        /// Backing value of Changed property
        /// </summary>
        private bool changed;

        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {
            get
            {
                return changed;
            }

            protected set
            {
                changed = Changed;
            }
        }

        // Constructors: 
        /// <summary>
        /// Creates an empty Spreadsheet whose IsValid regular expression accepts every string.
        /// </summary>
        public Spreadsheet()
        {
            Cells = new Dictionary<string, Cell>();
            dependencies = new DependencyGraph();
            isValid = new Regex("^.*$");
            changed = false;
        }

        /// Creates an empty Spreadsheet whose IsValid regular expression is provided as the parameter
        public Spreadsheet(Regex isValid) : this()
        {
            if (isValid != null)
            {
                this.isValid = isValid;
            }
        }

        /// Creates a Spreadsheet that is a duplicate of the spreadsheet saved in source.
        ///
        /// See the AbstractSpreadsheet.Save method and Spreadsheet.xsd for the file format 
        /// specification.  
        ///
        /// If there's a problem reading source, throws an IOException. 
        ///
        /// Else if the contents of source are not consistent with the schema in Spreadsheet.xsd, 
        /// throws a SpreadsheetReadException. 
        ///
        /// Else if the IsValid string contained in source is not a valid C# regular expression, throws
        /// a SpreadsheetReadException.  (If the exception is not thrown, this regex is referred to
        /// below as oldIsValid.)
        ///
        /// Else if there is a duplicate cell name in the source, throws a SpreadsheetReadException.
        /// (Two cell names are duplicates if they are identical after being converted to upper case.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a 
        /// SpreadsheetReadException.  (Use oldIsValid in place of IsValid in the definition of 
        /// cell name validity.)
        ///
        /// Else if there is an invalid cell name or an invalid formula in the source, throws a
        /// SpreadsheetVersionException.  (Use newIsValid in place of IsValid in the definition of
        /// cell name validity.)
        ///
        /// Else if there's a formula that causes a circular dependency, throws a SpreadsheetReadException. 
        ///
        /// Else, create a Spreadsheet that is a duplicate of the one encoded in source except that
        /// the new Spreadsheet's IsValid regular expression should be newIsValid.
        public Spreadsheet(TextReader source, Regex newIsValid) : this()
        {
            if (newIsValid == null)
            {
                newIsValid = new Regex("^.*$");
            }
            XmlSchemaSet sc = new XmlSchemaSet();
            sc = new XmlSchemaSet();
            sc.Add(null, "Spreadsheet.xsd");
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = sc;
            Regex oldIsValid = new Regex("^.*$");
            XmlReader reader = null;
            try
            {
                reader = XmlReader.Create(source, settings);
            }
            catch (Exception e)
            {
                throw new IOException(e.Message);
            }
            try
            {
                using (reader)
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    try
                                    {
                                        oldIsValid = new Regex(reader["IsValid"]);
                                    }
                                    catch (Exception e)
                                    {
                                        throw new SpreadsheetReadException(e.Message);
                                    }
                                    break;

                                case "cell":
                                    if (Cells.ContainsKey(reader["name"].ToUpper()))
                                    {
                                        throw new SpreadsheetReadException("Duplicate cell names found");
                                    } 
                                    if (oldIsValid.IsMatch(reader["name"].ToUpper()) && CellNamePattern.IsMatch(reader["name"].ToUpper()))
                                    {
                                        if (newIsValid.IsMatch(reader["name"].ToUpper()))
                                        {
                                            try
                                            {
                                                SetContentsOfCell(reader["name"].ToUpper(), reader["contents"]);
                                            }
                                            catch (Exception e)
                                            {
                                                throw new SpreadsheetReadException(e.Message);
                                            }
                                        }
                                        else
                                        {
                                            throw new SpreadsheetVersionException("Invalid Cell name in new version: " + reader["name"]);
                                        }

                                    }
                                    else
                                    {
                                        throw new SpreadsheetReadException("Non-valid cell name: " + reader["name"]);
                                    }
                                    break;
                            }
                        }
                    }
                }
                isValid = newIsValid;
            }
            catch (XmlException e)
            {
                throw new IOException(e.Message);
            }
            catch (XmlSchemaValidationException e)
            {
                throw new SpreadsheetReadException(e.Message);
            }
        }
        
        // Methods: 
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            if (name == null || !isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            name = name.ToUpper();
            if (Cells.ContainsKey(name))
            {
                return Cells[name].Contents;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            if (name == null || !isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            name = name.ToUpper();
            if (!Cells.ContainsKey(name))
            {
                return "";
            }

            if (Cells[name].Value != null)
            {
                return Cells[name].Value;
            }

            object result = GetCellContents(name);
            if (result.GetType().Equals(typeof(double)))
            {
                Cells[name].Value = result;
                return result;
            }
            if (result.GetType().Equals(new Formula().GetType()))
            {
                try
                {
                    result = ((Formula)result).Evaluate(s => double.Parse(GetCellValue(s).ToString()));
                    Cells[name].Value = result;
                    return result;
                }
                catch (FormulaEvaluationException)
                {
                    result = new FormulaError("The formula could not be evaluated");
                }
                catch (FormatException)
                {
                    result = new FormulaError("The formula could not be evaluated");
                }
            }
            if (result.Equals(""))
            {
                result = new FormulaError("The formula is referencing an empty cell");
            }
            Cells[name].Value = result;
            return result;
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> result = new List<string>();
            foreach (string s in Cells.Keys)
            {
                if (!Cells[s].Contents.Equals(""))
                {
                    result.Add(s);
                }
            }
            result.Sort();
            return result;
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to dest using an XML format.
        /// The XML elements should be structured as follows:
        ///
        /// <spreadsheet IsValid="IsValid regex goes here">
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        ///   <cell name="cell name goes here" contents="cell contents go here"></cell>
        /// </spreadsheet>
        ///
        /// The value of the IsValid attribute should be IsValid.ToString()
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.
        /// If the cell contains a string, the string (without surrounding double quotes) should be written as the contents.
        /// If the cell contains a double d, d.ToString() should be written as the contents.
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        ///
        /// If there are any problems writing to dest, the method should throw an IOException.
        /// </summary>
        public override void Save(TextWriter dest)
        {
            try
            {
                List<string> NonEmptyCells = new List<string>(GetNamesOfAllNonemptyCells());
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "\t";
                using (XmlWriter writer = XmlWriter.Create(dest, settings))
                {

                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("IsValid", isValid.ToString());

                    foreach (string s in NonEmptyCells)
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteAttributeString("name", Cells[s].Name);
                        if (Cells[s].Contents.GetType().Equals(new Formula().GetType()))
                        {
                            writer.WriteAttributeString("contents", "=" + Cells[s].Contents.ToString());
                        }
                        else
                        {
                            writer.WriteAttributeString("contents", Cells[s].Contents.ToString());
                        }
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                dest.Close();
                dest.Dispose();
                changed = false;
            }
            catch (Exception e)
            {
                throw new IOException(e.Message);
            }
        }

        /// <summary>
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (name == null || !isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            name = name.ToUpper();
            Cell CurrentCell;
            object PreviousContents;
            if (Cells.ContainsKey(name))
            {
                CurrentCell = Cells[name];
                PreviousContents = CurrentCell.Contents;
                CurrentCell.Contents = formula;
                ClearDependents(name);
            }
            else
            {
                CurrentCell = new Cell(name, formula);
                Cells.Add(name, CurrentCell);
                CurrentCell.Value = new FormulaError("Referencing Empty Cell");
                PreviousContents = "";
            }
            foreach (string s in formula.GetVariables())
            {
                dependencies.AddDependency(name, s);
                if (HasCircularDependency(s, name))
                {
                    CurrentCell.Contents = PreviousContents;
                    if (PreviousContents.GetType().Equals(new Formula().GetType()))
                    {
                        foreach (string s2 in ((Formula)PreviousContents).GetVariables())
                        {
                            dependencies.AddDependency(name, s2);
                        }
                    }
                    throw new CircularException();
                }
                if (!Cells.ContainsKey(s))
                {
                    Cells.Add(s, new Cell(s.ToUpper()));
                }
            }
            HashSet<string> result = new HashSet<string>();
            result.Add(name);
            result.UnionWith(GetIndirectAndDirectDependents(name));
            return result;
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("Text cannot be null");
            }
            if (name == null || !isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            name = name.ToUpper();
            Cell CurrentCell;
            if (Cells.ContainsKey(name))
            {
                CurrentCell = Cells[name];
                CurrentCell.Contents = text;
                ClearDependents(name);
            }
            else
            {
                CurrentCell = new Cell(name, text);
                Cells.Add(name, CurrentCell);
            }
            HashSet<string> result = new HashSet<string>();
            result.Add(name);
            result.UnionWith(GetIndirectAndDirectDependents(name));
            return result;
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            if (name == null || !isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            name = name.ToUpper();
            Cell CurrentCell;
            if (Cells.ContainsKey(name))
            {
                CurrentCell = Cells[name];
                CurrentCell.Contents = number;
                ClearDependents(name);
            }
            else
            {
                CurrentCell = new Cell(name, number);
                Cells.Add(name, CurrentCell);
            }
            HashSet<string> result = new HashSet<string>();
            result.Add(name);
            result.UnionWith(GetIndirectAndDirectDependents(name));
            return result;
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        ///
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        ///
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        ///
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor with s => s.ToUpper() as the normalizer and a validator that
        /// checks that s is a valid cell name as defined in the AbstractSpreadsheet
        /// class comment.  There are then three possibilities:
        ///
        ///   (1) If the remainder of content cannot be parsed into a Formula, a
        ///       Formulas.FormulaFormatException is thrown.
        ///
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///
        ///   (3) Otherwise, the contents of the named cell becomes f.
        ///
        /// Otherwise, the contents of the named cell becomes content.
        ///
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        ///
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("Content cannot be null");
            }
            if (name == null || !isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            name = name.ToUpper();
            HashSet<string> result;
            double number;
            if (double.TryParse(content, out number))
            {
                changed = true;
                result = new HashSet<string>(SetCellContents(name, number));
                Cells[name].Value = number;
                ResetValues(name);
                return result;
            }

            if (content.StartsWith("="))
            {
                try
                {
                    Formula f = new Formula(content.Substring(1), s => s.ToUpper(), v => isValid.IsMatch(v) && CellNamePattern.IsMatch(v));
                    changed = true;
                    result = new HashSet<string>(SetCellContents(name, f)); ;
                    Cells[name].Value = null;
                    ResetValues(name);
                    return result;
                }
                catch (FormulaFormatException)
                {
                    Console.Error.WriteLine("Cell Content started with \"=\", but was not a Function: \tCell Name: " + name + "\tContent: " + content);
                    throw;
                }
                catch (CircularException)
                {
                    Console.Error.WriteLine("Cell Content was a function that caused a CircularException: \tCell Name: " + name + "\tContent: " + content);
                    throw;
                }
            }
            changed = true;
            result = new HashSet<string>(SetCellContents(name, content));
            Cells[name].Value = content;
            ResetValues(name);
            return result;
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }
            if (!isValid.IsMatch(name) || !CellNamePattern.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            return dependencies.GetDependees(name);
        }

        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected IEnumerable<string> GetIndirectAndDirectDependents(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("Name cannot be null");
            }
            if (!isValid.IsMatch(name))
            {
                throw new InvalidNameException();
            }
            HashSet<string> result = new HashSet<string>(GetDirectDependents(name));
            HashSet<string> visited = new HashSet<string>();

            Queue<string> queue = new Queue<string>();
            queue.Enqueue(name);

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                result.Add(current);
                foreach (string s in dependencies.GetDependees(current))
                {
                    if (!visited.Contains(s))
                    {
                        queue.Enqueue(s);
                    }
                    visited.Add(s);
                }
            }

            return result;
        }

        /// <summary>
        /// Removes all dependees of a given cell name
        /// </summary>
        /// <param name="name"></param>
        private void ClearDependents(string name)
        {
            List<string> dependents = new List<string>(dependencies.GetDependents(name));
            for (int i = 0; i < dependents.Count; i++)
            {
                dependencies.RemoveDependency(name, dependents[i]);
            }
        }

        private void ResetValues(string name)
        {
            foreach (string s in GetCellsToRecalculate(name))
            {
                if (!s.Equals(name))
                    Cells[s].Value = null;
            }
        }

        /// <summary>
        /// Checks to see if a cell has a circular dependency
        /// </summary>
        /// <param name="dependee">The name of the dependee</param>
        /// <param name="name">The name of the cell</param>
        /// <returns>True if there is a circular dependency</returns>
        private bool HasCircularDependency(string dependee, string name)
        {
            HashSet<string> results = new HashSet<string>(GetIndirectAndDirectDependents(name));
            if (results.Contains(dependee) || dependee.Equals(name))
            {
                return true;
            }
            return false;
        }

    }
}
