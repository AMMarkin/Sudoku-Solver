using SolverLibrary.model.TechsLogic.Techs.Abstract;
using System.Collections.Generic;
using SolverLibrary.model.field;



namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class SimpleColoringTechChecker : ChainTechChecker
    {
        public override TechType Type => TechType.SimpleColoring;


        protected delegate int GetTargetParameter(int x);

        
        
        public override AnswerOfTech CheckField(Field field)
        {
            ClearLists();
            ClearChainLists();
            return FindElimination(field);
        }

        protected override AnswerOfTech FindElimination(Field field)
        {
            return SimpleColoring(field);
        }

        //только по сильным связям
        private AnswerOfTech SimpleColoring(Field field)
        {
            AnswerOfTech answer;


            int[] subChains = null;
            int subChainCounter = 0;

            for (int k = 0; k < 9; k++)
            {
                CreateChain(field, k);

                //поиск исключений
                ON.Clear();
                OFF.Clear();

                //нахожу компоненты связности
                subChains = new int[chainUnits.Count];
                subChainCounter = 0;

                bool[] visited = new bool[chainUnits.Count];

                for (int v = 0; v < chainUnits.Count; v++)
                {
                    if (!visited[v])
                    {
                        DfsStrong(ref visited, ref subChains, ref subChainCounter, v, field);
                        subChainCounter++;
                    }
                }
                //для всех компонент
                for (int i = 0; i < subChainCounter; i++)
                {

                    //раскрашиваю
                    SubChainColoring(i, subChains);

                    //поиск исключений

                    //повторения цвета

                    answer = ChainLogicRepeatRule(new int[] { k });
                    if (answer != null)
                    {
                        ClearChainBySubChain(i, subChains);
                        answer.Message = "Simple Coloring: " + answer.Message;
                        return answer;
                    }

                    //ячейки видимые двумя цветами
                    answer = TwoColorsElsewhere(field, k);
                    if (answer != null)
                    {
                        ClearChainBySubChain(i, subChains);
                        answer.Message = "Simple Coloring: " + answer.Message;
                        return answer;
                    }

                }
            }

            return null;
        }



        //повторение цвета 
        protected AnswerOfTech ChainLogicRepeatRule(int[] digitArray)
        {
            AnswerOfTech answer;

            int getRowFromIndex(int x) => x / Field.Row_Count;
            int getColumnFromIndex(int x) => x % Field.Column_Count;
            int getReginFromIndex(int x) => 3 * (getRowFromIndex(x) / 3) + (getColumnFromIndex(x) / 3);


            //по всем числам
            foreach (int digit in digitArray)
            {
                //строки
                answer = FindRepeatInGroup(digit, getRowFromIndex, "строке");
                if (answer != null) return answer;
                //столбцы
                answer = FindRepeatInGroup(digit, getColumnFromIndex, "столбце");
                if (answer != null) return answer;

                //регионы
                answer = FindRepeatInGroup(digit, getReginFromIndex, "регионе");
                if (answer != null) return answer;
            }
            return null;
        }

        protected AnswerOfTech FindRepeatInGroup(int digit, GetTargetParameter GetParam, string groupName)
        {
            AnswerOfTech answer;
            //для ON
            int targetParam;
            bool[] flags = new bool[Field.Row_Count];

            bool repeat = false;
            targetParam = -1;
            repeat = FindRepeatInGroupInColor(digit, GetParam, ref targetParam, flags, Color.ON);
            
            if (repeat)
            {
                ON.Clear();
                ON.AddRange(OFF);
                OFF.Clear();

                answer = MakeAnswer($"повторение цвета в {groupName} {(targetParam + 1)}: {digit+1} в нескольких ячейках одного цвета");
                AddChainLists(answer);
                return answer;
            }

            //для OFF
            flags = new bool[9];
            targetParam = -1;
            repeat = FindRepeatInGroupInColor(digit, GetParam, ref targetParam, flags, Color.OFF);
            
            if (repeat)
            {
                OFF.Clear();

                answer = MakeAnswer($"повторение цвета в {groupName} {(targetParam + 1)}: {digit+1} в нескольких ячейках одного цвета");
                AddChainLists(answer);
                return answer;
            }
            return null;
        }

        private bool FindRepeatInGroupInColor(int digit, GetTargetParameter GetParam, ref int targetParam, bool[] flags, Color color)
        {
            bool repeat = false;
            List<int[]> chainOfColor;
            if(color == Color.ON)
            {
                chainOfColor = ON;
            }
            else
            {
                chainOfColor = OFF;
            }

            foreach (int[] unit in chainOfColor)
            {
                targetParam = GetParam(unit[0]);
                if (unit[1] != digit)
                {
                    continue;
                }
                if (!flags[targetParam])
                {
                    flags[targetParam] = true;
                }

                else
                {
                    repeat = true;
                    foreach (int[] rem in chainOfColor)
                    {
                        AddElimination(rem[0], rem[1]);
                        AddRemovingMark(rem[0], rem[1]);
                    }

                    foreach (int[] rem in chainOfColor)
                    {
                        if (GetParam(rem[0]) == targetParam && rem[1]==digit)
                        {
                            AddClueMark(rem[0]);
                        }
                    }

                    break;
                }
            }
            return repeat;
        }


        
        //исключение кандидатов видимых двумя цветами
        protected AnswerOfTech TwoColorsElsewhere(Field field, int k)
        {
            AnswerOfTech answer;

            List<int> seenbyON;
            List<int> seenbyOFF;
            List<int> intersec = new List<int>();

            int i, j;
            //находим все ячейки которые видны кандидатом k в группе ON
            seenbyON = FindCellsSeenByColor(field, k, Color.ON);
            //находим все ячейки которые видны кандидатом k в группе OFF
            seenbyOFF = FindCellsSeenByColor(field, k, Color.OFF);


            foreach(int ONCell in seenbyON)
            {
                if (seenbyOFF.Contains(ONCell))
                {
                    if (!intersec.Contains(ONCell))
                    {
                        intersec.Add(ONCell);
                    }
                }
            }


            bool impact = false;
            string message = $"{(k + 1)} в ячейках: ";
            foreach (int ind in intersec)
            {
                i = ind / 9;
                j = ind % 9;
                if (field[i, j].candidates[k])
                {
                    impact = true;

                    AddElimination(ind, k);
                    AddRemovingMark(ind, k);

                    message += $"({(i + 1)};{(j + 1)}) ";
                }
            }
            if (impact)
            {
                message += "видимы из обоих цветов";

                answer = MakeAnswer(message);
                AddChainLists(answer);
                return answer;
            }

            return null;
        }


        private List<int> FindCellsSeenByColor(Field field, int digit, Color color)
        {
            List<int> seenByColor = new List<int>();
            List<int[]> targetColor = color == Color.ON ? ON : OFF;
            int i, j;

            foreach (int[] unit in targetColor)
            {
                //если нашли нужное число
                if (unit[1] == digit)
                {
                    //находим нужную ячейку на поле
                    i = unit[0] / 9;
                    j = unit[0] % 9;
                    //записываем все видимые ей индексы (новые)
                    for (int n = 0; n < field[i, j].seenCell.Length; n++)
                    {
                        if (!seenByColor.Contains(field[i, j].seenCell[n].ind))
                        {
                            seenByColor.Add(field[i, j].seenCell[n].ind);
                        }
                    }

                }
            }
            return seenByColor;
        }
    }
}
