using System;
using System.Collections.Generic;
using System.Text;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class NakedSingleCheker : TechChecker
    {
        public override TechType Type => TechType.NakedSingle;

        protected override string Discription => "Открытые одиночки: ";


        protected override AnswerOfTech FindElimination(Field field)
        {
            List<Field.Cell> list = (new FieldScanner()).FindXValueCells(field, 1);

            if (list.Count == 0) return null;

            int value;

            StringBuilder stringBuilder = new StringBuilder().AppendLine(Discription);

            foreach (Field.Cell cell in list)
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
