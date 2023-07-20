using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model.TechsLogic
{
    internal class FieldScanner
    {

        public List<Field.Cell> FindXValueCells(Field field, int count)
        {
            List<Field.Cell> result = new List<Field.Cell>();


            for(int i = 0; i < Field.Row_Count; i++)
            {
                for(int j = 0; j < Field.Column_Count; j++)
                {
                    if (field[i,j].remainingCandidates == count)
                    {
                        result.Add(field[i,j]);
                    }
                }
            }

            return result;
        }

        
        public List<Field.Cell> FindFilledCells(Field field)
        {
            List<Field.Cell> result = new List<Field.Cell>();

            
            for(int row = 0; row < Field.Row_Count; row++)
            {
                for(int col = 0; col < Field.Column_Count; col++)
                {
                    if (field[row, col].remainingCandidates == 0)
                    {
                        result.Add(field[row, col]);
                    }
                }
            }

            return result;
        }

        
    }
}
