using System;

namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class JellyfishChecker : LockedRectangleChecker
    {
        public override TechType Type => TechType.Jellyfish;

        protected override int Order => 4;

    }
}
