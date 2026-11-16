using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

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
    [XmlElement("Word", typeof(Word))]
    [XmlElement("Punctuation", typeof(Punctuation))]
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
            return Tokens[Tokens.Count - 1].Value == "?";
        }
    }

    public IEnumerator<Token> GetEnumerator() => Tokens.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < Tokens.Count; i++)
        {
            sb.Append(Tokens[i].Value);
            if (i < Tokens.Count - 1 && Tokens[i] is Word && Tokens[i + 1] is Word)
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

    public string BuildConcordance(string originalRawText)
    {
        if (Sentences == null || Sentences.Count == 0)
            return "Текст пуст.";

        string[] sourceLines = originalRawText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var concordance = new Dictionary<string, (int TotalCount, SortedSet<int> LineNumbers)>(StringComparer.OrdinalIgnoreCase);

        foreach (var sentence in Sentences)
        {
            foreach (var token in sentence.Tokens)
            {
                if (token is Word wordToken)
                {
                    string wordLower = wordToken.Value.ToLower();

                    if (!concordance.TryGetValue(wordLower, out var data))
                    {
                        var linesSet = new SortedSet<int>();
                        string pattern = @"(?<![\w-])" + Regex.Escape(wordLower) + @"(?![\w-])";

                        for (int i = 0; i < sourceLines.Length; i++)
                        {
                            if (Regex.IsMatch(sourceLines[i], pattern, RegexOptions.IgnoreCase))
                            {
                                linesSet.Add(i + 1);
                            }
                        }

                        data = (0, linesSet);
                    }

                    data.TotalCount++;
                    concordance[wordLower] = data;
                }
            }
        }

        var sortedWords = new List<string>(concordance.Keys);
        sortedWords.Sort(StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder();
        foreach (string word in sortedWords)
        {
            var entry = concordance[word];
            string lines = string.Join(" ", entry.LineNumbers);
            string leftPart = word.PadRight(35, '.');
            sb.AppendLine($"{leftPart}{entry.TotalCount}: {lines}");
        }

        return sb.ToString();
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

    public static HashSet<string> FindWordsInQuestions(Text text, int length)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                if (sentence.Tokens[i] is Word w && w.Length == length)
                {
                    if (w.Value.Length > 0 && consonants.Contains(char.ToLower(w.Value[0]).ToString()))
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
            if (sentence.Tokens[i] is Word w && w.Length == length)
            {
                sentence.Tokens[i] = new Word(subString);
            }
        }
    }

    public static void RemoveStopWords(Text text, HashSet<string> stopWords)
    {
        foreach (var sentence in text.Sentences)
        {
            for (int i = sentence.Tokens.Count - 1; i >= 0; i--)
            {
                if (sentence.Tokens[i] is Word w)
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
        XmlSerializer serializer = new XmlSerializer(typeof(Text));
        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            serializer.Serialize(writer, text);
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
            TextService.ReplaceWordsInSentence(textForReplace.Sentences[0], targetWordLen, replacement);
            Console.WriteLine($"\n=== 5. Замена слов длиной {targetWordLen} в 1-м предложении ===");
            Console.WriteLine(textForReplace.Sentences[0]);
        }

        Text textForStopWords = TextParser.Parse(rawText);
        var stopWords = LoadStopWords("stopwords_ru.txt", "stopwords_en.txt");
        TextService.RemoveStopWords(textForStopWords, stopWords);
        Console.WriteLine("\n=== 6. Текст после удаления стоп-слов ===");
        Console.WriteLine(textForStopWords);

        string xmlPath = "output.xml";
        TextService.ExportToXml(text, xmlPath);
        Console.WriteLine($"\n=== 7. Текст успешно сохранен в файл {xmlPath} ===");

        Console.WriteLine("\n=== 8. Конкорданс ===");
        string concordanceResult = text.BuildConcordance(rawText);
        Console.WriteLine(concordanceResult);
        File.WriteAllText("concordance.txt", concordanceResult, Encoding.UTF8);

        Console.ReadKey();
    }

    static HashSet<string> LoadStopWords(params string[] filePaths)
    {
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
