using System;
using System.Windows.Forms;
using OrganizationCalendar.Forms;

namespace OrganizationCalendar
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles for better appearance
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Create and run the main calendar form
                var mainForm = new MainCalendarForm();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fatal error starting application:\n\n{ex.Message}", 
                    "Application Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}
