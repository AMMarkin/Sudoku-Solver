using System.Collections.Generic;

namespace SolverLibrary.model
{
    public class AnswerOfTech
    {
        private string _message;

        private IEnumerable<Mark> _clues;
        private IEnumerable<Mark> _removed;
        private IEnumerable<Change> _eliminations;
        private IEnumerable<Change> _seted;
        private IEnumerable<int[]> _chain;
        private IEnumerable<int[]> _weak;
        private IEnumerable<int[]> _chainUnits;
        private IEnumerable<int[]> _ON;
        private IEnumerable<int[]> _OFF;

        public string Message { get => _message; set => _message = value; }
        public IEnumerable<Mark> Clues { get => _clues; set => _clues = value; }
        public IEnumerable<Mark> Removed { get => _removed; set => _removed = value; }
        public IEnumerable<Change> Eliminations { get => _eliminations; set => _eliminations = value; }
        public IEnumerable<Change> Seted { get => _seted; set => _seted = value; }
        public IEnumerable<int[]> Chain { get => _chain; set => _chain = value; }
        public IEnumerable<int[]> Weak { get => _weak; set => _weak = value; }
        public IEnumerable<int[]> ChainUnits { get => _chainUnits; set => _chainUnits = value; }
        public IEnumerable<int[]> ON { get => _ON; set => _ON = value; }
        public IEnumerable<int[]> OFF { get => _OFF; set => _OFF = value; }

        public AnswerOfTech(string message)
        {
            _message = message;
        }
    }
}
