using System;

namespace FortRise;



public static class StringUtils
{
    public static string ToTitleCase(string characters)  
    {
        return ToTitleCase(characters.AsSpan());
    }
    public static string ToTitleCase(ReadOnlySpan<char> characters) 
    {
        Span<char> s = stackalloc char[characters.Length];
        bool capital = true;
        for (int i = 0; i < characters.Length; i++) 
        {        
            char current = characters[i];
            if (char.IsWhiteSpace(current))
            {
                capital = true;
                continue;
            }
            if (capital) 
            {

                if (!char.IsUpper(current)) 
                {
                    s[i] = char.ToUpper(current);
                }
                capital = false;
                continue;
            }
            
            if (char.IsUpper(current)) 
            {
                s[i] = char.ToLower(current);
            }
            else
                s[i] = characters[i];
        }
        return s.ToString();
    }

    // Based on:
    // https://www.meziantou.net/split-a-string-into-lines-without-allocation.htm
    public static LineSplitEnumerator SplitLines(this ReadOnlySpan<char> str, char separator)
    {
        // LineSplitEnumerator is a struct so there is no allocation here
        return new LineSplitEnumerator(str, separator);
    }

    // Must be a ref struct as it contains a ReadOnlySpan<char>
    public ref struct LineSplitEnumerator
    {
        private ReadOnlySpan<char> _str;
        private char separator;

        public LineSplitEnumerator(ReadOnlySpan<char> str, char separator)
        {
            _str = str;
            Current = default;
            this.separator = separator;
        }

        // Needed to be compatible with the foreach operator
        public LineSplitEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = _str;
            if (span.Length == 0) // Reach the end of the string
                return false;

            var index = span.IndexOf(separator);
            if (index == -1) // The string is composed of only one line
            {
                _str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new LineSplitEntry(span, ReadOnlySpan<char>.Empty);
                return true;
            }

            Current = new LineSplitEntry(span.Slice(0, index), span.Slice(index, 1));
            _str = span.Slice(index + 1);
            return true;
        }

        public LineSplitEntry Current { get; private set; }
    }

    public readonly ref struct LineSplitEntry
    {
        public LineSplitEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
        {
            Line = line;
            Separator = separator;
        }

        public ReadOnlySpan<char> Line { get; }
        public ReadOnlySpan<char> Separator { get; }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
        {
            line = Line;
            separator = Separator;
        }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(LineSplitEntry entry) => entry.Line;
    }
}