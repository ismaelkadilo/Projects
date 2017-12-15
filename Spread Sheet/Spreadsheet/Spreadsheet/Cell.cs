// Implementation by Nathan Milot, u1063587

using System.Diagnostics.CodeAnalysis;

namespace SS {
    /// <summary>
    /// Representation of a single cell in a spreadsheet
    /// </summary>
    [ExcludeFromCodeCoverage]
    class Cell {
        
        /// <summary>
        /// Private variable to store the name value
        /// </summary>
        private string name;

        /// <summary>
        /// The name of the cell
        /// </summary>
        public string Name {
            get {
                return name;
            }
            private set {
            }
        }

        /// <summary>
        /// The literal contents of the cell
        /// i.e. the formula, text, or string
        /// </summary>
        public object Contents { get; set; }
        
        /// <summary>
        /// The value of the cell
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Constructs a new Cell object and sets its name
        /// </summary>
        /// <param name="name"></param>
        public Cell(string name) {
            this.name = name;
            Contents = "";
        }

        /// <summary>
        /// Constructs a new Cell object and sets its name and contents
        /// </summary>
        /// <param name="name"></param>
        /// <param name="contents"></param>
        public Cell(string name, object contents) : this(name) {
            Contents = contents;
        }

        /// <summary>
        /// Returns the name of the cell
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Name;
        }

    }
}
