using System;
using System.Collections.Generic;
using System.Text;
using SolverLibrary.model.field;


namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class NakedSingleCheker : TechChecker
    {
        public override TechType Type => TechType.NakedSingle;



        protected override AnswerOfTech FindElimination(Field field)
        {
            List<Cell> list = (new FieldScanner()).FindXValueCells(field, 1);

            if (list.Count == 0) return null;

            int value;

            StringBuilder stringBuilder = new StringBuilder().AppendLine(Discription);

            foreach (Cell cell in list)
            {
                value = Array.FindIndex(cell.candidates, (x) => x == true);

                AddSetingValue(cell.ind, value);
                AddClueMark(cell.ind, value);

                stringBuilder.AppendLine($"    ({cell.row + 1};{cell.column + 1}) => {value + 1}");
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return MakeAnswer(stringBuilder.ToString());
        }
    }
}
