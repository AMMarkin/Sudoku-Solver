using System.Collections.Generic;
using System.Linq;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class VirtualSingleChecker : TechChecker
    {
        public override TechType Type => TechType.VirtualSingle;

        protected override string Discription => "Виртуальная одиночка";

        private FieldScanner FieldScanner => new FieldScanner();

        private delegate int ParamGetter(Field.Cell cell);

        private enum Direction
        {
            Rows,
            Columns,
            FromRegion,
            ToRegion
        }

        protected override AnswerOfTech FindElimination(Field field)
        {
            AnswerOfTech answer;

            answer = FindRestrictionInUnit(field, Direction.ToRegion, Direction.Rows);
            if (answer != null)
                return answer;

            answer = FindRestrictionInUnit(field, Direction.ToRegion, Direction.Columns);
            if (answer != null)
                return answer;

            answer = FindRestrictionInUnit(field, Direction.FromRegion, Direction.Rows);
            if (answer != null)
                return answer;

            answer = FindRestrictionInUnit(field, Direction.FromRegion, Direction.Columns);
            if (answer != null)
                return answer;

            return null;
        }

        private AnswerOfTech FindRestrictionInUnit(Field field, Direction targetDirection, Direction techDirection)
        {
            var settings = TechTuning(targetDirection, techDirection, field);

            List<Field.Cell[]> groups = settings.groups;
            List<Field.Cell[]> targetGroups = settings.targetGroups;
            ParamGetter GetGroupParameter = settings.GetGroupParameter;
            ParamGetter GetTargetParameter = settings.GetTargetParameter;
            string groupName = settings.groupName;

            foreach (var groupOfCells in groups)
            {
                for (int digit = 0; digit < Field.Digits_Count; digit++)
                {
                    int counter = groupOfCells.Count((x) => x.candidates[digit] == true);

                    if (counter != 2 && counter != 3) continue;

                    int[] targetIndexes = groupOfCells
                        .Select((x, idx) => new { cell = x, index = idx })
                        .Where((x) => x.cell.candidates[digit] == true)
                        .Select((x) => x.index)
                        .ToArray();


                    //проверяю в одном ли они блоке
                    bool inOneUnit = true;
                    for (int i = 0; i < targetIndexes.Length - 1; i++)
                    {
                        if (GetTargetParameter(groupOfCells[targetIndexes[i]]) != GetTargetParameter(groupOfCells[targetIndexes[i + 1]]))
                        {
                            inOneUnit = false;
                            break;
                        }
                    }
                    if (inOneUnit == false) continue;

                    //проверка есть ли в найденном блоке исключения

                    bool impact = false;
                    foreach (Field.Cell target in targetGroups[GetTargetParameter(groupOfCells[targetIndexes[0]])])
                    {

                        if (GetGroupParameter(target) == GetGroupParameter(groupOfCells[0])) continue;

                        //нашлось исключение
                        if (target.candidates[digit] == true)
                        {
                            impact = true;
                            AddElimination(target.ind, digit);
                            AddRemovingMark(target.ind, digit);
                        }
                    }
                    //составляю ответ
                    if (impact == true)
                    {
                        for (int i = 0; i < targetIndexes.Length; i++)
                        {
                            AddClueMark(groupOfCells[targetIndexes[i]].ind, digit);
                        }

                        groupName += $"{GetGroupParameter(groupOfCells[0]) + 1}";

                        string cells = groupOfCells.
                            Where((x) => x.candidates[digit] == true).
                            Select((x) => $"({x.row + 1};{x.column + 1})").
                            Aggregate((x, y) => $"{x} - {y}");


                        return MakeAnswer($"{groupName}: {Discription}: {digit + 1} в {cells}");
                    }
                }
            }
            return null;
        }

        private (ParamGetter GetGroupParameter, ParamGetter GetTargetParameter, List<Field.Cell[]> groups, List<Field.Cell[]> targetGroups, string groupName)
            TechTuning(Direction targetDirection, Direction techDirection, Field field)
        {
            List<Field.Cell[]> groups = new List<Field.Cell[]>();
            List<Field.Cell[]> targetGroups = new List<Field.Cell[]>();
            ParamGetter GetGroupParameter = (x) => throw new System.NotImplementedException();
            ParamGetter GetTargetParameter = (x) => throw new System.NotImplementedException();
            string groupName = string.Empty;

            if (targetDirection == Direction.ToRegion)
            {
                if (techDirection == Direction.Rows)
                {
                    groups = FieldScanner.SplitFieldByRows(field);
                    GetGroupParameter = (x) => x.row;
                    groupName = "Строка ";
                }
                if (techDirection == Direction.Columns)
                {
                    groups = FieldScanner.SplitFieldByColums(field);
                    GetGroupParameter = (x) => x.column;
                    groupName = "Столбец ";
                }
                targetGroups = FieldScanner.SplitFieldByRegions(field);
                GetTargetParameter = (x) => x.region;
            }

            if (targetDirection == Direction.FromRegion)
            {
                groups = FieldScanner.SplitFieldByRegions(field);
                GetGroupParameter = (x) => x.region;
                groupName = "Регион ";
                if (techDirection == Direction.Rows)
                {
                    targetGroups = FieldScanner.SplitFieldByRows(field);
                    GetTargetParameter = (x) => x.row;
                }
                if (techDirection == Direction.Columns)
                {
                    targetGroups = FieldScanner.SplitFieldByColums(field);
                    GetTargetParameter = (x) => x.column;
                }
            }

            return (GetGroupParameter, GetTargetParameter, groups, targetGroups, groupName);
        }

    }

}
