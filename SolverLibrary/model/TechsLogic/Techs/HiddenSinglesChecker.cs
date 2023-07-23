using System.Linq;
using SolverLibrary.model.field;


namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class HiddenSinglesChecker : GroupedTechChecker
    {
        public override TechType Type => TechType.HiddenSingle;


        protected override Splitter[] Splitters => new Splitter[3] 
        { 
            FieldScanner.SplitFieldByRows, 
            FieldScanner.SplitFieldByColums,
            FieldScanner.SplitFieldByRegions
        };

        protected override string[] GroupDescriptions => new string[3] { "Строка", "Столбец", "Регион" };

        private FieldScanner FieldScanner => new FieldScanner();

        protected override AnswerOfTech FindEliminationInGroup(Cell[] group)
        {
            AnswerOfTech answer = null;
            string message;
            for (int digit = 0; digit < Field.Digits_Count; digit++)
            {
                int candidatCounter = group.Count((x) => x.candidates[digit] == true);

                if (candidatCounter == 1)
                {
                    Cell cell = group.First((x) => x.candidates[digit] == true);
                    AddSetingValue(cell.ind, digit);
                    AddClueMark(cell.ind, digit);
                    for (int k = 0; k < Field.Digits_Count; k++)
                    {
                        if (cell.candidates[k] && k != digit)
                        {
                            AddElimination(cell.ind, k);
                        }
                    }

                    message = $"({cell.row + 1};{cell.column + 1}) => {digit + 1}";

                    answer = MakeAnswer($"{Discription}: {message}");
                    break;
                }
            }

            return answer;
        }
    }
}
