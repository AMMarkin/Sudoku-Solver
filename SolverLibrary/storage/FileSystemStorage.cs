using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SolverLibrary.storage
{
    public class FileSystemStorage : Storage
    {
        private string CurrDirectory => Environment.CurrentDirectory;
        private string Dirpath => Directory.GetParent(CurrDirectory).Parent.Parent.FullName + @"\Sudoku\";

        public override (int[][], StorageOperationResult) Load(string name)
        {
            string fullpath = Dirpath + name + ".txt";
            string[] lines;

            int[][] puzzle;

            StorageOperationResult result;

            if (File.Exists(fullpath))
            {
                lines = File.ReadAllLines(fullpath);
                result = StorageOperationResult.Success;
            }
            else
            {
                string defaultSudoku = "6 0 0 8 0 0 7 0 9\n0 0 4 0 0 2 0 6 0\n0 0 0 0 3 7 0 0 0\n5 0 0 1 0 0 0 8 0\n0 0 1 0 0 0 6 0 0\n0 8 0 0 0 3 0 0 2\n0 0 0 0 1 0 0 0 0\n0 3 0 7 0 0 1 0 0\n4 0 2 0 0 6 0 0 8";
                lines = defaultSudoku.Split('\n');

                result = StorageOperationResult.Failed;
            }

            puzzle = new int[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                puzzle[i] = lines[i].Where(c => Char.IsDigit(c)).Select(c => c - 48).ToArray();
            }

            return (puzzle, result);
        }

        public override List<string> GetListOfPuzzles()
        {
            return Directory.GetFiles(Dirpath, "*", SearchOption.TopDirectoryOnly)
                .Select((filename) => Path.GetFileName(filename))
                .ToList();
        }


        public override void Save(int[][] puzzle, string puzzlename)
        {
            string fullpath = Dirpath + puzzlename + ".txt";

            string[] datalines = new string[puzzle.Length];
            for(int row =0;row< puzzle.Length;row++)
            {
                datalines[row] = puzzle[row].Select(x => x.ToString()).Aggregate((a, b) => $"{a} {b}");
            }

            string data = datalines.Aggregate((a, b) => $"{a}\n{b}");    

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
                    fullpath = $"{Dirpath}{name}.txt";
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
