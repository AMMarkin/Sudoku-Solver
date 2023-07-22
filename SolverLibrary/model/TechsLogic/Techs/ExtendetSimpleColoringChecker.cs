namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class ExtendetSimpleColoringChecker : SimpleColoringTechChecker
    {
        public override TechType Type => TechType.ExtendedSimpleColoring;

        protected override string Discription => "Extended Simple Coloring";

        protected override AnswerOfTech FindElimination(Field field)
        {
            return ExtendedSimpleColoring(field);
        }

        //Extended Simple Coloring
        //--------------------------------------------------------------------------------------------------------
        //сильные связи для всех цветов + ячейки с двумя кандидатам(сильная связь внутри ячейки)

        //TODO вынести создание цепи в отдельный метод
        private AnswerOfTech ExtendedSimpleColoring(Field field)
        {
            AnswerOfTech answer;

            int[] subChains;
            int subChainCounter;

            //добавляю все сильные связи
            CreateChain(field, 0, 1, 2, 3, 4, 5, 6, 7, 8);

            //добавляю в связи ячейки с двумя кандидатами
            AddBiValueToChain(field);

            //разделение по компонентам связности

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

            //поиск исключений

            //для всех компонент

            //раскрашиваем цепь в 2 цвета
            for (int i = 0; i < subChainCounter; i++)
            {

                //раскрашиваю
                SubChainColoring(i, subChains);

                //поиск исключений

                //проверка повторения цвета 
                answer = ChainLogicRepeatRule(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                //повторение цвета в ячейке
                answer = TwiceInCellRule();
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                answer = TwoColorsInCell(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                //кандидаты видимые из двух цветов
                for (int k = 0; k < 9; k++)
                {
                    answer = TwoColorsElsewhere(field, k);
                    if (answer != null)
                    {
                        ClearChainBySubChain(i, subChains);
                        answer.Message = "Extended Simple Coloring: " + answer.Message;
                        return answer;
                    }
                }
                //кандидаты делящие ячейку с одним цветом и видимые други
                answer = TwoColorsUnitCell(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                //цвет полностью исключающий ячейку
                answer = CellEmptiedByColor(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }

            }
            return null;
        }

        //цвет полностью исключающий одну ячейку
        //TODO вынести проверку в отдельный метод
        private AnswerOfTech CellEmptiedByColor(Field field)
        {
            AnswerOfTech answer;

            bool emptyed;
            bool contains;

            //для первого цвета
            //иду по всем ячейкам
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //если есть значение то пропускаем
                    if (field[i, j].value != -1)
                    {
                        continue;
                    }
                    //проверка нет ли текущей ячейки в цепи
                    contains = false;
                    foreach (int[] unit in ON)
                    {
                        if (9 * i + j == unit[0])
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains)
                    {
                        continue;
                    }
                    foreach (int[] unit in OFF)
                    {
                        if (9 * i + j == unit[0])
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains)
                    {
                        continue;
                    }

                    //если не закрашена то 
                    //иду по всем ее кандидатам

                    //для ON
                    emptyed = true;
                    for (int digit = 0; digit < 9; digit++)
                    {
                        if (!field[i, j].candidates[digit])
                        {
                            continue;
                        }
                        if (!SeenByColor(9 * i + j, digit, true))
                        {
                            emptyed = false;
                            break;
                        }
                    }
                    //если все кандидаты незакрашенной ячейки видны цветом
                    //то этот цвет исключается
                    if (emptyed)
                    {
                        foreach (int[] rem in ON)
                        {
                            AddElimination(rem[0], rem[1]);
                            AddRemovingMark(rem[0], rem[1]);
                        }
                        ON.Clear();
                        ON.AddRange(OFF);
                        OFF.Clear();

                        AddClueMark(9 * i + j);
                        answer = MakeAnswer($"все числа в ячейке ({(i + 1)};{(j + 1)}) видны цветом");
                        AddChainLists(answer);
                        return answer;

                    }

                    //для OFF
                    for (int digit = 0; digit < 9; digit++)
                    {
                        if (!field[i, j].candidates[digit])
                        {
                            continue;
                        }
                        if (!SeenByColor(9 * i + j, digit, false))
                        {
                            emptyed = false;
                            break;
                        }
                    }
                    //если все кандидаты незакрашенной ячейки видны цветом
                    //то этот цвет исключается
                    if (emptyed)
                    {
                        foreach (int[] rem in OFF)
                        {
                            AddElimination(rem[0], rem[1]);
                            AddRemovingMark(rem[0], rem[1]);
                        }

                        OFF.Clear();

                        AddClueMark(9 * i + j);

                        answer = MakeAnswer($"все числа в ячейке ({(i + 1)};{(j + 1)}) видны цветом");
                        AddChainLists(answer);
                        return answer;
                    }
                }
            }
            return null;
        }

        //виден ли кандидат в ячейке цветом
        //TODO заменить флаг на enum
        private bool SeenByColor(int ind, int k, bool OnOff)
        {
            //OnOff 
            //true  - ON
            //false - OFF

            bool res = false;
            if (OnOff)
            {
                foreach (int[] unit in ON)
                {
                    //если не тот кандидат пропускаем
                    if (unit[1] != k)
                    {
                        continue;
                    }
                    //если одна ячейка пропускаем
                    if (unit[0] == ind)
                    {
                        continue;
                    }
                    //если то же число, в другой ячейке из нужного цвета видит нужную ячейку, то возвращаем тру
                    if (IsSeen(ind, unit[0]))
                    {
                        return true;
                    }

                }
            }
            else
            {
                //тоже самое для другого цвета
                foreach (int[] unit in OFF)
                {
                    //если не тот кандидат пропускаем
                    if (unit[1] != k)
                    {
                        continue;
                    }
                    //если одна ячейка пропускаем
                    if (unit[0] == ind)
                    {
                        continue;
                    }
                    //если то же число, в другой ячейке из нужного цвета видит нужную ячейку, то возвращаем тру
                    if (IsSeen(ind, unit[0]))
                    {
                        return true;
                    }
                }
            }
            return res;
        }


        //проверка появления одного цвета дважды в одной ячейке
        //TODO вынести проверку в отдельный метод
        private AnswerOfTech TwiceInCellRule()
        {
            AnswerOfTech answer;

            
            //полным перебором смотрю нет ли повторений индексов
            foreach (int[] unit1 in ON)
            {

                foreach (int[] unit2 in ON)
                {
                    if (unit2.Equals(unit1))
                    {
                        continue;
                    }
                    //если нашли два цвета в одной ячейке
                    if (unit2[0] == unit1[0])
                    {
                        //Все ON удаляются
                        foreach (int[] rem in ON)
                        {
                            AddElimination(rem[0], rem[1]);
                            AddRemovingMark(rem[0], rem[1]);
                        }
                        //для красоты перекидываем оставшееся в ON
                        ON.Clear();
                        ON.AddRange(OFF);
                        OFF.Clear();

                        AddClueMark(unit2[0], unit2[1]);
                        AddClueMark(unit2[0], unit1[1]);


                        answer = MakeAnswer($"повторение цвета в ячейке ({(unit2[0] / 9 + 1)};{(unit2[0] % 9 + 1)})");
                        AddChainLists(answer);
                        return answer;
                    }
                }
            }

            //полным перебором смотрю нет ли повторений индексов
            foreach (int[] unit1 in OFF)
            {

                foreach (int[] unit2 in OFF)
                {
                    if (unit2.Equals(unit1))
                    {
                        continue;
                    }
                    //если нашли два цвета в одной ячейке
                    if (unit2[0] == unit1[0])
                    {

                        //Все ON удаляются
                        foreach (int[] rem in OFF)
                        {
                            AddElimination(rem[0], rem[1]);
                            AddRemovingMark(rem[0], rem[1]);
                        }
                        //для красоты перекидываем оставшееся в ON
                        OFF.Clear();

                        AddClueMark(unit2[0], unit2[1]);
                        AddClueMark(unit2[0], unit1[1]);


                        answer = new AnswerOfTech($"повторение цвета в ячейке ({(unit2[0] / 9 + 1)};{(unit2[0] % 9 + 1)})");
                        SetLists(answer);
                        return answer;
                    }
                }
            }
            return null;
        }

        //проверка появления двух цветов в одной ячейке
        private AnswerOfTech TwoColorsInCell(Field field)
        {
            AnswerOfTech answer;

            int i, j;
            bool impact = false;

            //иду по первому цвету
            foreach (int[] unit1 in ON)
            {
                //ищу такой же индекс во втором цвете 
                foreach (int[] unit2 in OFF)
                {
                    //если нахожу то исключаю из ячейки все кроме этих двух цветов
                    if (unit1[0] == unit2[0])
                    {
                        i = unit1[0] / 9;
                        j = unit1[0] % 9;

                        for (int digit = 0; digit < 9; digit++)
                        {
                            if (digit != unit1[1] && digit != unit2[1] && field[i, j].candidates[digit])
                            {
                                AddElimination(unit1[0], digit);
                                AddRemovingMark(unit1[0], digit);
                                impact = true;
                            }
                        }
                        if (impact)
                        {
                            AddClueMark(unit1[0]);

                            answer = MakeAnswer($"два цвета в ячейке ({(i + 1)};{(j + 1)})");
                            AddChainLists(answer);
                            return answer;
                        }
                    }
                }
            }
            return null;
        }

        //исключение кандидатов видимых одним цветом и находящихся в одной ячейке с другим

        //TODO вынести проверку в отдельный метод
        private AnswerOfTech TwoColorsUnitCell(Field field)
        {
            AnswerOfTech answer;
            //исключаю кандидатов которые без цвета
            //но видят один цвет 
            //а второй в их ячейке
            //потому что если он будет решением, то сломает один цвет потому что займет ячейку
            //второй цвет потому что исключит звено в цепи

            int i1, j1;
            int i2, j2;
            //обхожу весь первый цвет
            foreach (int[] unit1 in ON)
            {
                i1 = unit1[0] / 9;
                j1 = unit1[0] % 9;
                //по всем кандидатам
                for (int digit = 0; digit < Field.Digits_Count; digit++)
                {
                    //если кандидат есть и не закрашен
                    if (digit != unit1[1] && field[i1, j1].candidates[digit])
                    {
                        //ищем ему пару во втором цвете
                        foreach (int[] unit2 in OFF)
                        {
                            //если не тот кандидат 
                            //пропускаем
                            if (unit2[1] != digit)
                            {
                                continue;
                            }

                            if (unit2[0] == unit1[0]) //не уверен что такое возможно но лучше ифануть
                            {
                                continue;
                            }
                            //если не видит 
                            //пропускаем
                            if (!IsSeen(unit1[0], unit2[0]))
                            {
                                continue;
                            }

                            //если не пропустили то исключаем и составляем ответ
                            i2 = unit2[0] / 9;
                            j2 = unit2[0] % 9;

                            AddElimination(unit1[0], digit);
                            AddRemovingMark(unit1[0], digit);

                            answer = MakeAnswer($"{(digit + 1)} в ячейке ({(i1 + 1)};{(j1 + 1)}) видит один цвет и свою пару в ячейке ({(i2 + 1)};{(j2 + 1)})");
                            AddChainLists(answer);
                            return answer;
                        }
                    }
                }
            }

            //то же самое для второго цвета
            foreach (int[] unit1 in OFF)
            {
                i1 = unit1[0] / 9;
                j1 = unit1[0] % 9;
                //по всем кандидатам
                for (int digit = 0; digit < 9; digit++)
                {
                    //если кандидат есть и не закрашен
                    if (digit != unit1[1] && field[i1, j1].candidates[digit])
                    {
                        //ищем ему пару во втором цвете
                        foreach (int[] unit2 in ON)
                        {
                            //если не тот кандидат 
                            //пропускаем
                            if (unit2[1] != digit)
                            {
                                continue;
                            }

                            if (unit2[0] == unit1[0]) //не уверен что такое возможно но лучше ифануть
                            {
                                continue;
                            }
                            //если не видит 
                            //пропускаем
                            if (!IsSeen(unit1[0], unit2[0]))
                            {
                                continue;
                            }

                            //если не пропустили то исключаем и составляем ответ
                            i2 = unit2[0] / 9;
                            j2 = unit2[0] % 9;


                            AddElimination(unit1[0], digit);
                            AddRemovingMark(unit1[0], digit);

                            answer = MakeAnswer($"{(digit + 1)} в ячейке ({(i1 + 1)};{(j1 + 1)}) видит один цвет и свою пару в ячейке ({(i2 + 1)};{(j2 + 1)})");
                            AddChainLists(answer);
                            return answer;
                        }
                    }
                }
            }


            return null;
        }

        //проверка видят ли ячейки друг друга
        private bool IsSeen(int ind1, int ind2)
        {
            bool res = false;
            int i1, i2;
            int j1, j2;

            i1 = ind1 / 9;
            j1 = ind1 % 9;

            i2 = ind2 / 9;
            j2 = ind2 % 9;

            //видят по строке
            if (i1 == i2)
            {
                res = true;
            }
            //видят по столбцу
            if (j1 == j2)
            {
                res = true;
            }
            //видят по региону
            if ((3 * (i1 / 3) + j1 / 3) == (3 * (i2 / 3) + j2 / 3))
            {
                res = true;
            }

            return res;
        }

        //добавление в цепь сильных связей ячеек с двумя кандидатами
        private void AddBiValueToChain(Field field)
        {
            int counter = 0;
            int a;
            int b;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //если значение не известно
                    if (field[i, j].value == -1)
                    {
                        //ищу ровно два кандидата
                        if (field[i, j].remainingCandidates != 2) continue;

                        //записываю кандидатов
                        a = -1;
                        b = -1;
                        for (int k = 0; k < 9; k++)
                        {
                            if (field[i, j].candidates[k])
                            {
                                counter++;
                                //записываю кандидатов
                                if (a == -1)
                                {
                                    a = k;
                                }
                                else
                                {
                                    b = k;
                                }
                            }
                        }

                        //добавляю в цепь сильную связь

                        AddLinkToChain(9 * i + j, 9 * i + j, a, b);
                        AddUnitToChain(9 * i + j, a);
                        AddUnitToChain(9 * i + j, b);

                    }
                }
            }
        }

    }
}
