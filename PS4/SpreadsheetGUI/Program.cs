using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

//Authors: Yiliang Shi and Jet Vellinga
//Date: November 4, 2015
//PS6: Spreadsheet GUI
//References spreadsheet

namespace SpreadsheetGUI
{
    class SpreadsheetApplicationContext : ApplicationContext
    {

        private int spreadsheetCount = 0;

        private static SpreadsheetApplicationContext appContext;

        private SpreadsheetApplicationContext()
        {
        }

        public static SpreadsheetApplicationContext getAppContext()
        {
            if (appContext == null)
            {
                appContext = new SpreadsheetApplicationContext();
            }

            return appContext;
        }

        public void RunForm(Form form)
        {

            spreadsheetCount++;


            form.FormClosed += (o, e) => { if (--spreadsheetCount <= 0) ExitThread(); };
            
            form.Show();
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            View view = new View();
           
            SpreadsheetApplicationContext appContext = SpreadsheetApplicationContext.getAppContext();
            appContext.RunForm(view);
            Application.Run(appContext);
           
        }
    }
}
