using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model
{
    public class AnswerOfTech
    {
        private string _message;

        private IEnumerable<Mark> _clues;

        private IEnumerable<Mark> _removed;

        private IEnumerable<Change> _eliminations;

        private IEnumerable<Change> _seted;

        public string Message { get => _message; set => _message = value; }

        public IEnumerable<Mark> Clues { get => _clues; set => _clues = value; }

        public IEnumerable<Mark> Removed { get => _removed; set => _removed = value; }

        public IEnumerable<Change> Eliminations { get => _eliminations; set => _eliminations = value; }

        public IEnumerable<Change> Seted { get => _seted; set => _seted = value; }

        public AnswerOfTech(string message)
        {
            _message = message;
        }
    }
}
