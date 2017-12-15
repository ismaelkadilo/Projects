// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

// Implementation by Nathan Milot, u1063587 and Ismael Kadilo Wa Ngoie, u1120347

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace SpreadsheetGUI {

    [ExcludeFromCodeCoverage]
    static class Launch {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Get the application context and run one form inside it
            var context = ViewApplicationContext.GetContext();
            ViewApplicationContext.GetContext().RunNew();
            Application.Run(context);
        }
    }
}
