using SolverLibrary.model.field;
using System.Collections.Generic;


namespace SolverLibrary.model.TechsLogic.Techs
{
    internal abstract class GroupedTechChecker : TechChecker
    {
        protected delegate List<Cell[]> Splitter(Field field);
        protected abstract Splitter[] Splitters { get; }
        protected abstract string[] GroupDescriptions { get; }

        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer = null;
            List<Cell[]> groups;

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

        protected abstract AnswerOfTech FindEliminationInGroup(Cell[] group);
    }
}
