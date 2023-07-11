namespace SolverLibrary.Interfaces
{
    public interface ISolverView
    {
        IController Controller { get; set; }

        void PrintToConsole(string output);

        void ClearConsole();

    }
}
