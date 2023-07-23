namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class HiddenPairsChecker : MatrixTechChecker
    {
        public override TechType Type => TechType.HiddenPairs;

        protected override int Order => 2;

        protected override bool IsHidden => true;

        protected override Splitter[] Splitters => new Splitter[3] 
        { 
            FieldScanner.SplitFieldByRows,
            FieldScanner.SplitFieldByColums, 
            FieldScanner.SplitFieldByRegions 
        };

        protected override string[] GroupDescriptions => new string[3] { "Строка", "Столбец", "Регион" };


        private FieldScanner FieldScanner => new FieldScanner();
    }
}
