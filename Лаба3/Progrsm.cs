using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using StringSet = System.Collections.Generic.HashSet<string>;

[XmlInclude(typeof(Word))]
[XmlInclude(typeof(Punctuation))]
public abstract class Token
{
    [XmlText]
    public string Value { get; set; }

    protected Token() { }
    protected Token(string value) => Value = value;

    public override string ToString() => Value;
}

public class Word : Token
{
    public Word() : base() { }
    public Word(string value) : base(value) { }

    [XmlIgnore]
    public int Length => Value.Length;
}

public class Punctuation : Token
{
    public Punctuation() : base() { }
    public Punctuation(string value) : base(value) { }
}

public class Sentence : IEnumerable<Token>
{
    [XmlElement("word", typeof(Word))]
    [XmlElement("punctuation", typeof(Punctuation))]
    public List<Token> Tokens { get; set; } = new List<Token>();

    public void Add(Token token) => Tokens.Add(token);

    [XmlIgnore]
    public int WordCount
    {
        get
        {
            int count = 0;
            foreach (var token in Tokens)
            {
                if (token is Word) count++;
            }
            return count;
        }
    }

    [XmlIgnore]
    public int Length
    {
        get
        {
            int totalLength = 0;
            foreach (var token in Tokens)
            {
                totalLength += token.Value.Length;
            }
            return totalLength;
        }
    }

    [XmlIgnore]
    public bool IsQuestion
    {
        get
        {
            if (Tokens.Count == 0) return false;
            return Tokens.Last().Value == "?";
        }
    }

    public IEnumerator<Token> GetEnumerator() => Tokens.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Tokens.Count; i++)
        {
            sb.Append(Tokens.ElementAt(i).Value);
            if (i < Tokens.Count - 1 && Tokens.ElementAt(i) is Word && Tokens.ElementAt(i + 1) is Word)
                sb.Append(" ");
        }
        return sb.ToString();
    }
}

[XmlRoot("text")]
public class Text : IEnumerable<Sentence>
{
    [XmlElement("sentence")]
    public List<Sentence> Sentences { get; set; } = new List<Sentence>();

    public void Add(Sentence sentence) => Sentences.Add(sentence);

    public IEnumerator<Sentence> GetEnumerator() => Sentences.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        return string.Join(" ", Sentences);
    }
}

public static class TextParser
{
    public static Text Parse(string rawText)
    {
        var textObj = new Text();
        if (string.IsNullOrWhiteSpace(rawText)) return textObj;

        string sentenceRegex = @"(?<=[.!?])\s+";
        var rawSentences = Regex.Split(rawText, sentenceRegex);

        foreach (var rawSentence in rawSentences)
        {
            if (string.IsNullOrWhiteSpace(rawSentence)) continue;

            var sentenceObj = new Sentence();
            string tokenRegex = @"([\w-]+)|([^\w\s])";
            var matches = Regex.Matches(rawSentence, tokenRegex);

            foreach (Match match in matches)
            {
                if (match.Groups[1].Success)
                {
                    sentenceObj.Tokens.Add(new Word(match.Groups[1].Value));
                }
                else if (match.Groups[2].Success)
                {
                    sentenceObj.Tokens.Add(new Punctuation(match.Groups[2].Value));
                }
            }

            if (sentenceObj.Tokens.Count > 0)
            {
                textObj.Sentences.Add(sentenceObj);
            }
        }

        return textObj;
    }
}


public static class TextService
{
    public static List<Sentence> SortByWordCount(Text text)
    {
        List<Sentence> sorted = new List<Sentence>(text.Sentences);
        sorted.Sort((s1, s2) => s1.WordCount.CompareTo(s2.WordCount));
        return sorted;
    }

    public static List<Sentence> SortByLength(Text text)
    {
        List<Sentence> sorted = new List<Sentence>(text.Sentences);
        sorted.Sort((s1, s2) => s1.Length.CompareTo(s2.Length));
        return sorted;
    }

    public static StringSet FindWordsInQuestions(Text text, int length)
    {
        var result = new StringSet(StringComparer.OrdinalIgnoreCase);
        foreach (var sentence in text.Sentences)
        {
            if (sentence.IsQuestion)
            {
                foreach (var token in sentence.Tokens)
                {
                    if (token is Word word && word.Length == length)
                    {
                        result.Add(word.Value);
                    }
                }
            }
        }
        return result;
    }

    public static void RemoveWordsStartingWithConsonant(Text text, int length)
    {
        string consonants = "bcdfghjklmnpqrstvwxyzбвгджзйклмнпрстфхцчшщ";

        foreach (var sentence in text.Sentences)
        {
            for (int i = sentence.Tokens.Count - 1; i >= 0; i--)
            {
                if (sentence.Tokens.ElementAt(i) is Word w && w.Length == length)
                {
                    if (w.Value.Length > 0 && consonants.Contains(char.ToLower(w.Value.First()).ToString()))
                    {
                        sentence.Tokens.RemoveAt(i);
                    }
                }
            }
        }
    }

