using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneSearchApp
{
    class Program
    {
        class ProteinData
        {
            public string Name { get; set; }
            public string Organism { get; set; }
            public string Sequence { get; set; } 
        }

        static void Main(string[] args)
        {
            string sequencesPath = "sequences.txt";
            string commandsPath = "commands.txt";
            string outputPath = "genedata.txt";

            if (!File.Exists(sequencesPath) || !File.Exists(commandsPath))
            {
                Console.WriteLine("Ошибка: Отсутствуют входные файлы sequences.txt или commands.txt!");
                return;
            }

            List<ProteinData> proteins = new List<ProteinData>();
            foreach (var line in File.ReadLines(sequencesPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('\t');
                if (parts.Length >= 3)
                {
                    proteins.Add(new ProteinData
                    {
                        Name = parts[0].Trim(),
                        Organism = parts[1].Trim(),
                        Sequence = DecodeRLE(parts[2].Trim())
                    });
                }
            }
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                writer.WriteLine("Иван Иванов");
                writer.WriteLine("Генетический поиск");

                int commandCounter = 1;

                foreach (var line in File.ReadLines(commandsPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('\t');
                    string commandType = parts[0].Trim().ToLower();
                    string cmdNumberStr = commandCounter.ToString("D3");

                    writer.WriteLine("-----------------------------------------------------------------");

                    if (commandType == "search")
                    {
                        string searchTarget = DecodeRLE(parts[1].Trim());
                        writer.WriteLine($"{cmdNumberStr}  search  {searchTarget}");

                        var matches = proteins.Where(p => p.Sequence.Contains(searchTarget)).ToList();

                        if (matches.Count > 0)
                        {
                            foreach (var match in matches)
                            {
                                writer.WriteLine($"{match.Organism}     {match.Name}");
                            }
                        }
                        else
                        {
                            writer.WriteLine("NOT FOUND");
                        }
                    }
                    else if (commandType == "diff")
                    {
                        string protein1Name = parts[1].Trim();
                        string protein2Name = parts[2].Trim();
                        writer.WriteLine($"{cmdNumberStr}  diff  {protein1Name}  and  {protein2Name}");
                        writer.Write("amino-acids difference: ");

                        var p1 = proteins.FirstOrDefault(p => p.Name.Equals(protein1Name, StringComparison.OrdinalIgnoreCase));
                        var p2 = proteins.FirstOrDefault(p => p.Name.Equals(protein2Name, StringComparison.OrdinalIgnoreCase));
                        if (p1 == null || p2 == null)
                        {
                            List<string> missing = new List<string>();
                            if (p1 == null) missing.Add(protein1Name);
                            if (p2 == null) missing.Add(protein2Name);
                            writer.WriteLine($"MISSING: {string.Join(", ", missing)}");
                        }
                        else
                        {
                            int diffCount = CalculateDiff(p1.Sequence, p2.Sequence);
                            writer.WriteLine(diffCount);
                        }
                    }
                    else if (commandType == "mode")
                    {
                        string proteinName = parts[1].Trim();
                        writer.WriteLine($"{cmdNumberStr}  mode  {proteinName}");
                        writer.Write("amino-acid occurs: ");

                        var p = proteins.FirstOrDefault(x => x.Name.Equals(proteinName, StringComparison.OrdinalIgnoreCase));

                        if (p == null)
                        {
                            writer.WriteLine($"MISSING: {proteinName}");
                        }
                        else
                        {
                            var (aminoAcid, count) = FindMode(p.Sequence);
                            writer.WriteLine($"{aminoAcid} {count}");
                        }
                    }

                    commandCounter++;
                }
            }

            Console.WriteLine("Обработка завершена. Результаты сохранены в genedata.txt");
        }