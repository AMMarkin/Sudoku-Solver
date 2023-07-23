namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class X_WingsChecker : LockedRectangleChecker
    {
        public override TechType Type => TechType.X_Wings;

        protected override int Order => 2;

    }
}
