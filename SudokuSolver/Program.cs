using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SudokuSolver.controller;
using SolverLibrary.model;

namespace SudokuSolver
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            
            Logic logic = new Logic();
            logic.Init();


            WFController controller = new WFController(logic);
            
            Application.Run(new Solver(controller,logic.tecniques.ToArray()));
        }
    }
}
