using System.Collections.Generic;

namespace SolverLibrary.model
{
    internal static class TechsList
    {
        public static readonly List<string> techNames = new List<string>(){ 
            "Простые исключения",
            "Открытые одиночки", "Скрытые одиночки", "Виртуальные одиночки",
            "Открытые пары", "Cкрытые пары",
            "Открытые тройки", "Скрытые тройки",
            "Открытые четверки", "Скрытые четверки",
            "BUG",
            "X-Wings", "Swordfish", "Jellyfish",
            "Y-Wings","XYZ-Wing",
            "Simple Coloring", "Extended Simple Coloring"
        };

        public static TechType ConvertNameToType(string name)
        {
            for(int i = 0; i < techNames.Count; i++)
            {
                if (techNames[i].Equals(name))
                {
                    return (TechType)i;
                }
            }
            return TechType.None;
        }
    }

    public enum TechType
    {
        SimpleRestriction,
        NakedSingle, HiddenSingle, VirtualSingle,
        NakedPairs, HiddenPairs,
        NakedTriples, HiddenTriples,
        NakedQuads, HiddenQuads,
        BUG,
        X_Wings, Swordfish, Jellyfish,
        XY_Wing, XYZ_Wing,
        SimpleColoring, ExtendedSimpleColoring,

        None
    }
}
