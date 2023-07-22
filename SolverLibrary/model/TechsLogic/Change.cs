namespace SolverLibrary.model
{
    public class Change
    {
        private readonly int _i;
        private readonly int _j;
        private readonly int _ind;
        private readonly int _digit;
        private readonly ChangeType _type;

        public int I => _i;
        public int J => _j;
        public int Ind => _ind;
        public int Digit => _digit;
        public ChangeType Type => _type;


        public Change(int i, int j, int digit, ChangeType type)
        {
            _i = i;
            _j = j;
            _ind = 9 * i + j;
            _digit = digit;
            _type = type;
        }

        public Change(int ind, int digit, ChangeType type)
        {
            _ind = ind;
            _i = ind / 9;
            _j = ind % 9;
            _digit = digit;
            _type = type;
        }
    }

    public enum ChangeType
    {
        RemovingDigit,
        SettingValue
    }

}
