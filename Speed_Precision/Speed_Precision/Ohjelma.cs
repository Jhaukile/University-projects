#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;

#endregion


/// @author haukjohe
/// @version 23.03.2023
namespace Speed_Precision
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var game = new Speed_Precision();
            game.Run();
        }
    }
}
