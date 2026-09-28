using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GeneSearchApp
{
    class Program
    {
        // Класс для хранения информации о белке
        class ProteinData
        {
            public string Name { get; set; }
            public string Organism { get; set; }
            public string Sequence { get; set; } // Храним сразу в развернутом виде
        }

        static void Main(string[] args)
        {
            // Пути к файлам (предполагается, что они лежат в папке с запуском программы)
            string sequencesPath = "sequences.txt";
            string commandsPath = "commands.txt";
            string outputPath = "genedata.txt";

            if (!File.Exists(sequencesPath) || !File.Exists(commandsPath))
            {
                Console.WriteLine("Ошибка: Отсутствуют входные файлы sequences.txt или commands.txt!");
                return;
            }

            // 1. Чтение и парсинг базы данных белков
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
                        Sequence = DecodeRLE(parts[2].Trim()) // Декодируем RLE при чтении
                    });
                }
            }
