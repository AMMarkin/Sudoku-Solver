namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class NakedQuadsChecker : MatrixTechChecker
    {
        public override TechType Type => TechType.NakedQuads;

        protected override int Order => 4;

        protected override bool IsHidden => false;

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
