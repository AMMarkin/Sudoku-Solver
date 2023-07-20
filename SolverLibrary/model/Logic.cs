using SolverLibrary.model.TechsLogic.Techs;
using System.Collections.Generic;

namespace SolverLibrary.model
{

    //класс с техниками
    //получает на вход объект класса Field содержащий поле, а так же список используемых техник
    //находит возможные исключения кандидатов или возможные установки значений
    //записывает найденные ходы в буффер поля

    public class Logic
    {

        public const string noFound = "Исключений не найдено";
        public const string done = "Судоку решено!";

        //массив и библиотека названий техник
        public readonly List<string> tecniques = TechsList.techNames;

        private readonly List<TechChecker> techCheckers = new List<TechChecker>();
        private readonly List<FieldStatusChecker> statusCheckers = new List<FieldStatusChecker>()
        {
            new RepeatedValuesChecker(),
            new EmptyCellsChecker(),
            new ComplitionChecker()
        };

        public Dictionary<string, int> tech = new Dictionary<string, int>();

        private readonly List<Change> seted = new List<Change>();
        private readonly List<Change> eliminations = new List<Change>();

        private readonly List<Mark> clues = new List<Mark>();
        private readonly List<Mark> removed = new List<Mark>();


        //заполениен библиотеки
        public void Init()
        {
            for (int i = 0; i < tecniques.Count; i++)
            {
                tech.Add(tecniques[i], i);
            }

            TechCheckerBuilder builder = new TechCheckerBuilder();

            for (int i = 0; i < tecniques.Count; i++)
            {
                TechChecker techChecker = builder.GetTechChecker((TechType)i);
                if (techChecker != null)
                {
                    techCheckers.Add(techChecker);
                }
            }
        }

