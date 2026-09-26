Свободное место закончилось 6 дней назад … Недостаточно свободного места. Вы не сможете добавлять новые файлы на Диск, сохранять облачные копии в Google Фото и пользоваться Gmail. Получите 100 ГБ в хранилище и пользуйтесь им вместе с участниками семейной группы за 1,99 $ 0,49 $ в течение 3 месяцев.
100 %
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticSearch
{
    class Program
    {
        struct Protein
        {
            public string name; // protein name
            public string organism; //organism name
            public string amino_acids; // sequence of amino_asids
        }

        struct Command
        {
            public string name;
            public string parameter1;
            public string parameter2;
        }

        static List<Command> ReadCommands(string filename)
        {
            StreamReader reader = new StreamReader(filename);
            List<Command> commands = new List<Command>();

            Command command;
            command.name = String.Empty;
            command.parameter1 = String.Empty;
            command.parameter2 = String.Empty;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t');

                if (parts.Length == 2)
                {
                    command.name = parts[0];
                    command.parameter1 = parts[1];
                    command.parameter2 = String.Empty;
                }
                else
                {
                    command.name = parts[0];
                    command.parameter1 = parts[1];
                    command.parameter2 = parts[2];
                }
                commands.Add(command);
            }
            return commands;
        }

        static List<Protein> ReadData(string filename)
        {
            //reader object to read data from file
            StreamReader reader = new StreamReader(filename);

            // empty list to keep data about proteins
            List<Protein> data = new List<Protein>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] parts = line.Split('\t');
                Protein protein;
                protein.name = parts[0];
                protein.organism = parts[1];
                protein.amino_acids = parts[2];
                data.Add(protein);
            }
            return data;
        }

        static string Encoding(string amino_acids)
        {
            string encoded = String.Empty;
            for (int i = 0; i < amino_acids.Length; i++)
            {
                char ch = amino_acids[i];
                int count = 1;
                while (i < amino_acids.Length - 1 && amino_acids[i + 1] == ch)
                {
                    count++;
                    i++;
                }
                if (count > 2) encoded = encoded + count + ch;
                if (count == 1) encoded = encoded + ch;
                if (count == 2) encoded = encoded + ch + ch;
            }
            return encoded;
        }

        static string Decoding(string amino_acids)
        {
            string decoded = String.Empty;
            for (int i = 0; i < amino_acids.Length; i++)
            {   // 8ATA3TCGC4T....
                char ch = amino_acids[i];
                if (char.IsDigit(ch))  // '8' -> int 8
                {
                    char letter = amino_acids[i + 1];
                    int count = ch - '0'; // '8' - '0' = 8
                    for (int j = 1; j < count; j++)
                        decoded = decoded + letter;
                }
                else decoded = decoded + ch;
            }
            return decoded;
        }

        static void PrintData(List<Protein> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                Console.WriteLine("Protein " + (i + 1));
                Console.WriteLine(data[i].name);
                Console.WriteLine(data[i].organism);
                Console.WriteLine(data[i].amino_acids);
                Console.WriteLine("========================");
            }
        }

        static void PrintCommands(List<Command> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                Console.WriteLine("Command " + (i + 1));
                Console.WriteLine(commands[i].name);
                Console.WriteLine(commands[i].parameter1);
                Console.WriteLine(commands[i].parameter2);
                Console.WriteLine("========================");
            }
        }

        static void CommandHandler(List<Protein> proteins, List<Command> commands)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].name == "search") { }
                if (commands[i].name == "diff") { }
                if (commands[i].name == "mode") { }
            }
        }

        static void Main(string[] args)
        {
            //reding protein data
            List<Protein> data = ReadData("sequences.0.txt");
            PrintData(data);

            //reading commands
            List<Command> commands = ReadCommands("commands.0.txt");
            PrintCommands(commands);
        }
    }
}