using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Oodles
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GameForm game = new GameForm();

            Application.Idle += new EventHandler(game.OnApplicationIdle);
            Application.Run(game);
        }
    }
}