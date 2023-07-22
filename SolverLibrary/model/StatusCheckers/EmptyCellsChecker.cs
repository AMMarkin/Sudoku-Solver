using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model
{
    internal class EmptyCellsChecker : FieldStatusChecker
    {
        public override AnswerOfTech Check(Field field)
        {
            AnswerOfTech answer = null;

            for(int row = 0; row < Field.Row_Count; row++)
            {
                for(int col = 0; col < Field.Column_Count; col++)
                {
                    if (field[row,col].remainingCandidates==0 && field[row, col].value < 0)
                    {
                        answer = new AnswerOfTech($"Ошибка! В ячейке ({row+1};{col+1}) не осталось кандидатов!");
                        return answer;
                    }
                }
            }

            return answer;
        }
    }
}
