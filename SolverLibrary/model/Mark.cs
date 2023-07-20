namespace SolverLibrary.model
{
    public class Mark
    {
        private readonly int _i;
        private readonly int _j;
        private readonly int _ind;
        private readonly int _digit;
        private readonly MarkType _type;

        public int I => _i;
        public int J => _j;
        public int Ind => _ind;
        public int Digit => _digit;
        public MarkType Type => _type;

        public enum MarkType{
            Cell,
            Digit
        }


        public Mark(int ind, int digit = -1, MarkType type = MarkType.Cell)
        {
            _i = ind / 9;
            _j = ind % 9;
            _ind = ind;
            _digit = digit;
            if (digit != -1)
                type = MarkType.Digit;
            _type = type;
        }

        
    }
}
