using SolverLibrary.model.field;
using System.Text.Json.Serialization;

namespace SolverLibrary.storage
{
    internal class Puzzle
    {

        [JsonInclude]
        public string Discription;

        [JsonInclude]
        public int[][] sudoku;

    }
}
