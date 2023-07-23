using SolverLibrary.model.field;
using System.Collections.Generic;

namespace SolverLibrary.model
{
    internal abstract class TechChecker
    {
        public abstract TechType Type { get; }

        protected readonly List<Change> _seted = new List<Change>();
        protected readonly List<Change> _eliminations = new List<Change>();

        protected readonly List<Mark> _clues = new List<Mark>();
        protected readonly List<Mark> _removed = new List<Mark>();

        protected virtual string Discription => TechsList.ConvertTypeToName(Type);


        public virtual AnswerOfTech CheckField(Field field)
        {
            ClearLists();
            return FindElimination(field);
        }

        protected abstract AnswerOfTech FindElimination(Field field);

        protected void AddSetingValue(int ind, int digit)
        {
            _seted.Add(new Change(ind, digit, ChangeType.SettingValue));
            _eliminations.Add(new Change(ind, digit, ChangeType.RemovingDigit));
        }

        protected void AddElimination(int ind, int digit)
        {
            _eliminations.Add(new Change(ind, digit, ChangeType.RemovingDigit));
        }

        protected void AddClueMark(int ind, int digit = -1)
        {
            _clues.Add(new Mark(ind, digit));
        }

        protected void AddRemovingMark(int ind, int digit)
        {
            _removed.Add(new Mark(ind, digit));
        }

        protected virtual AnswerOfTech MakeAnswer(string message)
        {
            AnswerOfTech answer = new AnswerOfTech(message);
            SetLists(answer);
            return answer;
        }

        protected virtual void SetLists(AnswerOfTech answer)
        {
            answer.Seted = _seted;
            answer.Eliminations = _eliminations;

            answer.Clues = _clues;
            answer.Removed = _removed;
        }

        protected virtual void ClearLists()
        {
            _seted.Clear();
            _eliminations.Clear();

            _clues.Clear();
            _removed.Clear();
        }

    }
}