        public AnswerOfTech FindElimination(Field field, bool[] tecFlags)
        {
            AnswerOfTech answer;


            //простые исключения
            TechChecker checker = techCheckers.Find((x) => x.Type == TechType.SimpleRestriction);
            answer = checker.CheckField(field);
            if (answer != null)
            {
                return answer;
            }

            //техники на классах
            for (int i = 0; i < tecFlags.Length; i++)
            {
                if (tecFlags[i] == false) continue;

                checker = techCheckers.Find((x) => x.Type == TechsList.ConvertNameToType(tecniques[i]));

                if (checker == null) continue;

                answer = checker.CheckField(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //не реализованное ищется в тупую
            answer = FindEliminationByOldMethod(field, tecFlags);

            return answer;
        }


        public AnswerOfTech FindEliminationByOldMethod(Field field, bool[] tecFlags)
        {
            AnswerOfTech answer;
            ClearLists();

            //виртуальные одиночки
            if (tecFlags[tech["Виртуальные одиночки"]])
            {
                answer = VirtualSingle(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //BUG
            if (tecFlags[tech["BUG"]])
            {
                answer = BUG(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //XYZ-Wing
            if (tecFlags[tech["XYZ-Wing"]])
            {
                answer = XYZ_Wing(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //Y-Wings
            if (tecFlags[tech["Y-Wings"]])
            {
                answer = Y_Wings(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            //Simple Coloring
            if (tecFlags[tech["Simple Coloring"]])
            {
                answer = SimpleColoring(field);
                if (answer != null)
                {
                    return answer;
                }
                else
                {
                    field.Buffer.ON.Clear();
                    field.Buffer.OFF.Clear();
                    field.Buffer.chain.Clear();
                }
            }

            //Extended Simple Coloring
            if (tecFlags[tech["Extended Simple Coloring"]])
            {
                answer = ExtendedSimpleColoring(field);
                if (answer != null)
                {
                    return answer;
                }
                else
                {
                    field.Buffer.ON.Clear();
                    field.Buffer.OFF.Clear();
                    field.Buffer.chain.Clear();
                }
            }

            //проверка решения
            foreach(FieldStatusChecker cheker in statusCheckers)
            {
                answer = cheker.Check(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            answer = new AnswerOfTech(noFound);

            return answer;
        }



        //Extended Simple Coloring
        //--------------------------------------------------------------------------------------------------------
        //сильные связи для всех цветов + ячейки с двумя кандидатам(сильная связь внутри ячейки)
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
            subChains = new int[field.Buffer.chainUnits.Count];
            subChainCounter = 0;

            bool[] visited = new bool[field.Buffer.chainUnits.Count];

            for (int v = 0; v < field.Buffer.chainUnits.Count; v++)
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
                SubChainColoring(i, subChains, field);

                //поиск исключений

                //проверка повторения цвета 
                answer = ChainLogicRepeatRule(field, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains, field);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                //повторение цвета в ячейке
                answer = TwiceInCellRule(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains, field);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                answer = TwoColorsInCell(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains, field);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                //кандидаты видимые из двух цветов
                for (int k = 0; k < 9; k++)
                {
                    answer = TwoColorsElsewhere(field, k);
                    if (answer != null)
                    {
                        ClearChainBySubChain(i, subChains, field);
                        answer.Message = "Extended Simple Coloring: " + answer.Message;
                        return answer;
                    }
                }
                //кандидаты делящие ячейку с одним цветом и видимые други
                answer = TwoColorsUnitCell(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains, field);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }
                //цвет полностью исключающий ячейку
                answer = CellEmptiedByColor(field);
                if (answer != null)
                {
                    ClearChainBySubChain(i, subChains, field);
                    answer.Message = "Extended Simple Coloring: " + answer.Message;
                    return answer;
                }

            }
            return null;
        }

        //цвет полностью исключающий одну ячейку
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
                    foreach (int[] unit in field.Buffer.ON)
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
                    foreach (int[] unit in field.Buffer.OFF)
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

                    //для field.Buffer.ON
                    emptyed = true;
                    for (int k = 0; k < 9; k++)
                    {
                        if (!field[i, j].candidates[k])
                        {
                            continue;
                        }
                        if (!SeenByColor(9 * i + j, k, true, field))
                        {
                            emptyed = false;
                            break;
                        }
                    }
                    //если все кандидаты незакрашенной ячейки видны цветом
                    //то этот цвет исключается
                    if (emptyed)
                    {
                        foreach (int[] rem in field.Buffer.ON)
                        {
                            eliminations.Add(new Change(rem[0], rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(rem[0], rem[1]));
                        }
                        field.Buffer.ON.Clear();
                        field.Buffer.ON.AddRange(field.Buffer.OFF);
                        field.Buffer.OFF.Clear();

                        clues.Add(new Mark(9 * i + j));

                        answer = new AnswerOfTech($"все числа в ячейке ({(i + 1)};{(j + 1)}) видны цветом");
                        SetLists(answer);
                        return answer;

                    }

                    //для field.Buffer.OFF
                    for (int k = 0; k < 9; k++)
                    {
                        if (!field[i, j].candidates[k])
                        {
                            continue;
                        }
                        if (!SeenByColor(9 * i + j, k, false, field))
                        {
                            emptyed = false;
                            break;
                        }
                    }
                    //если все кандидаты незакрашенной ячейки видны цветом
                    //то этот цвет исключается
                    if (emptyed)
                    {
                        foreach (int[] rem in field.Buffer.OFF)
                        {
                            eliminations.Add(new Change(rem[0], rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(rem[0], rem[1]));
                        }

                        field.Buffer.OFF.Clear();

                        clues.Add(new Mark(9 * i + j));

                        answer = new AnswerOfTech($"все числа в ячейке ({(i + 1)};{(j + 1)}) видны цветом");
                        SetLists(answer);
                        return answer;

                    }

                }
            }


            return null;
        }

        //виден ли кандидат в ячейке цветом
        private bool SeenByColor(int ind, int k, bool OnOff, Field field)
        {
            //OnOff 
            //true  - Buffer.ON
            //false - Buffer.OFF

            bool res = false;
            if (OnOff)
            {
                foreach (int[] unit in field.Buffer.ON)
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
                foreach (int[] unit in field.Buffer.OFF)
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

        //исключение кандидатов видимых одним цветом и находящихся в одной ячейке с другим
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
            foreach (int[] unit1 in field.Buffer.ON)
            {
                i1 = unit1[0] / 9;
                j1 = unit1[0] % 9;
                //по всем кандидатам
                for (int k = 0; k < 9; k++)
                {
                    //если кандидат есть и не закрашен
                    if (k != unit1[1] && field[i1, j1].candidates[k])
                    {
                        //ищем ему пару во втором цвете
                        foreach (int[] unit2 in field.Buffer.OFF)
                        {
                            //если не тот кандидат 
                            //пропускаем
                            if (unit2[1] != k)
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

                            eliminations.Add(new Change(i1, j1, k, ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, k));

                            answer = new AnswerOfTech($"{(k + 1)} в ячейке ({(i1 + 1)};{(j1 + 1)}) видит один цвет и свою пару в ячейке ({(i2 + 1)};{(j2 + 1)})");
                            SetLists(answer);
                            return answer;
                        }
                    }
                }
            }

            //то же самое для второго цвета
            foreach (int[] unit1 in field.Buffer.OFF)
            {
                i1 = unit1[0] / 9;
                j1 = unit1[0] % 9;
                //по всем кандидатам
                for (int k = 0; k < 9; k++)
                {
                    //если кандидат есть и не закрашен
                    if (k != unit1[1] && field[i1, j1].candidates[k])
                    {
                        //ищем ему пару во втором цвете
                        foreach (int[] unit2 in field.Buffer.ON)
                        {
                            //если не тот кандидат 
                            //пропускаем
                            if (unit2[1] != k)
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


                            eliminations.Add(new Change(i1, j1, k, ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, k));

                            answer = new AnswerOfTech($"{(k + 1)} в ячейке ({(i1 + 1)};{(j1 + 1)}) видит один цвет и свою пару в ячейке ({(i2 + 1)};{(j2 + 1)})");
                            SetLists(answer);
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

        //проверка появления двух цветов в одной ячейке
        private AnswerOfTech TwoColorsInCell(Field field)
        {
            AnswerOfTech answer;

            int i, j;
            bool impact = false;

            List<Mark> clues = new List<Mark>();
            List<Mark> removed = new List<Mark>();
            List<Change> eliminations = new List<Change>();

            //иду по первому цвету
            foreach (int[] unit1 in field.Buffer.ON)
            {
                //ищу такой же индекс во втором цвете 
                foreach (int[] unit2 in field.Buffer.OFF)
                {
                    //если нахожу то исключаю из ячейки все кроме этих двух цветов
                    if (unit1[0] == unit2[0])
                    {
                        i = unit1[0] / 9;
                        j = unit1[0] % 9;

                        for (int k = 0; k < 9; k++)
                        {
                            if (k != unit1[1] && k != unit2[1] && field[i, j].candidates[k])
                            {
                                eliminations.Add(new Change(i, j, k, ChangeType.RemovingDigit));
                                removed.Add(new Mark(9 * i + j, k));
                                impact = true;
                            }
                        }
                        if (impact)
                        {
                            clues.Add(new Mark(9 * i + j));

                            answer = new AnswerOfTech($"два цвета в ячейке ({(i + 1)};{(j + 1)})");
                            SetLists(answer);
                            return answer;
                        }
                    }
                }
            }
            return null;
        }

        //проверка появления одного цвета дважды в одной ячейке
        private AnswerOfTech TwiceInCellRule(Field field)
        {
            AnswerOfTech answer;

            //иду по первому цвету
            int i1, j1;

            //полным перебором смотрю нет ли повторений индексов
            foreach (int[] unit1 in field.Buffer.ON)
            {

                foreach (int[] unit2 in field.Buffer.ON)
                {
                    if (unit2.Equals(unit1))
                    {
                        continue;
                    }
                    //если нашли два цвета в одной ячейке
                    if (unit2[0] == unit1[0])
                    {

                        //Все field.Buffer.ON удаляются
                        foreach (int[] rem in field.Buffer.ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1]));
                        }
                        //для красоты перекидываем оставшееся в field.Buffer.ON
                        field.Buffer.ON.Clear();
                        field.Buffer.ON.AddRange(field.Buffer.OFF);
                        field.Buffer.OFF.Clear();

                        clues.Add(new Mark(unit2[0], unit2[1]));
                        clues.Add(new Mark(unit2[0], unit1[1]));

                        answer = new AnswerOfTech($"повторение цвета в ячейке ({(unit2[0] / 9 + 1)};{(unit2[0] % 9 + 1)})");
                        SetLists(answer);
                        return answer;
                    }
                }
            }

            //полным перебором смотрю нет ли повторений индексов
            foreach (int[] unit1 in field.Buffer.OFF)
            {

                foreach (int[] unit2 in field.Buffer.OFF)
                {
                    if (unit2.Equals(unit1))
                    {
                        continue;
                    }
                    //если нашли два цвета в одной ячейке
                    if (unit2[0] == unit1[0])
                    {

                        //Все field.Buffer.ON удаляются
                        foreach (int[] rem in field.Buffer.OFF)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1]));
                        }
                        //для красоты перекидываем оставшееся в field.Buffer.ON
                        field.Buffer.OFF.Clear();

                        clues.Add(new Mark(unit2[0], unit2[1]));
                        clues.Add(new Mark(unit2[0], unit1[1]));

                        answer = new AnswerOfTech($"повторение цвета в ячейке ({(unit2[0] / 9 + 1)};{(unit2[0] % 9 + 1)})");
                        SetLists(answer);
                        return answer;
                    }
                }
            }
            return null;
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

                        AddLinkToChain(9 * i + j, 9 * i + j, a, b, field);
                        AddUnitToChain(9 * i + j, a, field);
                        AddUnitToChain(9 * i + j, b, field);

                    }
                }
            }
        }

        //очистка цепи для комфортного отображения
        private void ClearChainBySubChain(int subchainNumber, int[] subchains, Field field)
        {
            int ind, k;
            List<int[]> rem = new List<int[]>();
            for (int i = 0; i < subchains.Length; i++)
            {
                //если относится к ненужной компоненте
                if (subchains[i] != subchainNumber)
                {
                    //запоминаем
                    ind = field.Buffer.chainUnits[i][0];
                    k = field.Buffer.chainUnits[i][1];

                    foreach (int[] link in field.Buffer.chain)
                    {
                        if ((link[0] == ind && link[1] == k) || (link[2] == ind && link[3] == k))
                        {
                            rem.Add(link);
                        }
                    }
                }
            }

            for (int i = 0; i < rem.Count; i++)
            {
                field.Buffer.chain.Remove(rem[i]);
            }

        }

        //Simple Coloring
        //--------------------------------------------------------------------------------------------------------
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

                //раскрашиваем цепь в 2 цвета

                //списки цветов
                field.Buffer.ON.Clear();
                field.Buffer.OFF.Clear();

                //нахожу компоненты связности
                subChains = new int[field.Buffer.chainUnits.Count];
                subChainCounter = 0;

                bool[] visited = new bool[field.Buffer.chainUnits.Count];

                for (int v = 0; v < field.Buffer.chainUnits.Count; v++)
                {
                    if (!visited[v])
                    {
                        DfsStrong(ref visited, ref subChains, ref subChainCounter, v, field);
                        subChainCounter++;
                    }
                }
                //для всех кусков
                for (int i = 0; i < subChainCounter; i++)
                {

                    //раскрашиваю
                    SubChainColoring(i, subChains, field);

                    //поиск исключений

                    //повторения цвета
                    //для одного числа

                    answer = ChainLogicRepeatRule(field, new int[] { k });
                    if (answer != null)
                    {
                        ClearChainBySubChain(i, subChains, field);
                        answer.Message = "Simple Coloring: " + answer.Message;
                        return answer;
                    }

                    //ячейки видимые двумя цветами
                    answer = TwoColorsElsewhere(field, k);
                    if (answer != null)
                    {
                        ClearChainBySubChain(i, subChains, field);
                        answer.Message = "Simple Coloring: " + answer.Message;
                        return answer;
                    }

                }
            }

            return null;
        }

        //исключение кандидатов видимых двумя цветами
        private AnswerOfTech TwoColorsElsewhere(Field field, int k)
        {
            AnswerOfTech answer;

            List<int> seenbyON = new List<int>();
            List<int> intersec = new List<int>();

            int i, j;
            int i1, j1;
            //находим все ячейки которые видны кандидатом k в группе Buffer.ON
            foreach (int[] unit in field.Buffer.ON)
            {
                //если нашли нужное число
                if (unit[1] == k)
                {
                    //находим нужную ячейку на поле
                    i = unit[0] / 9;
                    j = unit[0] % 9;
                    //записываем все видимые ей индексы (новые)
                    for (int n = 0; n < field[i, j].seenInd[0].Length; n++)
                    {
                        i1 = field[i, j].seenInd[0][n];
                        j1 = field[i, j].seenInd[1][n];
                        if (!seenbyON.Contains(9 * i1 + j1))
                        {
                            seenbyON.Add(9 * i1 + j1);
                        }
                    }

                }
            }

            //находим все ячейки которые видны кандидатом k в группе field.Buffer.OFF
            foreach (int[] unit in field.Buffer.OFF)
            {
                //если нашли нужное число
                if (unit[1] == k)
                {
                    //находим нужную ячейку на поле
                    i = unit[0] / 9;
                    j = unit[0] % 9;
                    //записываем все видимые ей индексы (новые)
                    for (int n = 0; n < field[i, j].seenInd[0].Length; n++)
                    {
                        i1 = field[i, j].seenInd[0][n];
                        j1 = field[i, j].seenInd[1][n];
                        if (seenbyON.Contains(9 * i1 + j1))
                        {
                            intersec.Add(9 * i1 + j1);
                        }
                    }

                }
            }
            bool impact = false;
            answer = new AnswerOfTech($"{(k + 1)} в ячейках: ");
            foreach (int ind in intersec)
            {
                i = ind / 9;
                j = ind % 9;
                if (field[i, j].candidates[k])
                {
                    impact = true;

                    eliminations.Add(new Change(i, j, k, ChangeType.RemovingDigit));
                    removed.Add(new Mark(9 * i + j, k));

                    answer.Message += $"({(i + 1)};{(j + 1)}) ";

                }
            }
            if (impact)
            {
                answer.Message += "видимы из обоих цветов";
                SetLists(answer);
                return answer;
            }


            return null;
        }

        //повторение цвета 
        private AnswerOfTech ChainLogicRepeatRule(Field field, int[] kArray)
        {
            AnswerOfTech answer;

            bool repeat = false;
            bool[] flags = new bool[9];

            int i1, j1;
            //по всем числам
            foreach (int k in kArray)
            {
                //в строках
                //------------------------------------------------------------------------------
                //для Buffer.ON
                int row;
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                row = -1;
                foreach (int[] unit in field.Buffer.ON)
                {
                    row = unit[0] / 9;

                    if (unit[1] != k)
                    {
                        continue;
                    }
                    if (!flags[row])
                    {
                        flags[row] = true;
                    }
                    else
                    {
                        repeat = true;
                        //Все field.Buffer.ON удаляются
                        foreach (int[] rem in field.Buffer.ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1]));
                        }
                        //для красоты перекидываем оставшееся в field.Buffer.ON
                        field.Buffer.ON.Clear();
                        field.Buffer.ON.AddRange(field.Buffer.OFF);
                        field.Buffer.OFF.Clear();
                        break;
                    }
                }
                if (repeat)
                {
                    answer = new AnswerOfTech($"повторение цвета в строке {(row + 1)}");
                    SetLists(answer);
                    return answer;
                }

                //для Buffer.OFF
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                row = -1;
                foreach (int[] unit in field.Buffer.OFF)
                {
                    row = unit[0] / 9;
                    if (unit[1] != k)
                    {
                        continue;
                    }
                    if (!flags[row])
                    {
                        flags[row] = true;
                    }

                    else
                    {
                        repeat = true;
                        //Все field.Buffer.ON удаляются
                        foreach (int[] rem in field.Buffer.OFF)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1] + 1));
                        }
                        field.Buffer.OFF.Clear();
                        break;
                    }
                }
                if (repeat)
                {
                    answer = new AnswerOfTech($"повторение цвета в строке {(row + 1)}");
                    SetLists(answer);
                    return answer;
                }
                //------------------------------------------------------------------------------
                //в столбцах
                int col;
                //для Buffer.ON
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                col = -1;
                foreach (int[] unit in field.Buffer.ON)
                {
                    col = unit[0] / 9;
                    if (unit[1] != k)
                    {
                        continue;
                    }
                    if (!flags[col])
                    {
                        flags[col] = true;
                    }
                    else
                    {
                        repeat = true;
                        //Все field.Buffer.ON исключаются
                        foreach (int[] rem in field.Buffer.ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1] + 1));
                        }
                        //для красоты перекидываем оставшееся в field.Buffer.ON
                        field.Buffer.ON.Clear();
                        field.Buffer.ON.AddRange(field.Buffer.OFF);
                        field.Buffer.OFF.Clear();
                        break;
                    }
                }
                if (repeat)
                {
                    answer = new AnswerOfTech($"повторение цвета в столбце {(col + 1)}");
                    SetLists(answer);
                    return answer;
                }

                //для Buffer.OFF
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                col = -1;
                foreach (int[] unit in field.Buffer.OFF)
                {
                    col = unit[0] / 9;
                    if (unit[1] != k)
                    {
                        continue;
                    }
                    if (!flags[col])
                    {
                        flags[col] = true;
                    }
                    else
                    {
                        repeat = true;
                        //Все field.Buffer.ON исключаются
                        foreach (int[] rem in field.Buffer.OFF)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1] + 1));
                        }
                        //для красоты перекидываем оставшееся в field.Buffer.ON
                        field.Buffer.OFF.Clear();
                        break;
                    }
                }
                //если повторение в столбцах то 
                if (repeat)
                {
                    answer = new AnswerOfTech($"повторение цвета в столбце {(col + 1)}");
                    SetLists(answer);
                    return answer;
                }
                //------------------------------------------------------------------------------
                //в регионах
                int reg;
                //для Buffer.ON
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                reg = -1;
                foreach (int[] unit in field.Buffer.ON)
                {
                    int x = unit[0] / 9;
                    int y = unit[0] % 9;
                    reg = 3 * (x / 3) + (y / 3);
                    if (unit[1] != k)
                    {
                        continue;
                    }
                    if (!flags[reg])
                    {
                        flags[reg] = true;
                    }
                    else
                    {
                        repeat = true;
                        //Все field.Buffer.ON исключаются
                        foreach (int[] rem in field.Buffer.ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;

                            eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                            removed.Add(new Mark(9 * i1 + j1, rem[1] + 1));
                        }
                        //для красоты перекидываем оставшееся в field.Buffer.ON
                        field.Buffer.ON.Clear();
                        field.Buffer.ON.AddRange(field.Buffer.OFF);
                        field.Buffer.OFF.Clear();
                        break;
                    }
                }
                if (repeat)
                {
                    answer = new AnswerOfTech($"повторение цвета в регионе {(reg + 1)}");
                    SetLists(answer);
                    return answer;
                }
                //для Buffer.OFF
                if (!repeat)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        flags[j] = false;
                    }
                    col = -1;
                    foreach (int[] unit in field.Buffer.OFF)
                    {
                        int x = unit[0] / 9;
                        int y = unit[0] % 9;
                        reg = 3 * (x / 3) + (y / 3);
                        if (unit[1] != k)
                        {
                            continue;
                        }
                        if (!flags[reg])
                        {
                            flags[reg] = true;
                        }
                        else
                        {
                            repeat = true;
                            //Все field.Buffer.OFF исключаются
                            foreach (int[] rem in field.Buffer.OFF)
                            {
                                i1 = rem[0] / 9;
                                j1 = rem[0] % 9;

                                eliminations.Add(new Change(i1, j1, rem[1], ChangeType.RemovingDigit));
                                removed.Add(new Mark(9 * i1 + j1, rem[1] + 1));
                            }
                            //для красоты перекидываем оставшееся в field.Buffer.ON
                            field.Buffer.OFF.Clear();
                            break;
                        }
                    }
                }

                //если повторение в регионе
                if (repeat)
                {
                    answer = new AnswerOfTech($"повторение цвета в регионе {(reg + 1)}");
                    SetLists(answer);
                    return answer;
                }
            }

            return null;
        }

        //раскраска
        private void SubChainColoring(int subChainNumber, int[] subchains, Field field)
        {
            //переписываю кусок в массив для удобной работы
            int counter = 0;
            for (int j = 0; j < subchains.Length; j++)
            {
                if (subchains[j] == subChainNumber)
                {
                    counter++;
                }
            }
            if (counter == 0)
            {
                return;
            }


            int[][] subChain = new int[counter][];
            counter = 0;
            for (int j = 0; j < subchains.Length; j++)
            {
                if (subchains[j] == subChainNumber)
                {
                    subChain[counter] = new int[3];
                    subChain[counter][0] = field.Buffer.chainUnits[j][0];    //ind
                    subChain[counter][1] = field.Buffer.chainUnits[j][1];    //k
                    subChain[counter][2] = 0;                   //color
                    counter++;
                }
            }

            //раскрашиваю начиная с Buffer.ON
            subChain[0][2] = 1;

            int[][] links = FindStrongLinksInChain(subChain[0][0], subChain[0][1], field);

            //Buffer.ON  - true
            //Buffer.OFF - false

            Coloring(ref subChain, links, false, field);



            field.Buffer.ON.Clear();
            field.Buffer.OFF.Clear();
            for (int j = 0; j < subChain.Length; j++)
            {
                if (subChain[j][2] == 1)
                {
                    field.Buffer.ON.Add(new int[] { subChain[j][0], subChain[j][1] });
                }
                if (subChain[j][2] == -1)
                {
                    field.Buffer.OFF.Add(new int[] { subChain[j][0], subChain[j][1] });
                }
            }
        }

        //рекурсивный метод для раскраски цепи
        private void Coloring(ref int[][] subChain, int[][] links, bool OnOff, Field field)
        {
            //для каждого ребра 
            for (int i = 0; i < links.Length; i++)
            {
                //ищем вторую вершину в цепи
                for (int j = 0; j < subChain.Length; j++)
                {
                    //если нашли нужную
                    //если не раскрашена
                    if (subChain[j][2] == 0 && subChain[j][0] == links[i][0] && subChain[j][1] == links[i][1])
                    {
                        subChain[j][2] = OnOff ? 1 : -1;
                        Coloring(ref subChain, FindStrongLinksInChain(subChain[j][0], subChain[j][1], field), !OnOff, field);
                        break;
                    }
                }

            }
        }

        //создание сильной цепи 
        public void CreateChain(Field field, params int[] kArray)
        {
            int[][] matrix = new int[9][];

            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int counter;
            int a;
            int b;

            if (field.Buffer.chain != null)
            {
                field.Buffer.chain.Clear();
            }
            else
            {
                field.Buffer.chain = new List<int[]>();
            }
            if (field.Buffer.weak != null)
            {
                field.Buffer.weak.Clear();
            }
            else
            {
                field.Buffer.weak = new List<int[]>();
            }
            if (field.Buffer.chainUnits != null)
            {
                field.Buffer.chainUnits.Clear();
            }
            else
            {
                field.Buffer.chainUnits = new List<int[]>();
            }
            //заполняем цепи для каждого числа отдельно
            foreach (int k in kArray)
            {

                //переписываю матрицу
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        matrix[i][j] = field[i, j].candidates[k] ? 1 : 0;

                    }
                }

                //создание цепи 
                //сильные связи
                //----------------------------------------------------------------------------------------------------------------------------
                //обхожу все строки
                for (int i = 0; i < 9; i++)
                {
                    counter = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        if (matrix[i][j] != 0)
                        {
                            counter++;
                        }
                    }

                    //если нашли сильную связь
                    //записываем в цепь
                    if (counter == 2)
                    {
                        a = -1;
                        b = -1;
                        //записал индексы
                        for (int j = 0; j < 9; j++)
                        {
                            if (matrix[i][j] != 0)
                            {
                                if (a < 0)
                                {
                                    a = 9 * i + j;
                                }
                                else
                                {
                                    b = 9 * i + j;
                                }
                            }
                        }

                        //добавил в цепь
                        AddLinkToChain(a, b, k, k, field);
                        AddUnitToChain(a, k, field);
                        AddUnitToChain(b, k, field);
                    }
                }

                //столбцы
                for (int j = 0; j < 9; j++)
                {

                    counter = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (matrix[i][j] != 0)
                        {
                            counter++;
                        }
                    }
                    //если нашел сильную связь
                    if (counter == 2)
                    {
                        //записал индексы
                        a = -1;
                        b = -1;
                        for (int i = 0; i < 9; i++)
                        {
                            if (matrix[i][j] != 0)
                            {
                                if (a < 0)
                                {
                                    a = i * 9 + j;
                                }
                                else
                                {
                                    b = i * 9 + j;
                                }
                            }
                        }
                        //добавил в цепь
                        AddLinkToChain(a, b, k, k, field);
                        AddUnitToChain(a, k, field);
                        AddUnitToChain(b, k, field);

                    }



                }

                //регионы
                for (int y = 0; y < 3; y++)
                {
                    for (int x = 0; x < 3; x++)
                    {

                        counter = 0;
                        //считаю кандидатов
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (matrix[3 * y + i][3 * x + j] != 0)
                                {
                                    counter++;
                                }

                            }
                        }

                        //если нашел сильную связь
                        if (counter == 2)
                        {
                            //записал индексы
                            a = -1;
                            b = -1;
                            for (int i = 0; i < 3; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    if (matrix[3 * y + i][3 * x + j] != 0)
                                    {
                                        if (a < 0)
                                        {
                                            a = 9 * (3 * y + i) + (3 * x + j);
                                        }
                                        else
                                        {
                                            b = 9 * (3 * y + i) + (3 * x + j);
                                        }

                                    }
                                }
                            }
                            //добавил в цепь
                            AddLinkToChain(a, b, k, k, field);
                            AddUnitToChain(a, k, field);
                            AddUnitToChain(b, k, field);
                        }
                    }
                }
                //----------------------------------------------------------------------------------------------------------------------------
                //слабые связи
                //fillWeakLinks();
            }
        }

        //найти все сильные связи в цепи
        private int[][] FindStrongLinksInChain(int ind, int k, Field field)
        {
            //считаю колличество связей
            int counter = 0;
            foreach (int[] link in field.Buffer.chain)
            {
                if ((link[0] == ind && link[1] == k))
                {
                    counter++;
                }
            }

            //записываю связи
            int[][] links = new int[counter][];

            //ind, k
            counter = 0;
            foreach (int[] link in field.Buffer.chain)
            {
                if ((link[0] == ind && link[1] == k))
                {
                    links[counter] = new int[2];
                    if (link[0] == ind && link[1] == k)
                    {
                        links[counter][0] = link[2];
                        links[counter][1] = link[3];
                    }
                    counter++;
                }
            }



            return links;
        }

        //заполнение слабых связей
        public void FillWeakLinks(Field field)
        {
            //полный перебор

            foreach (int[] unit1 in field.Buffer.chainUnits)
            {
                foreach (int[] unit2 in field.Buffer.chainUnits)
                {
                    if (!unit1.Equals(unit2))
                    {
                        bool linked = false;
                        foreach (int[] link in field.Buffer.chain)
                        {
                            //если сильно связаны то пропускаем
                            if ((link[0] == unit1[0] && link[1] == unit1[1] && link[2] == unit2[0] && link[3] == unit2[1])
                                ||
                                (link[0] == unit2[0] && link[1] == unit2[1] && link[2] == unit1[0] && link[3] == unit1[1]))
                            {
                                linked = true;
                                break;
                            }
                        }
                        //если не связаны сильно то
                        //проверяем одно ли число содержат
                        //проверяем видят ли они друг друга
                        if (!linked)
                        {
                            if ((unit1[1] == unit2[1]) && IsSeen(unit1[0], unit2[0]))
                            {
                                //добавляю слабую связь
                                field.Buffer.weak.Add(new int[] { unit1[0], unit1[1], unit2[0], unit2[1] });
                            }

                        }
                    }
                }
            }
        }

        //дфс по сильным и слабым связям
        private void DfsWeakStrong(ref bool[] visited, ref int[] component, ref int components, int v, Field field)
        {
            visited[v] = true;
            component[v] = components;
            foreach (int[] link in field.Buffer.chain)
            {
                //нашли ребро

                if (field.Buffer.chainUnits[v][0] == link[0] && field.Buffer.chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в field.Buffer.chainUnits
                    int count = 0;
                    foreach (int[] unit in field.Buffer.chainUnits)
                    {
                        if (unit[0] == link[2] && unit[1] == link[3])
                        {
                            if (!visited[count])
                            {
                                DfsWeakStrong(ref visited, ref component, ref components, count, field);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }

            foreach (int[] link in field.Buffer.weak)
            {
                //нашли ребро

                if (field.Buffer.chainUnits[v][0] == link[0] && field.Buffer.chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в field.Buffer.chainUnits
                    int count = 0;
                    foreach (int[] unit in field.Buffer.chainUnits)
                    {
                        if (unit[0] == link[2] && unit[1] == link[3])
                        {
                            if (!visited[count])
                            {
                                DfsWeakStrong(ref visited, ref component, ref components, count, field);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }
        }

        //дфс по сильным связям
        private void DfsStrong(ref bool[] visited, ref int[] component, ref int components, int v, Field field)
        {
            visited[v] = true;
            component[v] = components;
            foreach (int[] link in field.Buffer.chain)
            {
                //нашли ребро

                if (field.Buffer.chainUnits[v][0] == link[0] && field.Buffer.chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в field.Buffer.chainUnits
                    int count = 0;
                    foreach (int[] unit in field.Buffer.chainUnits)
                    {
                        if (unit[0] == link[2] && unit[1] == link[3])
                        {
                            if (!visited[count])
                            {
                                DfsStrong(ref visited, ref component, ref components, count, field);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }
        }

        //добавление в цепь новой связь
        private void AddLinkToChain(int ind1, int ind2, int k1, int k2, Field field)
        {
            bool contains = false;
            foreach (int[] item in field.Buffer.chain)
            {
                if (item[0] == ind1 && item[1] == k1 && item[2] == ind2 && item[3] == k2)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                field.Buffer.chain.Add(new int[] { ind1, k1, ind2, k2 });
            }
            contains = false;
            foreach (int[] item in field.Buffer.chain)
            {
                if (item[0] == ind2 && item[1] == k2 && item[2] == ind1 && item[3] == k1)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                field.Buffer.chain.Add(new int[] { ind2, k2, ind1, k1 });
            }
        }

        //добавление в цепь нового звена
        private void AddUnitToChain(int ind, int k, Field field)
        {
            bool contains = false;
            foreach (int[] unit in field.Buffer.chainUnits)
            {
                if (unit[0] == ind && unit[1] == k)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                field.Buffer.chainUnits.Add(new int[] { ind, k });
            }
        }
        //--------------------------------------------------------------------------------------------------------

        //XYZ-Wing
        private AnswerOfTech XYZ_Wing(Field field)
        {
            AnswerOfTech answer;

            //ищем все ячейки с тремя кандидатами

            int counter;
            int i1, j1; //индексы первого крыла
            int i2, j2; //индексы второго крыла

            int rem;    //исключаемое число

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //корень с тремя кандидатами
                    if (field[i, j].remainingCandidates != 3)
                    {
                        continue;
                    }

                    //по региону ищем крыло с двумя кандидатами
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            i1 = 3 * (i / 3) + y;
                            j1 = 3 * (j / 3) + x;

                            if (field[i1, j1].remainingCandidates != 2)
                            {
                                continue;
                            }

                            //должно быть два общих кандидата
                            counter = 0;
                            for (int k = 0; k < 9; k++)
                            {
                                if (field[i, j].candidates[k] && field[i1, j1].candidates[k])
                                {
                                    counter++;
                                }
                            }

                            if (counter != 2)
                            {
                                continue;
                            }

                            //ищем второе крыло по строке

                            for (int col = 0; col < 9; col++)
                            {
                                i2 = i;
                                j2 = col;
                                //пропускаем корень и возможно первое крыло
                                if (j2 == j || (i2 == i1 && j2 == j1))
                                {
                                    continue;
                                }
                                if (field[i2, j2].remainingCandidates != 2)
                                {
                                    continue;
                                }

                                //должно быть два общих кандидата с корнем
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i, j].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 2)
                                {
                                    continue;
                                }

                                //должен быть один общий кандидат с первым крылом
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i1, j1].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 1)
                                {
                                    continue;
                                }


                                //ищем исключаемое число
                                //единственное общее число у корня и двух крыльев
                                rem = -1;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i, j].candidates[k] && field[i1, j1].candidates[k] && field[i2, j2].candidates[k])
                                    {
                                        rem = k;
                                        break;
                                    }
                                }

                                //исключения производим в регионе корня, строке второго крыла

                                for (int r = 0; r < 3; r++)
                                {
                                    //пропускаем корень и возможно первое крыло
                                    if ((i1 == i && 3 * (j / 3) + r == j1) || 3 * (j / 3) + r == j || (i2 == i && 3 * (j / 3) + r == j2))
                                    {
                                        continue;
                                    }
                                    //если нашли ячейку видимую из всех трех позиций 
                                    //и она имеет нужный кандидат
                                    //исключаем его
                                    if (field[i, 3 * (j / 3) + r].candidates[rem])
                                    {
                                        eliminations.Add(new Change(i, 3 * (j / 3) + r, rem, ChangeType.RemovingDigit));
                                        removed.Add(new Mark(9 * i + 3 * (j / 3) + r, rem));

                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i + j, k));
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i1 + j1, k));
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i2 + j2, k));
                                            }
                                        }

                                        answer = new AnswerOfTech($"XYZ-Wing: исключена {(rem + 1)} из ({(i + 1)};{(3 * (j / 3) + r + 1)})");
                                        SetLists(answer);
                                        return answer;
                                    }
                                }
                            }


                            //ищем второе крыло по столбцу

                            for (int row = 0; row < 9; row++)
                            {
                                i2 = row;
                                j2 = j;
                                //пропускаем корень и возможно первое крыло
                                if (i2 == i || (i2 == i1 && j2 == j1))
                                {
                                    continue;
                                }
                                if (field[i2, j2].remainingCandidates != 2)
                                {
                                    continue;
                                }

                                //должно быть два общих кандидата с корнем
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i, j].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 2)
                                {
                                    continue;
                                }

                                //должен быть один общий кандидат с первым крылом
                                counter = 0;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i2, j2].candidates[k] && field[i1, j1].candidates[k])
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != 1)
                                {
                                    continue;
                                }


                                //ищем исключаемое число
                                //единственное общее число у корня и двух крыльев
                                rem = -1;
                                for (int k = 0; k < 9; k++)
                                {
                                    if (field[i, j].candidates[k] && field[i1, j1].candidates[k] && field[i2, j2].candidates[k])
                                    {
                                        rem = k;
                                        break;
                                    }
                                }


                                //исключения производим в регионе корня, столбце второго крыла

                                for (int r = 0; r < 3; r++)
                                {
                                    //пропускаем корень и возможно первое крыло
                                    if ((j1 == j && 3 * (i / 3) + r == i1) || 3 * (i / 3) + r == i || (3 * (i / 3) + r == i2 && j2 == j))
                                    {
                                        continue;
                                    }
                                    //если нашли ячейку видимую из всех трех позиций 
                                    //и она имеет нужный кандидат
                                    //исключаем его
                                    if (field[3 * (i / 3) + r, j].candidates[rem])
                                    {
                                        eliminations.Add(new Change(3 * (i / 3) + r, j, rem, ChangeType.RemovingDigit));
                                        removed.Add(new Mark(9 * (3 * (i / 3) + r) + j, rem));

                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i + j, k));
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i1 + j1, k));
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                clues.Add(new Mark(9 * i2 + j2, k));
                                            }
                                        }

                                        answer = new AnswerOfTech($"XYZ-Wing: исключена {(rem + 1)} из ({(3 * (i / 3) + r + 1)};{(j + 1)})");
                                        SetLists(answer);
                                        return answer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        //Y-Wings
        private AnswerOfTech Y_Wings(Field field)
        {
            AnswerOfTech answer;

            //ищем все ячейки с двумя кандидатами

            List<Field.Cell> list = new List<Field.Cell>();

            int counter;


            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].remainingCandidates == 2)
                    {
                        list.Add(field[i, j]);
                    }
                }
            }

            Field.Cell Y;
            Field.Cell X1;
            Field.Cell X2;

            int a;
            int b;
            int c;


            bool foundet = false;

            //встаем в точку 
            for (int i = 0; i < list.Count; i++)
            {
                if (foundet)
                    break;

                Y = list[i];

                a = -1;
                b = 0;
                c = 0;

                //записываем кандидатов в a и b
                for (int k = 0; k < 9; k++)
                {
                    if (Y.candidates[k])
                    {
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

                //ищем X1 
                //крыло у которого только a или только b

                bool flag = false;
                for (int j = 0; j < Y.seenCell.Length; j++)
                {
                    if (foundet)
                        break;

                    //Берем вторую точку
                    X1 = Y.seenCell[j];

                    if (X1.remainingCandidates != 2) continue;

                    //если 0 совпадений то flag=false  -- пропускаем
                    //если 1 совпадение то flag=true   -- то что нужно
                    //если 2 совпадения то flag=false  -- пропускаем

                    for (int k = 0; k < 9; k++)
                    {
                        if (X1.candidates[k] && (k == a || k == b))
                        {
                            flag = !flag;
                        }
                    }

                    if (flag)
                    {
                        //нужно чтобы совпало именно a
                        flag = false;
                        for (int k = 0; k < 9; k++)
                        {
                            if (X1.candidates[k] && k == a)
                            {
                                flag = true;
                            }
                        }
                        //если совпало не а -- пропускаем
                        if (!flag)
                        {
                            continue;
                        }

                        //запоминаем c
                        for (int k = 0; k < 9; k++)
                        {
                            if (X1.candidates[k] && k != a)
                            {
                                c = k;
                            }
                        }

                        //ищем второе "крыло"
                        for (int j2 = 0; j2 < Y.seenCell.Length; j2++)
                        {
                            if (j2 == j) continue;

                            X2 = Y.seenCell[j2];

                            if (X2.remainingCandidates != 2) continue;

                            counter = 0;

                            flag = false;
                            //считаем совпадения
                            for (int k = 0; k < 9; k++)
                            {
                                if (X2.candidates[k] && k == a)
                                {
                                    flag = true;
                                    break;
                                }
                                if (X2.candidates[k] && k == b)
                                {
                                    counter++;
                                }
                                if (X2.candidates[k] && k == c)
                                {
                                    counter++;
                                }
                            }

                            if (counter != 2)
                                flag = true;

                            if (flag)
                                continue;

                            //если совпало и b и с
                            //то ищем ячейки которая видна из X1 и X2

                            for (int n = 0; n < X1.seenCell.Length; n++)
                            {
                                for (int m = 0; m < X2.seenCell.Length; m++)
                                {
                                    //если нашли ячейку видимую из двух крыльев
                                    //ищем есть ли в ней c
                                    if (X1.seenCell[n].Equals(X2.seenCell[m]))
                                    {
                                        bool flagC = false;
                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (X1.seenCell[n].candidates[k] && k == c)
                                            {
                                                flagC = true;
                                            }
                                        }
                                        //если найдено исключение
                                        //исключаем
                                        //записываем ключи и исключения

                                        if (flagC)
                                        {
                                            foundet = true;

                                            eliminations.Add(new Change(X1.seenCell[n].ind, c, ChangeType.RemovingDigit));
                                            removed.Add(new Mark(X1.seenCell[n].ind, c));
                                        }

                                    }
                                }
                            }

                            if (foundet)
                            {
                                clues.Add(new Mark(Y.ind, a));
                                clues.Add(new Mark(Y.ind, b));
                                clues.Add(new Mark(X1.ind, a));
                                clues.Add(new Mark(X1.ind, c));
                                clues.Add(new Mark(X2.ind, b));
                                clues.Add(new Mark(X2.ind, c));

                                answer = new AnswerOfTech($"Y-Wings по {(c + 1)} : ({(Y.row + 1)};{(Y.column + 1)}) => ({(X1.row + 1)};{(X1.column + 1)}) - ({(X2.row + 1)};{(X2.column + 1)})");
                                SetLists(answer);
                                return answer;

                            }

                        }

                    }

                }
            }

            return null;
        }

        //BUG
        private AnswerOfTech BUG(Field field)
        {
            AnswerOfTech answer;

            int counter = 0;

            int bugI = -1;
            int bugJ = -1;

            //все ячейки кроме одной должны содержать 2 кандидата
            //одна должна содержать 3

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].value != -1) continue;

                    if (field[i, j].remainingCandidates != 2 && field[i, j].remainingCandidates != 3)
                    {
                        return null;
                    }
                    else if (field[i, j].remainingCandidates == 3)
                    {
                        counter++;
                        bugI = i;
                        bugJ = j;
                    }
                }
            }

            //если 3 кандидата более чем в одной ячейке то бага нет
            if (counter != 1)
            {
                return null;
            }

            int[] candidats_counter = new int[9];

            int startX = 3 * (bugJ / 3);
            int startY = 3 * (bugI / 3);

            //обхожу регион с жуком
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    //считаю колличество всех кандидатов
                    for (int k = 0; k < 9; k++)
                    {
                        if (field[startY + y, startX + x].candidates[k])
                        {
                            candidats_counter[k]++;
                        }
                    }
                }
            }

            int bugDigit = -1;
            counter = 0;
            for (int k = 0; k < 9; k++)
            {
                if (candidats_counter[k] != 2 && candidats_counter[k] != 3 && candidats_counter[k] != 0)
                {
                    return new AnswerOfTech("BUG: ОШИБКА ПОДСЧЕТА КАНДИДАТОВ");
                }
                if (candidats_counter[k] == 3)
                {
                    counter++;
                    bugDigit = k;
                }
            }

            if (counter != 1)
            {
                return new AnswerOfTech("BUG: ОШИБКА");
            }

            clues.Add(new Mark(9 * bugI + bugJ, bugDigit));

            for (int k = 0; k < 9; k++)
            {
                if (k == bugDigit) continue;

                if (field[bugI, bugJ].candidates[k])
                {
                    removed.Add(new Mark(9 * bugI + bugJ, k));
                }
            }
            //устанавливаю значение
            seted.Add(new Change(bugI, bugJ, bugDigit, ChangeType.SettingValue));
            for (int k = 0; k < 9; k++)
            {
                //обнуляю всех оставшихся кандидатов
                if (field[bugI, bugJ].candidates[k])
                {
                    eliminations.Add(new Change(bugI, bugJ, k, ChangeType.RemovingDigit));
                }
            }

            answer = new AnswerOfTech($"BUG: если в ячейке ({(bugI + 1)};{(bugJ + 1)}) установить не {(bugDigit + 1)}, то судоку будет иметь 2 решения");
            SetLists(answer);
            return answer;
        }

        //виртуальные одиночки
        private AnswerOfTech VirtualSingle(Field field)
        {
            AnswerOfTech answer = null;
            /*строки
             * по виртуальной одиночке в строке можно исключить только кандидатов в регионе
             * обходим все строки
             * смотрим все числа
             * если кандидаты только в одном регионе только исключаем кандидатов из региона
             */

            //строка
            for (int i = 0; i < 9; i++)
            {
                //число
                for (int k = 0; k < 9; k++)
                {
                    int count = 0;
                    //столбец
                    //подсчет кандидатов
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            count++;
                        }
                    }
                    //если кандидатов по строке больше 3 то пропускаем число
                    if (count != 2 && count != 3)
                        continue;
                    int[] flags = new int[count];
                    int[] cluesInd = new int[count];
                    //проверяю пренадлежность к одному региону
                    int ind = 0;
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            flags[ind] = j / 3;
                            cluesInd[ind] = j;
                            ind++;
                        }
                    }
                    bool flag = true;
                    for (int x = 1; x < flags.Length; x++)
                    {
                        if (flags[x] != flags[x - 1])
                        {
                            flag = false;
                        }
                    }
                    //если число в строке может стоять только в одном регионе, то в этом регионе оно может стоять только в этой строке
                    if (flag)
                    {
                        //рассматриваемое число k
                        //номер текущей строки i
                        //номер региона по строке лежит в flags
                        //значит левая верхняя ячейка нужного региона (3*flags[0] ; 3*i/3)
                        //от нее смотрим 9 ячеек кроме найденной строки и пытаемся исключить кандидатов
                        //если было хоть одно исключение то формируем ответ

                        int startX = 3 * flags[0];
                        int startY = 3 * (i / 3);

                        bool impact = false;
                        for (int y = 0; y < 3; y++)
                        {
                            if ((startY + y) != i)
                            {
                                for (int x = 0; x < 3; x++)
                                {
                                    if (field[startY + y, startX + x].value < 1)
                                    {
                                        if (field[startY + y, startX + x].candidates[k])
                                        {
                                            impact = true;


                                            //field[startY + y, startX + x].RemoveCandidat(k + 1);
                                            eliminations.Add(new Change(9 * (startY + y) + startX + x, k, ChangeType.RemovingDigit));

                                            removed.Add(new Mark(9 * (startY + y) + startX + x, k));
                                        }
                                        if (impact)
                                        {
                                            for (int n = 0; n < cluesInd.Length; n++)
                                            {
                                                clues.Add(new Mark(9 * i + cluesInd[n], k));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //если найдено исключение то формируем ответ
                        if (impact)
                        {
                            answer = new AnswerOfTech($"Cтрока  {(i + 1)}: виртуальная одиночка {(k + 1)} в регионе {(3 * startY / 3 + startX / 3 + 1)}");
                            SetLists(answer);
                            return answer;
                        }
                    }

                }

            }


            //столбец
            for (int j = 0; j < 9; j++)
            {
                //число
                for (int k = 0; k < 9; k++)
                {
                    int count = 0;
                    //строка
                    //подсчет кандидатов
                    for (int i = 0; i < 9; i++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            count++;
                        }
                    }
                    //если кандидатов по столбце больше 3 то пропускаем число
                    if (count != 2 && count != 3)
                        continue;
                    int[] flags = new int[count];
                    int[] cluesInd = new int[count];

                    //проверяю пренадлежность к одному региону
                    int ind = 0;
                    for (int i = 0; i < 9; i++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            flags[ind] = i / 3;
                            cluesInd[ind] = i;
                            ind++;
                        }
                    }
                    bool flag = true;
                    for (int x = 1; x < flags.Length; x++)
                    {
                        if (flags[x] != flags[x - 1])
                        {
                            flag = false;
                        }
                    }

                    if (flag)
                    {
                        //рассматриваемое число k
                        //номер текущего столбца j
                        //номер региона по столбцу лежит в flags
                        //значит левая верхняя ячейка нужного региона (3*j/3 ; 3*flags[0])
                        //от нее смотрим 9 ячеек кроме найденного столбца и пытаемся исключить кандидатов
                        //если было хоть одно исключение то формируем ответ

                        int startX = 3 * (j / 3);
                        int startY = 3 * flags[0];

                        bool impact = false;
                        for (int x = 0; x < 3; x++)
                        {
                            if ((startX + x) != j)
                            {
                                for (int y = 0; y < 3; y++)
                                {
                                    if (field[startY + y, startX + x].value < 1)
                                    {
                                        if (field[startY + y, startX + x].candidates[k])
                                        {
                                            impact = true;

                                            eliminations.Add(new Change(startY + y, startX + x, k, ChangeType.RemovingDigit));
                                            removed.Add(new Mark(9 * (startY + y) + startX + x, k));
                                        }
                                        if (impact)
                                        {
                                            for (int n = 0; n < cluesInd.Length; n++)
                                            {
                                                clues.Add(new Mark(9 * cluesInd[n] + j, k));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //если найдено исключение то формируем ответ
                        if (impact)
                        {
                            answer = new AnswerOfTech($"Cтолбец {(j + 1)}: виртуальная одиночка {(k + 1)} в регионе {(3 * startY / 3 + startX / 3 + 1)}");
                            SetLists(answer);
                            return answer;
                        }
                    }

                }

            }


            //регион
            //номера регионов
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    //число
                    for (int k = 0; k < 9; k++)
                    {

                        //посчет кандидатов
                        int count = 0;
                        //обход региона
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (field[3 * y + i, 3 * x + j].candidates[k])
                                {
                                    count++;
                                }
                            }
                        }
                        //если более трех то пропускаем
                        if (count != 2 && count != 3)
                            continue;


                        //проверка в одной ли они строке

                        int[] indexes = new int[count];
                        int[] cluesInd = new int[count];
                        count = 0;
                        //обходим регион
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (field[3 * y + i, 3 * x + j].candidates[k])
                                {
                                    //запоминаем строку
                                    indexes[count] = 3 * y + i;
                                    cluesInd[count] = 3 * x + j;
                                    count++;
                                }
                            }
                        }
                        //флаг что в одной строке
                        bool flag = true;
                        for (int i = 1; i < indexes.Length; i++)
                        {
                            if (indexes[i] != indexes[i - 1])
                            {
                                flag = false;
                            }
                        }
                        //номер региона x,y
                        //номер строки лежит в indexes[0]



                        //исключение из строки
                        if (flag)
                        {
                            //флаг что было исключение
                            bool impact = false;

                            //идем по всем столбцам найденной строки
                            for (int n = 0; n < 9; n++)
                            {
                                //пропуская текущий регион
                                if (n / 3 != x)
                                {
                                    if (field[indexes[0], n].value < 1)
                                    {
                                        if (field[indexes[0], n].candidates[k])
                                        {
                                            impact = true;

                                            eliminations.Add(new Change(indexes[0], n, k, ChangeType.RemovingDigit));
                                            removed.Add(new Mark(9 * indexes[0] + n, k));
                                        }
                                    }
                                }
                            }
                            //если было исключение то формируем ответ
                            if (impact)
                            {
                                for (int n = 0; n < cluesInd.Length; n++)
                                {
                                    clues.Add(new Mark(9 * indexes[0] + cluesInd[n], k));
                                }
                                answer = new AnswerOfTech($"Регион  {(y * 3 + x + 1)}: виртуальная одиночка {(k + 1)} в строке {(indexes[0] + 1)}");
                                SetLists(answer);
                                return answer;
                            }

                        }



                        //проверка в одном ли они столбце
                        //исключение из столбца
                        indexes = new int[count];
                        cluesInd = new int[count];
                        count = 0;
                        //обходим регион
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (field[3 * y + i, 3 * x + j].candidates[k])
                                {
                                    //запоминаем стобец
                                    indexes[count] = 3 * x + j;
                                    cluesInd[count] = 3 * y + i;
                                    count++;
                                }
                            }
                        }
                        //флаг что в одном столбце
                        flag = true;
                        for (int i = 1; i < indexes.Length; i++)
                        {
                            if (indexes[i] != indexes[i - 1])
                            {
                                flag = false;
                            }
                        }
                        //номер региона x,y
                        //номер столбца лежит в indexes[0]


                        //исключение из столбца
                        if (flag)
                        {
                            //флаг что было исключение
                            bool impact = false;

                            //идем по всем строкам найденного столбца
                            for (int n = 0; n < 9; n++)
                            {
                                //пропуская текущий регион
                                if (n / 3 != y)
                                {
                                    if (field[n, indexes[0]].value < 1)
                                    {
                                        if (field[n, indexes[0]].candidates[k])
                                        {
                                            impact = true;

                                            eliminations.Add(new Change(n, indexes[0], k, ChangeType.RemovingDigit));
                                            removed.Add(new Mark(9 * n + indexes[0], k));
                                        }
                                    }
                                }
                            }
                            //если было исключение то формируем ответ
                            if (impact)
                            {
                                for (int n = 0; n < cluesInd.Length; n++)
                                {
                                    clues.Add(new Mark(9 * cluesInd[n] + indexes[0], k));
                                }
                                answer = new AnswerOfTech($"Регион {(y * 3 + x + 1)}: виртуальная одиночка {(k + 1)} в столбце {(indexes[0] + 1)}");
                                SetLists(answer);
                                return answer;
                            }

                        }

                    }
                }
            }
            return answer;
        }
        //простые ислючения
        public AnswerOfTech SimpleRestriction(Field field)
        {
            return techCheckers.Find((x) => x.Type == TechType.SimpleRestriction).CheckField(field);
        }

        
        private void SetLists(AnswerOfTech answer)
        {
            answer.Seted = seted;
            answer.Eliminations = eliminations;

            answer.Clues = clues;
            answer.Removed = removed;
        }

        private void ClearLists()
        {
            seted.Clear();
            removed.Clear();
            clues.Clear();
            eliminations.Clear();
        }
    }
}
