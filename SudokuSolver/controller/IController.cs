using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver.controller
{
    public  interface IController
    {
        Field Field { get; set; }
        Grid Grid { get; set; }
        Solver Solver { get; set; }
        Loader Loader { get; set; }
        Constructor Constructor { get; set; }


        bool[] UsedTechs { get; set; }

        void Do(bool[] usedTechs);

        void Undo();

        void HighlightDigit(int digit);

        void HighlightLinks(int digit);

        void LoadFrom(string filename);

        void SaveToFile(string filename, string data);

        List<string> GetListSaved();

        void Delete(string[] filenames);

        void Restart();
    }
}
