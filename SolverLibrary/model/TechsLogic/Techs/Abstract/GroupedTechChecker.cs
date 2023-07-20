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

        protected List<Field.Cell[]> SplitFieldByRows(Field field)
        {
            List<Field.Cell[]> groups = new List<Field.Cell[]>();

            for (int row = 0; row < Field.Row_Count; row++)
            {
                Field.Cell[] group = new Field.Cell[Field.Column_Count];
                for (int col = 0; col < Field.Column_Count; col++)
                {
                    group[col] = field[row, col];
                }
                groups.Add(group);
            }

            return groups;
        }

        protected List<Field.Cell[]> SplitFieldByColums(Field field)
        {
            List<Field.Cell[]> groups = new List<Field.Cell[]>();

            for (int col = 0; col < Field.Column_Count; col++)
            {
                Field.Cell[] group = new Field.Cell[Field.Column_Count];
                for (int row = 0; row < Field.Row_Count; row++)
                {
                    group[row] = field[row, col];
                }
                groups.Add(group);
            }
            return groups;
        }

        protected List<Field.Cell[]> SplitFieldByRegions(Field field)
        {
            List<List<Field.Cell>> groups = new List<List<Field.Cell>>();

            for (int reg = 0; reg < Field.Regions_Count; reg++)
            {
                List<Field.Cell> group = new List<Field.Cell>();
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
