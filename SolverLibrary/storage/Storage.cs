using System.Collections.Generic;

namespace SolverLibrary.storage
{
    public abstract class Storage
    {

        public abstract void Save(int[][] puzzle, string name);

        public abstract (int[][], StorageOperationResult) Load(string name);

        public abstract List<string> GetListOfPuzzles();

        public abstract StorageOperationResult Delete(string[] names);

    }

    public enum StorageOperationResult
    {
        Success,
        Failed
    }
}
