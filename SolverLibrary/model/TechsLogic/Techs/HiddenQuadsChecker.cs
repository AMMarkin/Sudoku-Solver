using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class HiddenQuadsChecker : MatrixTechChecker
    {
        public override TechType Type => TechType.HiddenQuads;

        protected override int Order => 4;

        protected override bool IsHidden => true;

        protected override Splitter[] Splitters => new Splitter[3] { SplitFieldByRows, SplitFieldByColums, SplitFieldByRegions };

        protected override string[] GroupDescriptions => new string[3] { "Строка", "Столбец", "Регион" };

        protected override string Discription => "Скрытая четверка";
    }
}
