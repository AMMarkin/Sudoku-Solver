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

        public void Init()
        {
            TechCheckerBuilder builder = new TechCheckerBuilder();

            foreach(string techName in tecniques) 
            { 
                TechChecker techChecker = builder.GetTechChecker(TechsList.ConvertNameToType(techName));
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

            //техники
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

            //проверка решения
            foreach (FieldStatusChecker cheker in statusCheckers)
            {
                answer = cheker.Check(field);
                if (answer != null)
                {
                    return answer;
                }
            }

            return new AnswerOfTech(noFound);
        }

        //простые ислючения
        public AnswerOfTech SimpleRestriction(Field field)
        {
            return techCheckers.Find((x) => x.Type == TechType.SimpleRestriction).CheckField(field);
        }
    }
}
