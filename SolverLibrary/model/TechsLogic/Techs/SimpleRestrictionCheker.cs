using System.Collections.Generic;

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
            
            List<Field.Cell> clues = (new FieldScanner()).FindFilledCells(field);
            foreach (Field.Cell clue in clues)
            {
                foreach (Field.Cell seenByClue in clue.seenCell)
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
