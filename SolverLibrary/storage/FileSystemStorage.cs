using SolverLibrary.model.field;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SolverLibrary.storage
{
    public class FileSystemStorage : Storage
    {
        private string CurrDirectory => Environment.CurrentDirectory;
        private string Dirpath => Directory.GetParent(CurrDirectory).Parent.Parent.FullName + @"\Puzzles\";

        private readonly string _format = ".json";

        public override (int[][], StorageOperationResult) Load(string name)
        {
            string fullpath = Dirpath + name + _format;
            string jsonstring;
            string[] lines;

            Puzzle puzzle;
            int[][] sudoku;

            StorageOperationResult result;

            if (File.Exists(fullpath))
            {
                jsonstring = File.ReadAllText(fullpath);

                puzzle = JsonSerializer.Deserialize<Puzzle>(jsonstring);

                result = StorageOperationResult.Success;
            }
            else
            {
                string defaultSudoku = "6 0 0 8 0 0 7 0 9\n0 0 4 0 0 2 0 6 0\n0 0 0 0 3 7 0 0 0\n5 0 0 1 0 0 0 8 0\n0 0 1 0 0 0 6 0 0\n0 8 0 0 0 3 0 0 2\n0 0 0 0 1 0 0 0 0\n0 3 0 7 0 0 1 0 0\n4 0 2 0 0 6 0 0 8";
                lines = defaultSudoku.Split('\n');


                sudoku = new int[lines.Length][];
                for (int i = 0; i < lines.Length; i++)
                {
                    sudoku[i] = lines[i].Where(c => Char.IsDigit(c)).Select(c => c - 48).ToArray();
                }
                puzzle = new Puzzle()
                {
                    sudoku = new int[lines.Length][]
                };
                sudoku.CopyTo(puzzle.sudoku, 0);

                result = StorageOperationResult.Failed;
            }


            return (puzzle.sudoku, result);
        }

        public override List<string> GetListOfPuzzles()
        {
            return Directory.GetFiles(Dirpath, "*", SearchOption.TopDirectoryOnly)
                .Select((filename) => Path.GetFileNameWithoutExtension(filename))
                .ToList();
        }


        public override void Save(int[][] puzzle, string puzzlename)
        {
            string fullpath = Dirpath + puzzlename + _format;

            Puzzle puzzleToSave = new Puzzle()
            {
                sudoku = new int[Field.Row_Count][]
            };
            puzzle.CopyTo(puzzleToSave.sudoku, 0);

            string data = JsonSerializer.Serialize<Puzzle>(puzzleToSave);

            using (FileStream f = File.Create(fullpath))
            {
                WriteDataToFileStream(data, f);
            }
        }

        public override StorageOperationResult Delete(string[] names)
        {
            StorageOperationResult result = StorageOperationResult.Success;

            try
            {
                string fullpath;
                foreach (string name in names)
                {
                    fullpath = $"{Dirpath}{name}{_format}";
                    if (File.Exists(fullpath))
                        File.Delete(fullpath);
                }
            }
            catch
            {
                result = StorageOperationResult.Failed;
            }


            return result;
        }


        private void WriteDataToFileStream(string data, FileStream f)
        {
            byte[] byteS = Encoding.UTF8.GetBytes(data);
            f.Write(byteS, 0, byteS.Length);
        }
    }
}
