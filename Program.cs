using System;
using System.Windows.Forms;

namespace TricksGame
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}