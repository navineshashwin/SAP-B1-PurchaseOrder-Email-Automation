using System;
using System.Windows.Forms;

namespace GK_DeliverySchedule
{
    static class Program
    {
        /// <summary>
        /// SAP Business One Add-on entry point
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Initialize Add-on
                APP.Root.Instance;

                // Start message loop
                Application.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to start SAP Add-on.\n\n" + ex.Message,
                    "Add-on Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

