namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class NakedTriplesChecker : MatrixTechChecker
    {
        public override TechType Type => TechType.NakedTriples;

        protected override int Order => 3;

        protected override bool IsHidden => false;

        protected override Splitter[] Splitters => new Splitter[3] { SplitFieldByRows, SplitFieldByColums, SplitFieldByRegions };

        protected override string[] GroupDescriptions => new string[3] { "Строка", "Столбец", "Регион" };

        protected override string Discription => "Открытая тройка";
    }
}
