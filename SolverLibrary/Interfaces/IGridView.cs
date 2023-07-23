using SolverLibrary.model;
using System.Collections.Generic;
using SolverLibrary.model.field;


namespace SolverLibrary.Interfaces
{
    public interface IGridView
    {
        IController Controller { get; set; }
        Field Field { get; set; }


        //обновить поле
        void UpdateGrid(Field field);

        //подсветка исключений
        void HighlighteRemoved(IEnumerable<Mark> clues, IEnumerable<Mark> removed, IEnumerable<int[]> ON, IEnumerable<int[]> OFF);



    }
}
