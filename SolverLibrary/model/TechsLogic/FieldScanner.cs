using SolverLibrary.model.field;
using System.Collections.Generic;
using System.Linq;


namespace SolverLibrary.model.TechsLogic
{
    internal class FieldScanner
    {

        public List<Cell> FindXValueCells(Field field, int count)
        {
            List<Cell> result = new List<Cell>();


            for (int i = 0; i < Field.Row_Count; i++)
            {
                for (int j = 0; j < Field.Column_Count; j++)
                {
                    if (field[i, j].remainingCandidates == count)
                    {
                        result.Add(field[i, j]);
                    }
                }
            }

            return result;
        }


        public List<Cell> FindFilledCells(Field field)
        {
            List<Cell> result = new List<Cell>();


            for (int row = 0; row < Field.Row_Count; row++)
            {
                for (int col = 0; col < Field.Column_Count; col++)
                {
                    if (field[row, col].remainingCandidates == 0)
                    {
                        result.Add(field[row, col]);
                    }
                }
            }

            return result;
        }


        internal List<Cell[]> SplitFieldByRows(Field field)
        {
            List<Cell[]> groups = new List<Cell[]>();

            for (int row = 0; row < Field.Row_Count; row++)
            {
                Cell[] group = new Cell[Field.Column_Count];
                for (int col = 0; col < Field.Column_Count; col++)
                {
                    group[col] = field[row, col];
                }
                groups.Add(group);
            }

            return groups;
        }

        internal List<Cell[]> SplitFieldByColums(Field field)
        {
            List<Cell[]> groups = new List<Cell[]>();

            for (int col = 0; col < Field.Column_Count; col++)
            {
                Cell[] group = new Cell[Field.Column_Count];
                for (int row = 0; row < Field.Row_Count; row++)
                {
                    group[row] = field[row, col];
                }
                groups.Add(group);
            }
            return groups;
        }

        internal List<Cell[]> SplitFieldByRegions(Field field)
        {
            List<List<Cell>> groups = new List<List<Cell>>();

            for (int reg = 0; reg < Field.Regions_Count; reg++)
            {
                List<Cell> group = new List<Cell>();
                groups.Add(group);
            }

            for (int col = 0; col < Field.Column_Count; col++)
            {
                for (int row = 0; row < Field.Row_Count; row++)
                {
                    groups[field[row, col].region].Add(field[row, col]);
                }
            }
            return groups.Select((x) => x.ToArray()).ToList();
        }

    }
}