    public static void ReplaceWordsInSentence(Sentence sentence, int length, string subString)
    {
        for (int i = 0; i < sentence.Tokens.Count; i++)
        {
            if (sentence.Tokens.ElementAt(i) is Word w && w.Length == length)
            {
                sentence.Tokens.RemoveAt(i);
                sentence.Tokens.Insert(i, new Word(subString));
            }
        }
    }

    public static void RemoveStopWords(Text text, StringSet stopWords)
    {
        foreach (var sentence in text.Sentences)
        {
            for (int i = sentence.Tokens.Count - 1; i >= 0; i--)
            {
                if (sentence.Tokens.ElementAt(i) is Word w)
                {
                    if (stopWords.Contains(w.Value.ToLower()))
                    {
                        sentence.Tokens.RemoveAt(i);
                    }
                }
            }
        }
    }

    public static void ExportToXml(Text text, string filePath)
    {
        var xmlText = new Text();
        foreach (var sentence in text.Sentences)
        {
            var newSentence = new Sentence();
            var words = new List<Word>();
            foreach (var token in sentence.Tokens)
            {
                if (token is Word w)
                {
                    words.Add(new Word(w.Value));
                }
            }
            words.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.OrdinalIgnoreCase));
            foreach (var w in words)
            {
                newSentence.Tokens.Add(w);
            }
            xmlText.Sentences.Add(newSentence);
        }

        XmlSerializer serializer = new XmlSerializer(typeof(Text));
        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
        ns.Add("", "");

        XmlWriterSettings settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = ""
        };

        using (XmlWriter writer = XmlWriter.Create(filePath, settings))
        {
            writer.WriteStartDocument(true);
            serializer.Serialize(writer, xmlText, ns);
            writer.WriteEndDocument();
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        string inputFile = "input.txt";

        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Файл '{inputFile}' не найден. Создайте его в папке: {Directory.GetCurrentDirectory()}");
            Console.ReadKey();
            return;
        }

        string rawText = File.ReadAllText(inputFile, Encoding.UTF8);
        Text text = TextParser.Parse(rawText);

        Console.WriteLine("=== 1. По возрастанию количества слов ===");
        foreach (var s in TextService.SortByWordCount(text))
        {
            Console.WriteLine($"[{s.WordCount} сл.] {s}");
        }

        Console.WriteLine("\n=== 2. По возрастанию длины предложения ===");
        foreach (var s in TextService.SortByLength(text))
        {
            Console.WriteLine($"[{s.Length} симв.] {s}");
        }

        int questionWordLen = 4;
        Console.WriteLine($"\n=== 3. Слова длиной {questionWordLen} в вопросах ===");
        var qWords = TextService.FindWordsInQuestions(text, questionWordLen);
        Console.WriteLine(qWords.Count > 0 ? string.Join(", ", qWords) : "Не найдено.");

        int removeLen = 5;
        Text textForConsonants = TextParser.Parse(rawText);
        TextService.RemoveWordsStartingWithConsonant(textForConsonants, removeLen);
        Console.WriteLine($"\n=== 4. После удаления слов длиной {removeLen} на согласную ===");
        Console.WriteLine(textForConsonants);

        Text textForReplace = TextParser.Parse(rawText);
        if (textForReplace.Sentences.Count > 0)
        {
            int targetWordLen = 3;
            string replacement = "[ЗАМЕНА]";
            TextService.ReplaceWordsInSentence(textForReplace.Sentences.First(), targetWordLen, replacement);
            Console.WriteLine($"\n=== 5. Замена слов длиной {targetWordLen} в 1-м предложении ===");
            Console.WriteLine(textForReplace.Sentences.First());
        }

        Text textForStopWords = TextParser.Parse(rawText);
        StringSet stopWords = LoadStopWords("stopwords_ru.txt", "stopwords_en.txt");
        TextService.RemoveStopWords(textForStopWords, stopWords);
        Console.WriteLine("\n=== 6. Текст после удаления стоп-слов ===");
        Console.WriteLine(textForStopWords);
        string xmlPath = "output.xml";
        TextService.ExportToXml(text, xmlPath);
        Console.WriteLine($"\n=== 7. Текст успешно сохранен в файл {xmlPath} ===");
        Console.ReadKey();
    }
    static StringSet LoadStopWords(params string[] filePaths)
    {
        var stopWords = new StringSet(StringComparer.OrdinalIgnoreCase);
        foreach (var path in filePaths)
        {
            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    string trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        stopWords.Add(trimmed.ToLower());
                    }
                }
            }
        }
        return stopWords;
    }
}