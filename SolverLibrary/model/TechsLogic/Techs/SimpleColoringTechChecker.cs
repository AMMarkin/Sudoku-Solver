using System.Collections.Generic;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class SimpleColoringTechChecker : TechChecker
    {
        public override TechType Type => TechType.SimpleColoring;

        protected override string Discription => "Simple Coloring";

        protected delegate int GetTargetParameter(int x);

        protected List<int[]> chain = new List<int[]>();        //ind, k => ind, k    сильные связи !A =>  B
        protected List<int[]> weak = new List<int[]>();         //ind, k => ind, k     слабые связи  A => !B
        protected List<int[]> chainUnits = new List<int[]>();   //ind, k

        //цепи по двум цветам
        protected readonly List<int[]> ON = new List<int[]>();  //ind, k
        protected readonly List<int[]> OFF = new List<int[]>(); //ind, k

        protected enum Color
        {
            ON,
            OFF
        }

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

            GetTargetParameter getRowFromIndex = (x) => x / Field.Row_Count;
            GetTargetParameter getColumnFromIndex = (x) => x % Field.Column_Count;
            GetTargetParameter getReginFromIndex = (x) => 3 * (getRowFromIndex(x) / 3) + (getColumnFromIndex(x) / 3);


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
            targetParam = -1;

            bool repeat = false;

            foreach (int[] unit in ON)
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
                    break;
                }
            }
            if (repeat)
            {
                answer = MakeAnswer($"повторение цвета в {groupName} {(targetParam + 1)}");
                AddChainLists(answer);
                return answer;
            }

            //для OFF
            for (int j = 0; j < 9; j++)
            {
                flags[j] = false;
            }
            targetParam = -1;
            foreach (int[] unit in OFF)
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
                    //Все ON удаляются
                    foreach (int[] rem in OFF)
                    {
                        AddElimination(rem[0], rem[1]);
                        AddRemovingMark(rem[0], rem[1]);
                    }
                    OFF.Clear();
                    break;
                }
            }
            if (repeat)
            {
                answer = MakeAnswer($"повторение цвета в {groupName} {(targetParam + 1)}");
                AddChainLists(answer);
                return answer;
            }
            return null;
        }

        //исключение кандидатов видимых двумя цветами
        protected AnswerOfTech TwoColorsElsewhere(Field field, int k)
        {
            AnswerOfTech answer;

            List<int> seenbyON = new List<int>();
            List<int> intersec = new List<int>();

            int i, j;
            int i1, j1;
            //находим все ячейки которые видны кандидатом k в группе ON
            foreach (int[] unit in ON)
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

            //находим все ячейки которые видны кандидатом k в группе OFF
            foreach (int[] unit in OFF)
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
                            if (!intersec.Contains(9 * i1 + j1)) 
                            { 
                                intersec.Add(9 * i1 + j1);
                            }
                        }
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

        //раскраска
        protected void SubChainColoring(int subChainNumber, int[] subchains)
        {
            ON.Clear();
            OFF.Clear();

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
                    subChain[counter][0] = chainUnits[j][0];    //ind
                    subChain[counter][1] = chainUnits[j][1];    //k
                    subChain[counter][2] = 0;                   //color
                    counter++;
                }
            }

            //раскрашиваю начиная с ON
            subChain[0][2] = 1;

            int[][] links = FindStrongLinksInChain(subChain[0][0], subChain[0][1]);

            //ON  - true
            //OFF - false


            //TODO переделать флаг на enum
            Coloring(ref subChain, links, false);


            for (int j = 0; j < subChain.Length; j++)
            {
                if (subChain[j][2] == 1)
                {
                    AddColoredUnit(subChain[j][0], subChain[j][1], Color.ON);
                }
                if (subChain[j][2] == -1)
                {
                    AddColoredUnit(subChain[j][0], subChain[j][1], Color.OFF);
                }
            }
        }

        //рекурсивный метод для раскраски цепи
        private void Coloring(ref int[][] subChain, int[][] links, bool OnOff)
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
                        Coloring(ref subChain, FindStrongLinksInChain(subChain[j][0], subChain[j][1]), !OnOff);
                        break;
                    }
                }

            }
        }

        //создание сильной цепи 
        protected void CreateChain(Field field, params int[] kArray)
        {
            int[][] matrix = new int[9][];

            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int counter;
            int a;
            int b;

            chain.Clear();
            weak.Clear();
            chainUnits.Clear();

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
                        AddLinkToChain(a, b, k, k);
                        AddUnitToChain(a, k);
                        AddUnitToChain(b, k);
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
                        AddLinkToChain(a, b, k, k);
                        AddUnitToChain(a, k);
                        AddUnitToChain(b, k);

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
                            AddLinkToChain(a, b, k, k);
                            AddUnitToChain(a, k);
                            AddUnitToChain(b, k);
                        }
                    }
                }
            }
        }

        //найти все сильные связи в цепи
        protected int[][] FindStrongLinksInChain(int ind, int k)
        {
            //считаю колличество связей
            int counter = 0;
            foreach (int[] link in chain)
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
            foreach (int[] link in chain)
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

        //дфс по сильным связям
        protected void DfsStrong(ref bool[] visited, ref int[] component, ref int components, int v, Field field)
        {
            visited[v] = true;
            component[v] = components;
            foreach (int[] link in chain)
            {
                //нашли ребро

                if (chainUnits[v][0] == link[0] && chainUnits[v][1] == link[1])
                {
                    //нахожу номер конца ребра в chainUnits
                    int count = 0;
                    foreach (int[] unit in chainUnits)
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

        //очистка цепи для комфортного отображения
        protected void ClearChainBySubChain(int subchainNumber, int[] subchains)
        {
            int ind, k;
            List<int[]> rem = new List<int[]>();
            for (int i = 0; i < subchains.Length; i++)
            {
                //если относится к ненужной компоненте
                if (subchains[i] != subchainNumber)
                {
                    //запоминаем
                    ind = chainUnits[i][0];
                    k = chainUnits[i][1];

                    foreach (int[] link in chain)
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
                chain.Remove(rem[i]);
            }

        }

        //добавление в цепь новой связь
        protected void AddLinkToChain(int ind1, int ind2, int k1, int k2)
        {
            bool contains = false;
            foreach (int[] item in chain)
            {
                if (item[0] == ind1 && item[1] == k1 && item[2] == ind2 && item[3] == k2)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                chain.Add(new int[] { ind1, k1, ind2, k2 });
            }
            contains = false;
            foreach (int[] item in chain)
            {
                if (item[0] == ind2 && item[1] == k2 && item[2] == ind1 && item[3] == k1)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                chain.Add(new int[] { ind2, k2, ind1, k1 });
            }
        }

        //добавление в цепь нового звена
        protected void AddUnitToChain(int ind, int k)
        {
            bool contains = false;
            foreach (int[] unit in chainUnits)
            {
                if (unit[0] == ind && unit[1] == k)
                {
                    contains = true;
                    break;
                }
            }
            if (!contains)
            {
                chainUnits.Add(new int[] { ind, k });
            }
        }

        protected void AddColoredUnit(int index, int digit, Color color)
        {
            if (color == Color.ON)
            {
                ON.Add(new int[] { index, digit });
            }
            else
            {
                OFF.Add(new int[] { index, digit });
            }
        }

        protected void AddChainLists(AnswerOfTech answer)
        {
            answer.Chain = chain;
            answer.Weak = weak;
            answer.ChainUnits = chainUnits;

            answer.ON = ON;
            answer.OFF = OFF;
        }

        protected void ClearChainLists()
        {
            chain.Clear();
            weak.Clear();
            chainUnits.Clear();

            ON.Clear();
            OFF.Clear();
        }
    }
}
