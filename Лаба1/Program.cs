using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            using (StreamWriter output = new StreamWriter(resFile))
            {
                output.WriteLine("Иван Иванов");
                output.WriteLine("Генетический поиск");

                int index = 1;

                foreach (string row in File.ReadLines(cmdFile))
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;

                    string[] segments = row.Split('\t');
                    string action = segments[0].Trim().ToLower();
                    string label = index.ToString("D3");

                    output.WriteLine("-----------------------------------------------------------------");

                    switch (action)
                    {
                        case "search":
                            string target = UnpackSequence(segments[1].Trim());
                            output.WriteLine($"{label}  search  {target}");

                            var found = registry.Where(item => item.Chain.Contains(target)).ToList();
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
                            break;

                        case "diff":
                            string firstKey = segments[1].Trim();
                            string secondKey = segments[2].Trim();
                            output.WriteLine($"{label}  diff  {firstKey}  and  {secondKey}");
                            output.Write("amino-acids difference: ");

                            var unit1 = registry.FirstOrDefault(x => x.Title.Equals(firstKey, StringComparison.OrdinalIgnoreCase));
                            var unit2 = registry.FirstOrDefault(x => x.Title.Equals(secondKey, StringComparison.OrdinalIgnoreCase));

                            if (unit1.Title == null || unit2.Title == null)
                            {
                                List<string> lost = new List<string>();
                                if (unit1.Title == null) lost.Add(firstKey);
                                if (unit2.Title == null) lost.Add(secondKey);
                                output.WriteLine($"MISSING: {string.Join(", ", lost)}");
                            }
                            else
                            {
                                output.WriteLine(GetDistance(unit1.Chain, unit2.Chain));
                            }
                            break;

                        case "mode":
                            string targetKey = segments[1].Trim();
                            output.WriteLine($"{label}  mode  {targetKey}");
                            output.Write("amino-acid occurs: ");

                            var match = registry.FirstOrDefault(x => x.Title.Equals(targetKey, StringComparison.OrdinalIgnoreCase));
                            if (match.Title == null)
                            {
                                output.WriteLine($"MISSING: {targetKey}");
                            }
                            else
                            {
                                var frequencyData = GetDominantChar(match.Chain);
                                output.WriteLine($"{frequencyData.Key} {frequencyData.Value}");
                            }
                            break;
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
            return sequence.GroupBy(c => c)
                           .Select(g => new KeyValuePair<char, int>(g.Key, g.Count()))
                           .OrderByDescending(kvp => kvp.Value)
                           .ThenBy(kvp => kvp.Key)
                           .First();
        }
    }
}
