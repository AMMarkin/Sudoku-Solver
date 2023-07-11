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


            //создаю контроллер WINFORMS
            WFController controller = new WFController();
            //заполняю нужные массивы
            Logic.Init();
            //запускаю форму
            Application.Run(new Solver(controller,Logic.tecniques.ToArray()));
        }
    }
}
