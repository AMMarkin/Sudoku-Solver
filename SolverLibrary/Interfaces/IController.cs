using SolverLibrary.model.field;
using System.Collections.Generic;


namespace SolverLibrary.Interfaces
{
    public interface IController
    {
        Field Field { get; set; }
        IGridView Grid { get; set; }
        ISolverView Solver { get; set; }
        ILoader Loader { get; set; }
        IConstructor Constructor { get; set; }


        bool[] UsedTechs { get; set; }

        void Do(bool[] usedTechs);

        void Undo();

        void HighlightDigit(int digit);

        void HighlightLinks(int digit);

        void LoadFrom(string filename);

        void SaveToFile(string name, int[][] puzzle);

        List<string> GetListSaved();

        void Delete(string[] filenames);

        void Restart();
    }
}
