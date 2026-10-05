using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeneSearchApp
{
    class Program
    {
        struct BioItem
        {
            public string Title { get; set; }
            public string Source { get; set; }
            public string Chain { get; set; }
        }

        static void Main(string[] args)
        {
            const string srcFile = "sequences.txt";
            const string cmdFile = "commands.txt";
            const string resFile = "genedata.txt";

            if (!File.Exists(srcFile) || !File.Exists(cmdFile))
            {
                Console.WriteLine("Ошибка: Отсутствуют входные файлы sequences.txt или commands.txt!");
                return;
            }

            List<BioItem> registry = LoadRegistry(srcFile);

            ExecuteCommands(cmdFile, resFile, registry);
        }

        static void ExecuteCommands(string cmdPath, string resPath, List<BioItem> registry)
        {
            using (StreamWriter output = new StreamWriter(resPath))
            {
                output.WriteLine("Сабуть Марат Андреевич");
                output.WriteLine("Генетический поиск");

                int index = 1;

                foreach (string row in File.ReadLines(cmdPath))
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;

                    string[] segments = row.Split('\t');
                    string action = segments[0].Trim().ToLower();
                    string label = index.ToString("D3");

                    output.WriteLine("-----------------------------------------------------------------");

                    if (action == "search")
                    {
                        string target = UnpackSequence(segments[1].Trim());
                        output.WriteLine($"{label}  search  {target}");

                        List<BioItem> found = new List<BioItem>();
                        foreach (var item in registry)
                        {
                            if (item.Chain.Contains(target))
                            {
                                found.Add(item);
                            }
                        }

                        if (found.Count > 0)
                        {
                            foreach (var element in found)
                            {
                                output.WriteLine($"{element.Source}     {element.Title}");
                            }
                        }
                        else
                        {
                            output.WriteLine("NOT FOUND");
                        }
                    }
                    else if (action == "diff")
                    {
                        string firstKey = segments[1].Trim();
                        string secondKey = segments[2].Trim();
                        output.WriteLine($"{label}  diff  {firstKey}  and  {secondKey}");
                        output.Write("amino-acids difference: ");

                        BioItem unit1 = default;
                        BioItem unit2 = default;
                        bool hasUnit1 = false;
                        bool hasUnit2 = false;

                        foreach (var x in registry)
                        {
                            if (x.Title.Equals(firstKey, StringComparison.OrdinalIgnoreCase))
                            {
                                unit1 = x;
                                hasUnit1 = true;
                            }
                            if (x.Title.Equals(secondKey, StringComparison.OrdinalIgnoreCase))
                            {
                                unit2 = x;
                                hasUnit2 = true;
                            }
                        }

                        if (!hasUnit1 || !hasUnit2)
                        {
                            List<string> lost = new List<string>();
                            if (!hasUnit1) lost.Add(firstKey);
                            if (!hasUnit2) lost.Add(secondKey);
                            output.WriteLine($"MISSING: {string.Join(", ", lost)}");
                        }
                        else
                        {
                            output.WriteLine(GetDistance(unit1.Chain, unit2.Chain));
                        }
                    }
                    else if (action == "mode")
                    {
                        string targetKey = segments[1].Trim();
                        output.WriteLine($"{label}  mode  {targetKey}");
                        output.Write("amino-acid occurs: ");

                        BioItem match = default;
                        bool hasMatch = false;

                        foreach (var x in registry)
                        {
                            if (x.Title.Equals(targetKey, StringComparison.OrdinalIgnoreCase))
                            {
                                match = x;
                                hasMatch = true;
                                break;
                            }
                        }

                        if (!hasMatch)
                        {
                            output.WriteLine($"MISSING: {targetKey}");
                        }
                        else
                        {
                            KeyValuePair<char, int> frequencyData = GetDominantChar(match.Chain);
                            output.WriteLine($"{frequencyData.Key} {frequencyData.Value}");
                        }
                    }

                    index++;
                }
            }

            Console.WriteLine("Обработка завершена. Результаты сохранены в genedata.txt");
        }

        static List<BioItem> LoadRegistry(string path)
        {
            var items = new List<BioItem>();
            foreach (string row in File.ReadLines(path))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;

                string[] tokens = row.Split('\t');
                if (tokens.Length >= 3)
                {
                    items.Add(new BioItem
                    {
                        Title = tokens[0].Trim(),
                        Source = tokens[1].Trim(),
                        Chain = UnpackSequence(tokens[2].Trim())
                    });
                }
            }
            return items;
        }

        static string UnpackSequence(string template)
        {
            StringBuilder sb = new StringBuilder();
            int pointer = 0;

            while (pointer < template.Length)
            {
                if (char.IsDigit(template[pointer]))
                {
                    int factor = template[pointer] - '0';
                    sb.Append(template[pointer + 1], factor);
                    pointer += 2;
                }
                else
                {
                    sb.Append(template[pointer]);
                    pointer++;
                }
            }
            return sb.ToString();
        }

        static int GetDistance(string a, string b)
        {
            int totalDiff = 0;
            int limit = Math.Max(a.Length, b.Length);

            for (int i = 0; i < limit; i++)
            {
                if (i >= a.Length || i >= b.Length)
                {
                    totalDiff++;
                }
                else if (a[i] != b[i])
                {
                    totalDiff++;
                }
            }
            return totalDiff;
        }

        static KeyValuePair<char, int> GetDominantChar(string sequence)
        {
            var counts = new Dictionary<char, int>();
            foreach (char c in sequence)
            {
                if (counts.ContainsKey(c))
                {
                    counts[c]++;
                }
                else
                {
                    counts[c] = 1;
                }
            }

            KeyValuePair<char, int> best = new KeyValuePair<char, int>('\0', -1);

            foreach (var kvp in counts)
            {
                if (best.Value == -1)
                {
                    best = kvp;
                    continue;
                }

                if (kvp.Value > best.Value)
                {
                    best = kvp;
                }
                else if (kvp.Value == best.Value)
                {
                    if (kvp.Key < best.Key)
                    {
                        best = kvp;
                    }
                }
            }

            return best;
        }
    }
}
