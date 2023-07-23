namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class SwordfishChecker : LockedRectangleChecker
    {
        public override TechType Type => TechType.Swordfish;

        protected override int Order => 3;

    }
}
