#nullable disable
using SpaceCraft_Mod_Tool;
using System;
using System.Windows.Forms;

namespace SpaceCraft_Mod_Tool
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LauncherForm());
        }
    }
}