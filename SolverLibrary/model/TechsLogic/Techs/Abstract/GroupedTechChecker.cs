using System.Collections.Generic;
using System.Linq;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal abstract class GroupedTechChecker : TechChecker
    {
        protected delegate List<Field.Cell[]> Splitter(Field field);
        protected abstract Splitter[] Splitters { get; }
        protected abstract string[] GroupDescriptions { get; }

        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer = null;
            List<Field.Cell[]> groups;

            for (int splitCounter = 0; splitCounter < Splitters.Length; splitCounter++)
            {
                groups = Splitters[splitCounter](field);

                for (int groupCounter = 0; groupCounter < 9; groupCounter++)
                {
                    answer = FindEliminationInGroup(groups[groupCounter]);
                    if (answer != null)
                    {
                        answer.Message = $"{GroupDescriptions[splitCounter]} {groupCounter + 1}: " + answer.Message;
                        return answer;
                    }
                }
            }
            return answer;
        }

        protected abstract AnswerOfTech FindEliminationInGroup(Field.Cell[] group);
    }
}
