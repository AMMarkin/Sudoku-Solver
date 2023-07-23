using System.Collections.Generic;
using SolverLibrary.model.field;


namespace SolverLibrary.model.TechsLogic
{
    internal class SimpleRestrictionCheker : TechChecker
    {
        public override TechType Type => TechType.SimpleRestriction;

        protected override string Discription => "Найдены простые исключения";

        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer = null;

            bool impact = false;
            
            List<Cell> clues = (new FieldScanner()).FindFilledCells(field);
            foreach (Cell clue in clues)
            {
                foreach (Cell seenByClue in clue.seenCell)
                {
                    if (seenByClue.candidates[clue.value])
                    {
                        impact = true;
                        AddElimination(seenByClue.ind, clue.value);
                    }
                }
            }

            if (impact)
            {
                answer = MakeAnswer(Discription);
            }

            return answer;
        }







    }
}
