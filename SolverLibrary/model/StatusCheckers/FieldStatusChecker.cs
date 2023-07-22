using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model
{
    internal abstract class FieldStatusChecker
    {
        public abstract AnswerOfTech Check(Field field);
    }
}
