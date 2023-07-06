using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuSolver
{
    internal static class Logic
    {

        public static string noFound = "Исключений не найдено";

        //массив и библиотека названий техник
        public static List<string> tecniques = new List<string>() {
            "Открытые одиночки", "Скрытые одиночки","Виртуальные одиночки",
            "Открытые пары", "Cкрытые пары",
            "Открытые тройки", "Скрытые тройки",
            "Открытые четверки", "Скрытые четверки",
            "BUG",
            //"X-Cycles",
            "X-Wings","Swordfish","Jellyfish",
            "Y-Wings","XYZ-Wing",
            "Simple Coloring","Extended Simple Coloring"
        };

        public static Dictionary<string, int> tech = new Dictionary<string, int>();

        //список ключей и исключенных кандидатов
        public static List<int[]> clues;        //i,j -- где ключ,         k -- что ключ
        public static List<int[]> removed;      //i,j -- откуда исключаем, k -- что исключаем


        //для цепных техник
        //список связей и звеньев цепи 
        //ind = 9 * i + j
        public static List<int[]> chain;        //ind, k => ind, k    сильные связи !A =>  B
        public static List<int[]> weak;         //ind, k => ind, k     слабые связи  A => !B
        public static List<int[]> chainUnits;   //ind, k

        //цепи по двум цветам
        public static List<int[]> ON;           //ind, k
        public static List<int[]> OFF;          //ind, k

        public static bool done = false;

        //перебор всех техник внесенных по возрастанию сложности
        public static string findElimination(Field field, bool[] tecFlags)
        {

            string answer = noFound;

            //очистка переменных
            string tmp = "";
            ClearChainBuffer();

            //открытые одиночки
            if (tecFlags[tech["Открытые одиночки"]])
            {
                tmp = NakedSingle(field);

                //проверка простых исключений
                SimpleRestriction(field);

                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //скрытые одиночки
            if (tecFlags[tech["Скрытые одиночки"]])
            {
                tmp = HiddenSingle(field);

                //проверка простых исключений
                SimpleRestriction(field);

                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //виртуальные одиночки
            if (tecFlags[tech["Виртуальные одиночки"]])
            {
                tmp = VirtualSingle(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //открытые пары
            if (tecFlags[tech["Открытые пары"]])
            {
                tmp = NakedPairs(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //скрытые пары
            if (tecFlags[tech["Cкрытые пары"]])
            {
                tmp = HiddenPairs(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //открытые тройки
            if (tecFlags[tech["Открытые тройки"]])
            {
                tmp = NakedTriples(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //скрытые тройки
            if (tecFlags[tech["Скрытые тройки"]])
            {
                tmp = HiddenTriples(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Открытые четверки
            if (tecFlags[tech["Открытые четверки"]])
            {
                tmp = NakedQuads(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Скрытые четверки
            if (tecFlags[tech["Скрытые четверки"]])
            {
                tmp = HiddenQuads(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //BUG
            if (tecFlags[tech["BUG"]])
            {
                tmp = BUG(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //X-Wings
            if (tecFlags[tech["X-Wings"]])
            {
                tmp = X_Wings(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //XYZ-Wing
            if (tecFlags[tech["XYZ-Wing"]])
            {
                tmp = XYZ_Wing(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Swordfish
            if (tecFlags[tech["Swordfish"]])
            {
                tmp = Swordfish(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Jellyfish
            if (tecFlags[tech["Jellyfish"]])
            {
                tmp = Jellyfish(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Y-Wings
            if (tecFlags[tech["Y-Wings"]])
            {
                tmp = Y_Wings(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
            }

            //Simple Coloring
            if (tecFlags[tech["Simple Coloring"]])
            {
                tmp = SimpleColoring(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
                else
                {
                    ON.Clear();
                    OFF.Clear();
                    chain.Clear();
                }
            }

            //Extended Simple Coloring
            if (tecFlags[tech["Extended Simple Coloring"]])
            {
                tmp = ExtendedSimpleColoring(field);
                if (!tmp.Equals(""))
                {
                    return tmp;
                }
                else
                {
                    ON.Clear();
                    OFF.Clear();
                    chain.Clear();
                }
            }

            //проверка решения
            tmp = check(field);
            if (!tmp.Equals(""))
            {
                return tmp;
            }

            return answer;
        }

        public static void ClearChainBuffer()
        {
            clues?.Clear();
            clues = clues ?? new List<int[]>();

            removed?.Clear();
            removed = removed ?? new List<int[]>();

            chain?.Clear();
            chain = chain ?? new List<int[]>();

            weak?.Clear();
            weak = weak ?? new List<int[]>();

            chainUnits?.Clear();
            chainUnits = chainUnits ?? new List<int[]>();

            ON?.Clear();
            ON = ON ?? new List<int[]>();

            OFF?.Clear();
            OFF = OFF ?? new List<int[]>();
        }

        //WXYZ-Wing
        private static string WXYZ_Wing(Field field)
        {
            string answer = "";

            //к моменту использования этой техники уже будут найдены все тройки и четверки

            //нахожу три ячейки с четыремя кандидатами в строке или столбце
            //по очереди пытаюсь выбрать одну из ячеек корнем
            //в регионе корня ищу ячейку которая "закроет" четверку
            
            //внутри четверки считаю колличество "несвязанных" чисел
            //если несвязанных !=1 то скип
            //если ==1 то ищу исключения среди ячеек которые видимы всеми ячейками с несвязанным числом 









            return answer;
        }



        //X-Cycles
        //поиск циклов в цепи из сильных и слабых связей для одного числа
        //не готово
        private static string X_Cycles(Field field)
        {
            string answer = "";

            int[] subChains;
            int subChainCounter;

            for(int k = 0; k < 9; k++)
            {
                //сильные связи для числа
                CreateChain(field, k);
                //нахожу слабые связи
                fillWeakLinks();


                //нахожу компоненты связности
                subChains = new int[chainUnits.Count];
                subChainCounter = 0;

                bool[] visited = new bool[chainUnits.Count];

                for (int v = 0; v < chainUnits.Count; v++)
                {
                    if (!visited[v])
                    {
                        dfsWeakStrong(ref visited, ref subChains, ref subChainCounter, v);
                        subChainCounter++;
                    }
                }
                //для всех кусков
                for (int i = 0; i < subChainCounter; i++)
                {

                    //раскрашиваю
                    //SubChainColoring(i, subChains);

                    ClearChainBySubChain(i, subChains);



                }



            }
            


            return answer;
        }

        //Extended Simple Coloring
        //--------------------------------------------------------------------------------------------------------
        //сильные связи для всех цветов + ячейки с двумя кандидатам(сильная связь внутри ячейки)
        private static string ExtendedSimpleColoring(Field field)
        {
            string answer = "";

            int[] subChains = null;
            int subChainCounter = 0;

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
                    dfsStrong(ref visited, ref subChains, ref subChainCounter, v);
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
                answer = chainLogicRepeatRule(field, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
                if (!answer.Equals(""))
                {
                    ClearChainBySubChain(i, subChains);
                    return ("Extended Simple Coloring: " + answer);
                }
                //повторение цвета в ячейке
                answer = TwiceInCellRule(field);
                if (!answer.Equals(""))
                {
                    ClearChainBySubChain(i, subChains);
                    return ("Extended Simple Coloring: " + answer);
                }
                answer = TwoColorsInCell(field);
                if (!answer.Equals(""))
                {
                    ClearChainBySubChain(i, subChains);
                    return ("Extended Simple Coloring: " + answer);
                }
                //кандидаты видимые из двух цветов
                for (int k = 0; k < 9; k++)
                {
                    answer = TwoColorsElsewhere(field, k);
                    if (!answer.Equals(""))
                    {
                        ClearChainBySubChain(i, subChains);
                        return ("Extended Simple Coloring: " + answer);
                    }
                }
                //кандидаты делящие ячейку с одним цветом и видимые други
                answer = TwoColorsUnitCell(field);
                if (!answer.Equals(""))
                {
                    ClearChainBySubChain(i, subChains);
                    return ("Extended Simple Coloring: " + answer);
                }
                //цвет полностью исключающий ячейку
                answer = CellEmptiedByColor(field);
                if (!answer.Equals(""))
                {
                    ClearChainBySubChain(i, subChains);
                    return ("Extended Simple Coloring: " + answer);
                }

            }
            return answer;
        }

        //цвет полностью исключающий одну ячейку
        private static string CellEmptiedByColor(Field field)
        {
            string answer = "";

            bool emptyed;
            bool contains;

            int i1, j1;

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
                    for (int k = 0; k < 9; k++)
                    {
                        if (!field[i, j].candidates[k])
                        {
                            continue;
                        }
                        if (!seenByColor(9 * i + j, k, true))
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
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
                        }
                        ON.Clear();
                        ON.AddRange(OFF);
                        OFF.Clear();

                        answer = "все числа в ячейке (" + (i + 1) + ";" + (j + 1) + ") видны цветом";
                        return answer;

                    }

                    //для OFF
                    for (int k = 0; k < 9; k++)
                    {
                        if (!field[i, j].candidates[k])
                        {
                            continue;
                        }
                        if (!seenByColor(9 * i + j, k, false))
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
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
                        }
                        OFF.Clear();

                        answer = "все числа в ячейке (" + (i + 1) + ";" + (j + 1) + ") видны цветом";
                        return answer;

                    }

                }
            }


            return answer;
        }

        //виден ли кандидат в ячейке цветом
        private static bool seenByColor(int ind, int k, bool OnOff)
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
                    if (isSeen(ind, unit[0]))
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
                    if (isSeen(ind, unit[0]))
                    {
                        return true;
                    }

                }

            }


            return res;
        }

        //исключение кандидатов видимых одним цветом и находящихся в одной ячейке с другим
        private static string TwoColorsUnitCell(Field field)
        {
            string answer = "";
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
                for (int k = 0; k < 9; k++)
                {
                    //если кандидат есть и не закрашен
                    if (k != unit1[1] && field[i1, j1].candidates[k])
                    {
                        //ищем ему пару во втором цвете
                        foreach (int[] unit2 in OFF)
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
                            if (!isSeen(unit1[0], unit2[0]))
                            {
                                continue;
                            }

                            //если не пропустили то исключаем и составляем ответ
                            i2 = unit2[0] / 9;
                            j2 = unit2[0] % 9;

                            answer = (k + 1) + " в ячейке (" + (i1 + 1) + ";" + (j1 + 1) + ") " +
                                     "видит один цвет и свою пару в ячейке (" + (i2 + 1) + ";" + (j2 + 1) + ")";
                            field[i1, j1].RemoveCandidat(k + 1);
                            removed.Add(new int[] { i1, j1, k });
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
                for (int k = 0; k < 9; k++)
                {
                    //если кандидат есть и не закрашен
                    if (k != unit1[1] && field[i1, j1].candidates[k])
                    {
                        //ищем ему пару во втором цвете
                        foreach (int[] unit2 in ON)
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
                            if (!isSeen(unit1[0], unit2[0]))
                            {
                                continue;
                            }

                            //если не пропустили то исключаем и составляем ответ
                            i2 = unit2[0] / 9;
                            j2 = unit2[0] % 9;

                            answer = (k + 1) + " в ячейке (" + (i1 + 1) + ";" + (j1 + 1) + ") " +
                                     "видит один цвет и свою пару в ячейке (" + (i2 + 1) + ";" + (j2 + 1) + ")";
                            field[i1, j1].RemoveCandidat(k + 1);
                            removed.Add(new int[] { i1, j1, k });
                            return answer;
                        }
                    }
                }
            }


            return answer;
        }

        //проверка видят ли ячейки друг друга
        private static bool isSeen(int ind1, int ind2)
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
        private static string TwoColorsInCell(Field field)
        {
            string answer = "";

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

                        for (int k = 0; k < 9; k++)
                        {
                            if (k != unit1[1] && k != unit2[1] && field[i, j].candidates[k])
                            {
                                field[i, j].RemoveCandidat(k + 1);
                                removed.Add(new int[] { i, j, k });
                                impact = true;
                            }
                        }
                        if (impact)
                        {
                            answer = "два цвета в ячейке (" + (i + 1) + ";" + (j + 1) + ")";
                            return answer;
                        }
                    }

                }

            }

            return answer;
        }

        //проверка появления одного цвета дважды в одной ячейке
        private static string TwiceInCellRule(Field field)
        {
            string answer = "";

            //иду по первому цвету
            int i1, j1;

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
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
                        }
                        //для красоты перекидываем оставшееся в ON
                        ON.Clear();
                        ON.AddRange(OFF);
                        OFF.Clear();
                        answer = "повторение цвета в ячейке (" + (unit2[0] / 9 + 1) + ";" + (unit2[0] % 9 + 1) + ")";
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
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
                        }
                        //для красоты перекидываем оставшееся в ON
                        OFF.Clear();
                        answer = "повторение цвета в ячейке (" + (unit2[0] / 9 + 1) + ";" + (unit2[0] % 9 + 1) + ")";
                        return answer;
                    }
                }
            }
            return answer;
        }

        //добавление в цепь сильных связей ячеек с двумя кандидатами
        private static void AddBiValueToChain(Field field)
        {
            int counter = 0;
            int a = -1;
            int b = -1;

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

        //очистка цепи для комфортного отображения
        private static void ClearChainBySubChain(int subchainNumber, int[] subchains)
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

        //DEBUG
        private static string printBiValue()
        {
            string answer = "";
            int i, j;
            foreach (int[] link in chain)
            {
                if (link[0] == link[2])
                {
                    i = link[0] / 9;
                    j = link[0] % 9;

                    answer += "\n(" + (i + 1) + ";" + (j + 1) + ") => " + (link[1] + 1) + " - " + (link[3] + 1);
                }
            }

            return answer;
        }

        //Simple Coloring
        //--------------------------------------------------------------------------------------------------------
        //только по сильным связям
        private static string SimpleColoring(Field field)
        {
            string answer = "";


            int[] subChains = null;
            int subChainCounter = 0;


            for (int k = 0; k < 9; k++)
            {
                CreateChain(field, k);


                //поиск исключений

                //раскрашиваем цепь в 2 цвета

                //списки цветов
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
                        dfsStrong(ref visited, ref subChains, ref subChainCounter, v);
                        subChainCounter++;
                    }
                }
                //для всех кусков
                for (int i = 0; i < subChainCounter; i++)
                {

                    //раскрашиваю
                    SubChainColoring(i, subChains);

                    //поиск исключений

                    //повторения цвета
                    //для одного числа

                    answer = chainLogicRepeatRule(field, new int[] { k });
                    if (!answer.Equals(""))
                    {
                        ClearChainBySubChain(i, subChains);
                        return ("Simple Coloring: " + answer);
                    }

                    //ячейки видимые двумя цветами
                    answer = TwoColorsElsewhere(field, k);
                    if (!answer.Equals(""))
                    {
                        ClearChainBySubChain(i, subChains);
                        return ("Simple Coloring: " + answer);
                    }

                }
            }

            return answer;
        }

        //исключение кандидатов видимых двумя цветами
        private static string TwoColorsElsewhere(Field field, int k)
        {
            string answer = "";

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
                            intersec.Add(9 * i1 + j1);
                        }
                    }

                }
            }
            bool impact = false;
            answer = (k + 1) + " в ячейках: ";
            foreach (int ind in intersec)
            {
                i = ind / 9;
                j = ind % 9;
                if (field[i, j].candidates[k])
                {
                    answer += "(" + (i + 1) + ";" + (j + 1) + ") ";
                    field[i, j].RemoveCandidat(k + 1);
                    impact = true;
                }
                removed.Add(new int[] { i, j, k });
            }
            if (impact)
            {
                answer += "видимы из обоих цветов";
            }
            else
            {
                answer = "";
            }


            return answer;
        }

        //повторение цвета 
        private static string chainLogicRepeatRule(Field field, int[] kArray)
        {
            string answer = "";

            bool repeat = false;
            bool[] flags = new bool[9];

            int i1, j1;
            //по всем числам
            foreach (int k in kArray)
            {
                //в строках
                //------------------------------------------------------------------------------
                //для ON
                int row;
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                row = -1;
                foreach (int[] unit in ON)
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
                        //Все ON удаляются
                        foreach (int[] rem in ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
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
                    answer = "повторение цвета в строке " + (row + 1);
                    return answer;
                }

                //для OFF
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                row = -1;
                foreach (int[] unit in OFF)
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
                        //Все ON удаляются
                        foreach (int[] rem in OFF)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
                        }
                        OFF.Clear();
                        break;
                    }
                }
                if (repeat)
                {
                    answer = "повторение цвета в строке " + (row + 1);
                    return answer;
                }
                //------------------------------------------------------------------------------
                //в столбцах
                int col;
                //для ON
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                col = -1;
                foreach (int[] unit in ON)
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
                        //Все ON исключаются
                        foreach (int[] rem in ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
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
                    answer = "повторение цвета в столбце " + (col + 1);
                    return answer;
                }

                //для OFF
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                col = -1;
                foreach (int[] unit in OFF)
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
                        //Все ON исключаются
                        foreach (int[] rem in OFF)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
                        }
                        //для красоты перекидываем оставшееся в ON
                        OFF.Clear();
                        break;
                    }
                }
                //если повторение в столбцах то 
                if (repeat)
                {
                    answer = "повторение цвета в столбце " + (col + 1);
                    return answer;
                }
                //------------------------------------------------------------------------------
                //в регионах
                int reg;
                //для ON
                for (int j = 0; j < 9; j++)
                {
                    flags[j] = false;
                }
                reg = -1;
                foreach (int[] unit in ON)
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
                        //Все ON исключаются
                        foreach (int[] rem in ON)
                        {
                            i1 = rem[0] / 9;
                            j1 = rem[0] % 9;
                            field[i1, j1].RemoveCandidat(rem[1] + 1);
                            removed.Add(new int[] { i1, j1, rem[1] });
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
                    answer = "повторение цвета в регионе " + (reg + 1);
                    return answer;
                }
                //для OFF
                if (!repeat)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        flags[j] = false;
                    }
                    col = -1;
                    foreach (int[] unit in OFF)
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
                            //Все OFF исключаются
                            foreach (int[] rem in OFF)
                            {
                                i1 = rem[0] / 9;
                                j1 = rem[0] % 9;
                                field[i1, j1].RemoveCandidat(rem[1] + 1);
                                removed.Add(new int[] { i1, j1, rem[1] });
                            }
                            //для красоты перекидываем оставшееся в ON
                            OFF.Clear();
                            break;
                        }
                    }
                }

                //если повторение в регионе
                if (repeat)
                {
                    answer = "повторение цвета в регионе " + (reg + 1);
                    return answer;
                }
            }

            return answer;
        }

        //раскраска
        private static void SubChainColoring(int subChainNumber, int[] subchains)
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
                    subChain[counter][0] = chainUnits[j][0];    //ind
                    subChain[counter][1] = chainUnits[j][1];    //k
                    subChain[counter][2] = 0;                   //color
                    counter++;
                }
            }

            //раскрашиваю начиная с ON
            subChain[0][2] = 1;

            int[][] links = findStrongLinksInChain(subChain[0][0], subChain[0][1]);

            //ON  - true
            //OFF - false

            coloring(ref subChain, links, false);



            ON.Clear();
            OFF.Clear();
            for (int j = 0; j < subChain.Length; j++)
            {
                if (subChain[j][2] == 1)
                {
                    ON.Add(new int[] { subChain[j][0], subChain[j][1] });
                }
                if (subChain[j][2] == -1)
                {
                    OFF.Add(new int[] { subChain[j][0], subChain[j][1] });
                }
            }
        }

        //рекурсивный метод для раскраски цепи
        private static void coloring(ref int[][] subChain, int[][] links, bool OnOff)
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
                        coloring(ref subChain, findStrongLinksInChain(subChain[j][0], subChain[j][1]), !OnOff);
                        break;
                    }
                }

            }
        }

        //создание сильной цепи 
        public static void CreateChain(Field field,params int[] kArray)
        {
            int[][] matrix = new int[9][];

            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }

            int counter = 0;
            int a = -1;
            int b = -1;

            if (chain != null)
            {
                chain.Clear();
            }
            else
            {
                chain = new List<int[]>();
            }
            if (weak != null)
            {
                weak.Clear();
            }
            else
            {
                weak = new List<int[]>();
            }
            if (chainUnits != null)
            {
                chainUnits.Clear();
            }
            else
            {
                chainUnits = new List<int[]>();
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
                //----------------------------------------------------------------------------------------------------------------------------
                //слабые связи
                //fillWeakLinks();
            }
        }

        //DEBUG
        private static string pringLinks(int ind, int k, int[][] links)
        {
            string ans = "(" + (ind / 9 + 1) + ";" + (ind % 9 + 1) + ") => "
                ;
            for (int i = 0; i < links.Length; i++)
            {
                ans += "(" + (links[i][0] / 9 + 1) + ";" + (links[i][0] % 9 + 1) + ") ";
            }



            return ans;
        }

        //найти все сильные связи в цепи
        private static int[][] findStrongLinksInChain(int ind, int k)
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

        //DEBUG
        private static string printSubChains(int subChainCounter, int[] subChains)
        {
            string ans = "";
            for (int i = 0; i < subChainCounter; i++)
            {
                ans += "\n---------------" + (i + 1) + "---------------\n";
                for (int j = 0; j < chainUnits.Count; j++)
                {
                    if (i == subChains[j])
                    {
                        int x = chainUnits[j][0] / 9;
                        int y = chainUnits[j][0] % 9;
                        ans += "(" + (x + 1) + ";" + (y + 1) + ") ";

                    }
                }

            }
            return ans;
        }

        //заполнение слабых связей
        public static void fillWeakLinks()
        {
            //полный перебор

            foreach (int[] unit1 in chainUnits)
            {
                foreach (int[] unit2 in chainUnits)
                {
                    if (!unit1.Equals(unit2))
                    {
                        bool linked = false;
                        foreach (int[] link in chain)
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
                            if ((unit1[1] == unit2[1]) && isSeen(unit1[0], unit2[0]))
                            {
                                //добавляю слабую связь
                                weak.Add(new int[] { unit1[0], unit1[1], unit2[0], unit2[1] });
                            }

                        }
                    }
                }
            }
        }

        //дфс по сильным и слабым связям
        private static void dfsWeakStrong(ref bool[] visited, ref int[] component, ref int components, int v)
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
                                dfsWeakStrong(ref visited, ref component, ref components, count);
                            }
                        }
                        else
                        {
                            count++;
                        }

                    }
                }
            }

            foreach (int[] link in weak)
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
                                dfsWeakStrong(ref visited, ref component, ref components, count);
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
        private static void dfsStrong(ref bool[] visited, ref int[] component, ref int components, int v)
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
                                dfsStrong(ref visited, ref component, ref components, count);
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
        private static void AddLinkToChain(int ind1, int ind2, int k1, int k2)
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
        private static void AddUnitToChain(int ind, int k)
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
        //--------------------------------------------------------------------------------------------------------

        //XYZ-Wing
        private static string XYZ_Wing(Field field)
        {
            string answer = "";

            //ищем все ячейки с тремя кандидатами

            List<int> indexes = new List<int>();
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

                                if (rem == -1)
                                {
                                    answer = "ОШИБКА ЛОХ";
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
                                        field[i, 3 * (j / 3) + r].RemoveCandidat(rem + 1);
                                        removed.Add(new int[] { i, 3 * (j / 3) + r, rem });



                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                clues.Add(new int[] { i, j, k });
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                clues.Add(new int[] { i1, j1, k });
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                clues.Add(new int[] { i2, j2, k });
                                            }
                                        }

                                        answer = "XYZ-Wing: исключена " + (rem + 1) + " из (" + (i + 1) + ";" + (3 * (j / 3) + r + 1) + ")";
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
                                        field[3 * (i / 3) + r, j].RemoveCandidat(rem + 1);
                                        removed.Add(new int[] { 3 * (i / 3) + r, j, rem });



                                        for (int k = 0; k < 9; k++)
                                        {
                                            if (field[i, j].candidates[k])
                                            {
                                                clues.Add(new int[] { i, j, k });
                                            }
                                            if (field[i1, j1].candidates[k])
                                            {
                                                clues.Add(new int[] { i1, j1, k });
                                            }
                                            if (field[i2, j2].candidates[k])
                                            {
                                                clues.Add(new int[] { i2, j2, k });
                                            }
                                        }

                                        answer = "XYZ-Wing: исключена " + (rem + 1) + " из (" + (3 * (i / 3) + r + 1) + ";" + (j + 1) + ")";
                                        return answer;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return answer;
        }

        //Y-Wings
        private static string Y_Wings(Field field)
        {
            string answer = "";

            //ищем все ячейки с двумя кандидатами

            List<Field.Cell> list = new List<Field.Cell>();

            int counter = 0;


            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i,j].remainingCandidates == 2)
                    {
                        list.Add(field[i, j]);
                    }
                }
            }

            Field.Cell Y;
            Field.Cell X1;
            Field.Cell X2;

            int a = 0;
            int b = 0;
            int c = 0;


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

                                            removed.Add(new int[] { X1.seenCell[n].row, X1.seenCell[n].column, c });
                                            X1.seenCell[n].RemoveCandidat(c + 1);

                                        }

                                    }
                                }
                            }

                            if (foundet)
                            {
                                clues.Add(new int[] { Y.row, Y.column, a });
                                clues.Add(new int[] { Y.row, Y.column, b });
                                clues.Add(new int[] { X1.row, X1.column, a });
                                clues.Add(new int[] { X1.row, X1.column, c });
                                clues.Add(new int[] { X2.row, X2.column, b });
                                clues.Add(new int[] { X2.row, X2.column, c });

                                answer = "Y-Wings по " + (c + 1) + " : " +
                                         "(" + (Y.row + 1) + ";" + (Y.column + 1) + ") => " +
                                         "(" + (X1.row + 1) + ";" + (X1.column + 1) + ") - " +
                                         "(" + (X2.row + 1) + ";" + (X2.column + 1) + ")"
                                         ;
                                return answer;

                            }

                        }

                    }

                }
            }



            return answer;
        }

        //BUG
        private static string BUG(Field field)
        {
            string answer = "";

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
                        return answer;
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
                return answer;
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
                    return "BUG: ОШИБКА ПОДСЧЕТА КАНДИДАТОВ";
                }
                if (candidats_counter[k] == 3)
                {
                    counter++;
                    bugDigit = k;
                }
            }

            if (counter != 1)
            {
                return "BUG: ОШИБКА";
            }

            answer = "BUG: если в ячейке (" + (bugI + 1) + ";" + (bugJ + 1) + ") установить не " + (bugDigit + 1) + ", то судоку будет иметь 2 решения";
            clues.Add(new int[] { bugI, bugJ, bugDigit });
            for (int k = 0; k < 9; k++)
            {
                if (k == bugDigit) continue;

                if (field[bugI, bugJ].candidates[k])
                {
                    removed.Add(new int[] { bugI, bugJ, k });
                }
            }
            field[bugI, bugJ].SetValue(bugDigit + 1);

            return answer;
        }

        //Jellyfish
        private static string Jellyfish(Field field)
        {
            string answer = "";

            int[][] matrix = new int[9][];
            int[][] shape;
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }
            //по всем числам
            for (int k = 0; k < 9; k++)
            {
                //смотрю строки
                //исключаю в колонках

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[i][j] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 0;
                        }
                    }
                }

                shape = Shape4InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] строки
                //[1] колонки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                        {
                            field[i, shape[1][0]].RemoveCandidat(k + 1);
                            field[i, shape[1][1]].RemoveCandidat(k + 1);
                            field[i, shape[1][2]].RemoveCandidat(k + 1);
                            field[i, shape[1][3]].RemoveCandidat(k + 1);

                            removed.Add(new int[] { i, shape[1][0], k });
                            removed.Add(new int[] { i, shape[1][1], k });
                            removed.Add(new int[] { i, shape[1][2], k });
                            removed.Add(new int[] { i, shape[1][3], k });
                        }
                    }

                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            clues.Add(new int[] { shape[0][x], shape[1][y], k });
                        }
                    }

                    answer = "Jellyfish по " + (k + 1) +
                            " в (" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + "-" + (shape[0][3] + 1) + ";"
                                   + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + "-" + (shape[1][3] + 1) + ")"
                            ;
                    return answer;

                }


                //смотрю столбцы
                //исключаю строки

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[j][i] = 0;
                        }
                    }
                }

                shape = Shape4InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] колонки
                //[1] строки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                        {
                            field[shape[1][0], i].RemoveCandidat(k + 1);
                            field[shape[1][1], i].RemoveCandidat(k + 1);
                            field[shape[1][2], i].RemoveCandidat(k + 1);
                            field[shape[1][3], i].RemoveCandidat(k + 1);

                            removed.Add(new int[] { shape[1][0], i, k });
                            removed.Add(new int[] { shape[1][1], i, k });
                            removed.Add(new int[] { shape[1][2], i, k });
                            removed.Add(new int[] { shape[1][3], i, k });
                        }
                    }

                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            clues.Add(new int[] { shape[1][x], shape[0][y], k });
                        }
                    }

                    answer = "Jellyfish по" + (k + 1) +
                             " в (" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + "-" + (shape[1][3] + 1) + ";"
                                    + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + "-" + (shape[0][3] + 1) + ")";
                    return answer;

                }

            }


            return answer;
        }

        //скрытые четверки
        private static string HiddenQuads(Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenQuadsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenQuadsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenQuadsInGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые четверки в группе
        private static string HiddenQuadsInGroup(Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {

                        matrix[j][i] = 1;
                    }
                }
            }

            int[][] shape = Shape4InMatrix(matrix);

            //числа в строках
            //ячейки в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                    {
                        group[shape[1][0]].RemoveCandidat(i + 1);
                        group[shape[1][1]].RemoveCandidat(i + 1);
                        group[shape[1][2]].RemoveCandidat(i + 1);
                        group[shape[1][3]].RemoveCandidat(i + 1);

                        removed.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, i });
                        removed.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, i });
                        removed.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, i });
                        removed.Add(new int[] { group[shape[1][3]].row, group[shape[1][3]].column, i });
                    }
                }
                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][3]].row, group[shape[1][3]].column, shape[0][x] });
                }

                answer = "Скрытая четверка " + (shape[0][0] + 1) + "/" + (shape[0][1] + 1) + "/" + (shape[0][2] + 1) + "/" + (shape[0][3] + 1) +
                         " в (" + (group[shape[1][0]].row + 1) + ";" + (group[shape[1][0]].column + 1) +
                         ") и (" + (group[shape[1][1]].row + 1) + ";" + (group[shape[1][1]].column + 1) +
                         ") и (" + (group[shape[1][2]].row + 1) + ";" + (group[shape[1][2]].column + 1) +
                         ") и (" + (group[shape[1][3]].row + 1) + ";" + (group[shape[1][3]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //открытые четверки
        private static string NakedQuads(Field field)
        {
            string answer = "";

            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = NakedQuadsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = NakedQuadsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = NakedQuadsInGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //открытые тройки в группе
        private static string NakedQuadsInGroup(Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {

                        matrix[i][j] = 1;
                    }
                }
            }

            int[][] shape = Shape4InMatrix(matrix);

            //[0] ячейки
            //[1] кандидаты

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2] && i != shape[0][3])
                    {
                        group[i].RemoveCandidat(shape[1][0] + 1);
                        group[i].RemoveCandidat(shape[1][1] + 1);
                        group[i].RemoveCandidat(shape[1][2] + 1);
                        group[i].RemoveCandidat(shape[1][3] + 1);

                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][0] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][1] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][2] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][3] });
                    }
                }

                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][2]].row, group[shape[0][2]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][3]].row, group[shape[0][3]].column, shape[1][x] });
                }

                answer = "Открытая четверка " + (shape[1][0] + 1) + "/" + (shape[1][1] + 1) + "/" + (shape[1][2] + 1) + "/" + (shape[1][3] + 1) +
                         " в (" + (group[shape[0][0]].row + 1) + ";" + (group[shape[0][0]].column + 1) +
                         ") и (" + (group[shape[0][1]].row + 1) + ";" + (group[shape[0][1]].column + 1) +
                         ") и (" + (group[shape[0][2]].row + 1) + ";" + (group[shape[0][2]].column + 1) +
                         ") и (" + (group[shape[0][3]].row + 1) + ";" + (group[shape[0][3]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //нахождение фигуры 4х4 в строках
        private static int[][] Shape4InMatrix(int[][] a)
        {
            //для четверок n=4
            int n = 4;
            int[] rows = new int[n];
            int[] colums = new int[n];

            for (int i = 0; i < n; i++)
            {
                rows[i] = -1;
                colums[i] = -1;
            }

            int[][] answer = new int[][] { rows, colums };
            //считаю колличество строк в которые n вхождений
            int[] sums = new int[a.Length];
            int counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (a[i][j] != 0)
                    {
                        sums[i]++;
                    }
                }
                if (sums[i] > 1 && sums[i] <= n)
                {
                    counter++;
                }
            }
            //если таких строк меньше n то нужной комбинации нет
            if (counter < n)
            {
                return null;
            }

            //запоминаю номера нужных строк
            int[] digits = new int[counter];
            counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (sums[i] > 1 && sums[i] <= n)
                {
                    digits[counter] = i;
                    counter++;
                }
            }

            counter = 0;
            sums = new int[a.Length];
            if (digits.Length > 1)
            {
                //перебираю все комбинации нужных строк
                for (int k = 0; k < digits.Length; k++)
                {
                    for (int l = k + 1; l < digits.Length; l++)
                    {
                        for (int h = l + 1; h < digits.Length; h++)
                        {
                            for (int g = h + 1; g < digits.Length; g++)
                            {
                                sums = new int[a.Length];
                                //подсчитываю колличество вхождений в выбранные строки
                                for (int j = 0; j < a.Length; j++)
                                {
                                    if (a[digits[k]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                    if (a[digits[l]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                    if (a[digits[h]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                    if (a[digits[g]][j] > 0)
                                    {
                                        sums[j]++;
                                    }
                                }
                                counter = 0;
                                for (int j = 0; j < a.Length; j++)
                                {
                                    if (sums[j] > 0)
                                    {
                                        counter++;
                                    }
                                }

                                if (counter != n)
                                {
                                    continue;
                                }

                                //запоминаю найденную фигуру
                                rows[0] = digits[k];
                                rows[1] = digits[l];
                                rows[2] = digits[h];
                                rows[3] = digits[g];

                                counter = 0;

                                for (int j = 0; j < a.Length; j++)
                                {
                                    if (sums[j] > 1 && sums[j] <= n)
                                    {
                                        colums[counter] = j;
                                        counter++;
                                    }
                                }

                                //смотрю есть ли исключения
                                for (int j = 0; j < n; j++)
                                {
                                    for (int i = 0; i < a.Length; i++)
                                    {
                                        if (i != rows[0] && i != rows[1] && i != rows[2] && i != rows[3] && a[i][colums[j]] > 0)
                                        {
                                            return new int[][] { rows, colums };
                                        }
                                    }
                                }

                            }
                        }
                    }

                }
            }

            return null;
        }

        //Swordfish
        private static string Swordfish(Field field)
        {
            string answer = "";

            int[][] matrix = new int[9][];
            int[][] shape;
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }
            //по всем числам
            for (int k = 0; k < 9; k++)
            {
                //смотрю строки
                //исключаю в колонках

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[i][j] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 0;
                        }
                    }
                }

                shape = Shape3InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] строки
                //[1] колонки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                        {
                            field[i, shape[1][0]].RemoveCandidat(k + 1);
                            field[i, shape[1][1]].RemoveCandidat(k + 1);
                            field[i, shape[1][2]].RemoveCandidat(k + 1);

                            removed.Add(new int[] { i, shape[1][0], k });
                            removed.Add(new int[] { i, shape[1][1], k });
                            removed.Add(new int[] { i, shape[1][2], k });
                        }
                    }

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            clues.Add(new int[] { shape[0][x], shape[1][y], k });
                        }
                    }

                    answer = "Swordfish по " + (k + 1) +
                            " в (" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + ";"
                                   + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + ")"
                            ;
                    return answer;

                }


                //смотрю столбцы
                //исключаю строки

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[j][i] = 0;
                        }
                    }
                }

                shape = Shape3InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] колонки
                //[1] строки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                        {
                            field[shape[1][0], i].RemoveCandidat(k + 1);
                            field[shape[1][1], i].RemoveCandidat(k + 1);
                            field[shape[1][2], i].RemoveCandidat(k + 1);

                            removed.Add(new int[] { shape[1][0], i, k });
                            removed.Add(new int[] { shape[1][1], i, k });
                            removed.Add(new int[] { shape[1][2], i, k });
                        }
                    }

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            clues.Add(new int[] { shape[1][x], shape[0][y], k });
                        }
                    }

                    answer = "Swordfish по" + (k + 1) +
                             " в (" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + "-" + (shape[1][2] + 1) + ";"
                                    + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + "-" + (shape[0][2] + 1) + ")";
                    return answer;

                }

            }


            return answer;
        }

        //скрытые тройки
        private static string HiddenTriples(Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenTriplesInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenTriplesInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenTriplesInGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые пары в группе
        private static string HiddenTriplesInGroup(Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {

                        matrix[j][i] = 1;
                    }
                }
            }

            int[][] shape = Shape3InMatrix(matrix);

            //числа в строках
            //ячейки в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                    {
                        group[shape[1][0]].RemoveCandidat(i + 1);
                        group[shape[1][1]].RemoveCandidat(i + 1);
                        group[shape[1][2]].RemoveCandidat(i + 1);

                        removed.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, i });
                        removed.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, i });
                        removed.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, i });
                    }
                }

                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][x] });
                    clues.Add(new int[] { group[shape[1][2]].row, group[shape[1][2]].column, shape[0][x] });
                }


                answer = "Скрытая тройка " + (shape[0][0] + 1) + "/" + (shape[0][1] + 1) + "/" + (shape[0][2] + 1) +
                         " в (" + (group[shape[1][0]].row + 1) + ";" + (group[shape[1][0]].column + 1) +
                         ") и (" + (group[shape[1][1]].row + 1) + ";" + (group[shape[1][1]].column + 1) +
                         ") и (" + (group[shape[1][2]].row + 1) + ";" + (group[shape[1][2]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //открытые тройки
        private static string NakedTriples(Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = NakedTripesInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = NakedTripesInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = NakedTripesInGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //открытые тройки в группе
        private static string NakedTripesInGroup(Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {

                        matrix[i][j] = 1;
                    }
                }
            }

            int[][] shape = Shape3InMatrix(matrix);

            //ячейки в строках
            //кандидаты в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1] && i != shape[0][2])
                    {
                        group[i].RemoveCandidat(shape[1][0] + 1);
                        group[i].RemoveCandidat(shape[1][1] + 1);
                        group[i].RemoveCandidat(shape[1][2] + 1);

                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][0] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][1] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][2] });
                    }
                }

                for (int x = 0; x < shape[1].Length; x++)
                {
                    clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][x] });
                    clues.Add(new int[] { group[shape[0][2]].row, group[shape[0][2]].column, shape[1][x] });
                }

                answer = "Открытая тройка " + (shape[1][0] + 1) + "/" + (shape[1][1] + 1) + "/" + (shape[1][2] + 1) +
                         " в (" + (group[shape[0][0]].row + 1) + ";" + (group[shape[0][0]].column + 1) +
                         ") и (" + (group[shape[0][1]].row + 1) + ";" + (group[shape[0][1]].column + 1) +
                         ") и (" + (group[shape[0][2]].row + 1) + ";" + (group[shape[0][2]].column + 1) + ")"
                         ;
            }


            return answer;
        }

        //нахождение фигуры 3х3 в строках
        private static int[][] Shape3InMatrix(int[][] a)
        {
            //для троек n=2
            int n = 3;
            int[] rows = new int[n];
            int[] colums = new int[n];

            for (int i = 0; i < n; i++)
            {
                rows[i] = -1;
                colums[i] = -1;
            }

            int[][] answer = new int[][] { rows, colums };
            //считаю колличество строк в которые n вхождений
            int[] sums = new int[a.Length];
            int counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (a[i][j] != 0)
                    {
                        sums[i]++;
                    }
                }
                if (sums[i] > 1 && sums[i] <= n)
                {
                    counter++;
                }
            }
            //если таких строк меньше n то нужной комбинации нет
            if (counter < n)
            {
                return null;
            }

            //запоминаю номера нужных строк
            int[] digits = new int[counter];
            counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (sums[i] > 1 && sums[i] <= n)
                {
                    digits[counter] = i;
                    counter++;
                }
            }

            counter = 0;
            sums = new int[a.Length];
            if (digits.Length > 1)
            {
                //перебираю все комбинации нужных строк
                for (int k = 0; k < digits.Length; k++)
                {
                    for (int l = k + 1; l < digits.Length; l++)
                    {
                        for (int h = l + 1; h < digits.Length; h++)
                        {
                            sums = new int[a.Length];
                            //подсчитываю колличество вхождений в выбранные строки
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (a[digits[k]][j] > 0)
                                {
                                    sums[j]++;
                                }
                                if (a[digits[l]][j] > 0)
                                {
                                    sums[j]++;
                                }
                                if (a[digits[h]][j] > 0)
                                {
                                    sums[j]++;
                                }
                            }
                            counter = 0;
                            for (int j = 0; j < a.Length; j++)
                            {
                                if (sums[j] > 0)
                                {
                                    counter++;
                                }
                            }

                            if (counter != n)
                            {
                                continue;
                            }

                            //запоминаю найденную фигуру
                            rows[0] = digits[k];
                            rows[1] = digits[l];
                            rows[2] = digits[h];
                            counter = 0;

                            for (int j = 0; j < a.Length; j++)
                            {
                                if (sums[j] > 1 && sums[j] <= n)
                                {
                                    colums[counter] = j;
                                    counter++;
                                }
                            }

                            //смотрю есть ли исключения
                            for (int j = 0; j < n; j++)
                            {
                                for (int i = 0; i < a.Length; i++)
                                {
                                    if (i != rows[0] && i != rows[1] && i != rows[2] && a[i][colums[j]] > 0)
                                    {
                                        return new int[][] { rows, colums };
                                    }
                                }
                            }

                        }
                    }

                }
            }

            return null;
        }

        //X-Wings
        private static string X_Wings(Field field)
        {
            string answer = "";

            int[][] matrix = new int[9][];
            int[][] shape;
            for (int i = 0; i < 9; i++)
            {
                matrix[i] = new int[9];
            }
            //по всем числам
            for (int k = 0; k < 9; k++)
            {
                //смотрю строки
                //исключаю в колонках

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[i][j] = 1;
                        }
                        else
                        {
                            matrix[i][j] = 0;
                        }
                    }
                }

                shape = Shape2InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] строки
                //[1] колонки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1])
                        {
                            field[i, shape[1][0]].RemoveCandidat(k + 1);
                            field[i, shape[1][1]].RemoveCandidat(k + 1);

                            removed.Add(new int[] { i, shape[1][0], k });
                            removed.Add(new int[] { i, shape[1][1], k });
                        }
                    }
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            clues.Add(new int[] { shape[0][x], shape[1][y], k });
                        }
                    }
                    answer = "X-Wings по " + (k + 1) + " в (" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) +
                             ";" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) + ")";
                    return answer;

                }


                //смотрю столбцы
                //исключаю строки

                //составляю матрицу кандидатов
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (field[i, j].candidates[k])
                        {
                            matrix[j][i] = 1;
                        }
                        else
                        {
                            matrix[j][i] = 0;
                        }
                    }
                }

                shape = Shape2InMatrix(matrix);

                //если нашлось то провожу исключения и составляю ответ
                //[0] колонки
                //[1] строки
                if (shape != null)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != shape[0][0] && i != shape[0][1])
                        {
                            field[shape[1][0], i].RemoveCandidat(k + 1);
                            field[shape[1][1], i].RemoveCandidat(k + 1);

                            removed.Add(new int[] { shape[1][0], i, k });
                            removed.Add(new int[] { shape[1][1], i, k });
                        }
                    }

                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            clues.Add(new int[] { shape[1][x], shape[0][y], k });
                        }
                    }

                    answer = "X-Wings по" + (k + 1) + " в (" + (shape[1][0] + 1) + "-" + (shape[1][1] + 1) +
                             ";" + (shape[0][0] + 1) + "-" + (shape[0][1] + 1) + ")";
                    return answer;

                }

            }


            return answer;
        }

        //скрытые пары
        private static string HiddenPairs(Field field)
        {
            string answer = "";


            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenPairsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenPairsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenPairsInGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые пары в группе

        private static string HiddenPairsInGroup(Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {

                        matrix[j][i] = 1;
                    }
                }
            }


            int[][] shape = Shape2InMatrix(matrix);
            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1])
                    {
                        group[shape[1][0]].RemoveCandidat(i + 1);
                        group[shape[1][1]].RemoveCandidat(i + 1);

                        removed.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, i });
                        removed.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, i });
                    }
                }

                clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][0] });
                clues.Add(new int[] { group[shape[1][0]].row, group[shape[1][0]].column, shape[0][1] });
                clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][0] });
                clues.Add(new int[] { group[shape[1][1]].row, group[shape[1][1]].column, shape[0][1] });

                answer = "Скрытая пара " + (shape[0][0] + 1) + "/" + (shape[0][1] + 1) +
                         " в (" + (group[shape[1][0]].row + 1) + ";" + (group[shape[1][0]].column + 1) +
                         ") и (" + (group[shape[1][1]].row + 1) + ";" + (group[shape[1][1]].column + 1) + ")"
                         ;
            }

            return answer;
        }

        //нахождение фигуры 2х2 в строках
        private static int[][] Shape2InMatrix(int[][] a)
        {
            //для пар n=2
            int n = 2;
            int[] rows = new int[n];
            int[] colums = new int[n];

            for (int i = 0; i < n; i++)
            {
                rows[i] = -1;
                colums[i] = -1;
            }

            int[][] answer = new int[][] { rows, colums };
            //считаю колличество строк в которые n вхождений
            int[] sums = new int[a.Length];
            int counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (a[i][j] != 0)
                    {
                        sums[i]++;
                    }
                }
                if (sums[i] > 1 && sums[i] <= n)
                {
                    counter++;
                }
            }
            //если таких строк меньше n то нужной комбинации нет
            if (counter < n)
            {
                return null;
            }

            //запоминаю номера нужных строк
            int[] digits = new int[counter];
            counter = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (sums[i] > 1 && sums[i] <= n)
                {
                    digits[counter] = i;
                    counter++;
                }
            }

            counter = 0;
            sums = new int[a.Length];
            if (digits.Length > 1)
            {
                //перебираю все комбинации нужных строк
                for (int k = 0; k < digits.Length; k++)
                {
                    for (int l = k + 1; l < digits.Length; l++)
                    {
                        sums = new int[a.Length];
                        //подсчитываю колличество вхождений в выбранные строки
                        for (int j = 0; j < a.Length; j++)
                        {
                            if (a[digits[k]][j] > 0)
                            {
                                sums[j]++;
                            }
                            if (a[digits[l]][j] > 0)
                            {
                                sums[j]++;
                            }
                        }
                        counter = 0;
                        for (int j = 0; j < a.Length; j++)
                        {
                            if (sums[j] > 1 && sums[j] <= n)
                            {
                                counter++;
                            }
                        }

                        if (counter < n)
                        {
                            continue;
                        }

                        //запоминаю найденную фигуру
                        rows[0] = digits[k];
                        rows[1] = digits[l];
                        counter = 0;

                        for (int j = 0; j < a.Length; j++)
                        {
                            if (sums[j] > 1 && sums[j] <= n)
                            {
                                colums[counter] = j;
                                counter++;
                            }
                        }

                        //смотрю есть ли исключения
                        for (int j = 0; j < n; j++)
                        {
                            for (int i = 0; i < a.Length; i++)
                            {
                                if (i != rows[0] && i != rows[1] && a[i][colums[j]] > 0)
                                {
                                    return new int[][] { rows, colums };
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        //виртуальные одиночки
        private static string VirtualSingle(Field field)
        {
            string answer = "";
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
                                            field[startY + y, startX + x].RemoveCandidat(k + 1);
                                            impact = true;


                                            removed.Add(new int[] { startY + y, startX + x, k });
                                        }
                                        if (impact)
                                        {
                                            for (int n = 0; n < cluesInd.Length; n++)
                                            {
                                                clues.Add(new int[] { i, cluesInd[n], k });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //если найдено исключение то формируем ответ
                        if (impact)
                        {
                            answer = "Cтрока  " + (i + 1) + ": виртуальная одиночка " + (k + 1) + " в регионе " + (3 * startY / 3 + startX / 3 + 1)
                                + "";
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
                                            field[startY + y, startX + x].RemoveCandidat(k + 1);
                                            impact = true;

                                            removed.Add(new int[] { startY + y, startX + x, k });
                                        }
                                        if (impact)
                                        {
                                            for (int n = 0; n < cluesInd.Length; n++)
                                            {
                                                clues.Add(new int[] { cluesInd[n], j, k });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //если найдено исключение то формируем ответ
                        if (impact)
                        {
                            answer = "Cтолбец " + (j + 1) + ": виртуальная одиночка " + (k + 1) + " в регионе " + (3 * startY / 3 + startX / 3 + 1)
                                + "";
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
                                            field[indexes[0], n].RemoveCandidat(k + 1);
                                            impact = true;

                                            removed.Add(new int[] { indexes[0], n, k });
                                        }
                                    }
                                }
                            }
                            //если было исключение то формируем ответ
                            if (impact)
                            {
                                for (int n = 0; n < cluesInd.Length; n++)
                                {
                                    clues.Add(new int[] { indexes[0], cluesInd[n], k });
                                }
                                answer = "Регион  " + (y * 3 + x + 1) + ": виртуальная одиночка " + (k + 1) + " в строке " + (indexes[0] + 1) + "";
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
                                            field[n, indexes[0]].RemoveCandidat(k + 1);
                                            impact = true;

                                            removed.Add(new int[] { n, indexes[0], k });
                                        }
                                    }
                                }
                            }
                            //если было исключение то формируем ответ
                            if (impact)
                            {
                                for (int n = 0; n < cluesInd.Length; n++)
                                {
                                    clues.Add(new int[] { cluesInd[n], indexes[0], k });
                                }
                                answer = "Регион " + (y * 3 + x + 1) + ": виртуальная одиночка " + (k + 1) + " в столбце " + (indexes[0] + 1) + "";
                                return answer;
                            }

                        }

                    }
                }
            }
            return answer;
        }

        //открытые пары
        private static string NakedPairs(Field field)
        {

            string answer = "";

            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = NakedPairsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = NakedPairsInGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = NakedPairsInGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + ": " + answer;
                        return answer;
                    }
                }
            }
            return answer;
        }

        //способ на матрицах для масштабирования
        private static string NakedPairsInGroup(Field.Cell[] group)
        {
            string answer = "";

            //составляю и заполняю матрицу вхождений
            int[][] matrix = new int[group.Length][];
            for (int i = 0; i < group.Length; i++)
            {
                matrix[i] = new int[group.Length];
            }
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = 0; j < group.Length; j++)
                {
                    if (group[i].candidates[j])
                    {

                        matrix[i][j] = 1;
                    }
                }
            }


            int[][] shape = Shape2InMatrix(matrix);

            //ячейки в строках
            //кандидаты в колонках

            //если была найдена фигура то проводим исключения и формируем ответ
            if (shape != null)
            {
                for (int i = 0; i < 9; i++)
                {
                    if (i != shape[0][0] && i != shape[0][1])
                    {
                        group[i].RemoveCandidat(shape[1][0] + 1);
                        group[i].RemoveCandidat(shape[1][1] + 1);

                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][0] });
                        removed.Add(new int[] { group[i].row, group[i].column, shape[1][1] });
                    }
                }

                clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][0] });
                clues.Add(new int[] { group[shape[0][0]].row, group[shape[0][0]].column, shape[1][1] });
                clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][0] });
                clues.Add(new int[] { group[shape[0][1]].row, group[shape[0][1]].column, shape[1][1] });

                answer = "Открытая пара " + (shape[1][0] + 1) + "/" + (shape[1][1] + 1) +
                         " в (" + (group[shape[0][0]].row + 1) + ";" + (group[shape[0][0]].column + 1) +
                         ") и (" + (group[shape[0][1]].row + 1) + ";" + (group[shape[0][1]].column + 1) + ")"
                         ;
            }

            return answer;
        }

        //скрытые одиночки
        private static string HiddenSingle(Field field)
        {
            string answer = "";

            //места для группы
            Field.Cell[] group = new Field.Cell[9];
            for (int i = 0; i < 9; i++)
            {
                group[i] = new Field.Cell(0, 0);
            }


            //строки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[i][j];
                }
                answer = HiddenSingleGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Строка  " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //столбцы
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    group[j] = field.cells[j][i];
                }
                answer = HiddenSingleGroup(group);
                if (!answer.Equals(""))
                {
                    answer = "Столбец " + (i + 1).ToString() + ": " + answer;
                    return answer;
                }
            }

            //регионы
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //i,j индексы региона
                    //x,y, индексы внутри региона

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            group[y * 3 + x] = field.cells[i * 3 + x][j * 3 + y];
                        }
                    }
                    answer = HiddenSingleGroup(group);
                    if (!answer.Equals(""))
                    {
                        answer = "Регион  " + ((i) * 3 + j + 1).ToString() + ": " + answer;
                        return answer;
                    }
                }
            }


            return answer;
        }

        //скрытые одиночки в группе
        private static string HiddenSingleGroup(Field.Cell[] group)
        {
            string answer = "";

            int count = 0;
            int index = -1;
            int v = 0;
            for (int k = 0; k < 9; k++)
            {
                count = 0;
                for (int i = 0; i < 9; i++)
                {
                    if (group[i].candidates[k])
                    {
                        count++;
                        index = i;
                    }
                }
                if (count == 1)
                {
                    v = k + 1;
                    group[index].SetValue(v);
                    clues.Add(new int[] { group[index].row, group[index].column, v - 1 });
                    answer = "Найдена скрытая одиночка: " + v.ToString() + " в (" + (group[index].row + 1).ToString() + ";" + (group[index].column + 1).ToString() + ")";
                    return answer;
                }
            }


            return answer;
        }

        //открытые одиночки
        private static string NakedSingle(Field field)
        {
            string answer = "";
            //обход всех ячеек
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //подсчет колличества кандидатов
                    int count = 0;
                    if (field[i, j].value == -1)
                    {
                        for (int k = 0; k < 9; k++)
                        {
                            if (field[i, j].candidates[k])
                            {
                                count++;
                            }
                        }
                    }
                    //если остался один кандидат, выставляем значение
                    if (count == 1)
                    {
                        int v = 0;
                        for (int k = 0; k < 9; k++)
                        {
                            if (field[i, j].candidates[k])
                            {
                                v = k + 1;
                            }
                        }

                        field[i, j].SetValue(v);

                        clues.Add(new int[] { i, j, v - 1 });

                        answer = "Найдена открытая одиночка: " + v.ToString() + " в (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")";
                        return answer;
                    }
                }
            }
            return answer;
        }

        //простые ислючения
        //способ на списке
        public static void SimpleRestriction(Field field)
        {
            int value = 0;
            //обход всего поля
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    value = field[i, j].value;
                    //если известно число
                    if (value > 0)
                    {
                        //обходим все видимые ячейки
                        for (int k = 0; k < field[i, j].seenCell.Length; k++)
                        {

                            //если есть исключение то исключаем
                            if (field[i, j].seenCell[k].candidates[value - 1])
                            {
                                field[i, j].seenCell[k].RemoveCandidat(value);
                            }
                        }
                    }
                }
            }
        }

        //проверка
        private static string check(Field field)
        {
            string answer = "";

            //проверка на решение
            bool full = true;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].value < 1)
                    {
                        full = false;
                    }
                }
            }
            if (full)
            {
                answer = "Судоку решено!";
                done = true;
                return answer;
            }

            //проверка на ошибки
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (field[i, j].value != -1)
                    {
                        //обход по столбцу
                        for (int x = 0; x < 9; x++)
                        {
                            if (x != i & field[x, j].value == field[i, j].value)
                            {
                                answer = "Ошибка! Совпадение в столбце " + (j + 1).ToString() + ": (" + (x + 1).ToString() + ";" + (j + 1).ToString() + ") и (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")!";
                                done = false;
                                return answer;
                            }
                        }
                        //обход по строке
                        for (int x = 0; x < 9; x++)
                        {
                            if (x != j & field[i, x].value == field[i, j].value)
                            {
                                answer = "Ошибка! Совпадение в строке " + (i + 1).ToString() + ": (" + (i + 1).ToString() + ";" + (x + 1).ToString() + ") и (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")!";
                                done = false;
                                return answer;
                            }
                        }
                        //обход в региону
                        //левый верхний угол региона
                        int indX = 3 * (i / 3);
                        int indY = 3 * (j / 3);

                        for (int x = 0; x < 3; x++)
                        {
                            for (int y = 0; y < 3; y++)
                            {
                                if ((indX + x) != i & (indY + y) != j & field[(indX + x), (indY + y)].value == field[i, j].value)
                                {
                                    answer = "Ошибка! Совпадение в регионе " + (indY * 3 + indX + 1).ToString() + ": (" + ((indX + x) + 1).ToString() + ";" + ((indY + y) + 1).ToString() + ") и (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")!";
                                    done = false;
                                    return answer;
                                }
                            }
                        }
                    }
                }
            }


            return answer;
        }

        public static string makeAnswer(int i, int j, int v,Field field)
        {
            return "Исключена " + v.ToString() + " из ячейки (" + (i + 1).ToString() + ";" + (j + 1).ToString() + ")";
        }
    }
}
