using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SolverLibrary.model;

namespace SolverLibrary.Interfaces
{
    public interface IGridView
    {
        IController Controller { get; set; }
        Field Field { get; set; }


        //обновить поле
        void UpdateGrid(Field field);

        //подсветка исключений
        void HighlighteRemoved(List<int[]> clues, List<int[]> removed, List<int[]> ON, List<int[]> OFF);



    }
}
