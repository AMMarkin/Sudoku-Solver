namespace SolverLibrary.model.TechsLogic.Techs
{
    internal class TechCheckerBuilder
    {

        public TechChecker GetTechChecker(TechType type)
        {
            switch (type)
            {
                case TechType.SimpleRestriction:
                    return new SimpleRestrictionCheker();
                case TechType.NakedSingle:
                    return new NakedSingleCheker();
                case TechType.HiddenSingle:
                    return new HiddenSinglesChecker();
                case TechType.NakedPairs:
                    return new NakedPairsChecker();
                case TechType.HiddenPairs:
                    return new HiddenPairsChecker();
                case TechType.NakedTriples:
                    return new NakedTriplesChecker();
                case TechType.HiddenTriples:
                    return new HiddenTriplesChecker();
                case TechType.NakedQuads:
                    return new NakedQuadsChecker();
                case TechType.HiddenQuads:
                    return new HiddenQuadsChecker();
                case TechType.X_Wings:
                    return new X_WingsChecker();
                case TechType.Swordfish:
                    return new SwordfishChecker();
                case TechType.Jellyfish:
                    return new JellyfishChecker();

                default: return null;
            }

        }

}
}
