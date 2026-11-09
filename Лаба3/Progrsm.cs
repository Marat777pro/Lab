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

    public IEnumerator<Sentence> GetEnumerator() => Sentences.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString()
    {
        return string.Join(" ", Sentences);
    }
}
