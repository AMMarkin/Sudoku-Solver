namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class HiddenTriplesChecker : MatrixTechChecker
    {
        public override TechType Type => TechType.HiddenTriples;

        protected override int Order => 3;

        protected override bool IsHidden => true;

        protected override Splitter[] Splitters => new Splitter[3] 
        { 
            FieldScanner.SplitFieldByRows, 
            FieldScanner.SplitFieldByColums,
            FieldScanner.SplitFieldByRegions
        };

        protected override string[] GroupDescriptions => new string[3] { "Строка", "Столбец", "Регион" };

        protected override string Discription => "Скрытая тройка";

        private FieldScanner FieldScanner => new FieldScanner();
    }
}
