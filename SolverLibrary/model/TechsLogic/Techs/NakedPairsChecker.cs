using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class NakedPairsChecker : MatrixTechChecker
    {
        public override TechType Type => TechType.NakedPairs;

        protected override int Order => 2;

        protected override bool IsHidden => false;

        protected override Splitter[] Splitters => new Splitter[3] 
        { 
            FieldScanner.SplitFieldByRows, 
            FieldScanner.SplitFieldByColums,
            FieldScanner.SplitFieldByRegions
        };

        protected override string[] GroupDescriptions => new string[3] { "Строка", "Столбец", "Регион" };

        protected override string Discription => "Открытая пара";

        private FieldScanner FieldScanner => new FieldScanner();
    }
}
