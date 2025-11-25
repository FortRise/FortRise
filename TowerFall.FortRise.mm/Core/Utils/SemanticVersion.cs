// Referenced to: https://github.com/Shockah/Nickel/blob/0e8c138e44fb5e9d854d50889aef005ed2d2c53c/NickelCommon/SemanticVersion.cs
#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FortRise;

public readonly struct SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
{
    public int Major { get; init; }
    public int Minor { get; init; }
    public int Patch { get; init; }
    public string? Prerelease { get; init; }

    public static readonly SemanticVersion Empty = new SemanticVersion(0, 0, 0, null);

    public SemanticVersion(int major = 1, int minor = 0, int patch = 0, string? prerelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
    }

    public SemanticVersion(string versionString)
    {
        if (!TryParse(versionString, out var version)) 
        {
            throw new Exception($"Version '{versionString}' is invalid.");
        }
        this = version;
    }

    public SemanticVersion(ReadOnlySpan<char> versionString)
    {
        if (!TryParse(versionString, out var version)) 
        {
            throw new Exception($"Version '{versionString}' is invalid.");
        }
        this = version;
    }

    public static bool TryParse(ReadOnlySpan<char> versionString, out SemanticVersion version)
    {
        version = default;
        int patch = 0;
        string? prerelease = null;

        versionString = versionString.Trim();
        if (versionString.IsEmpty)
        {
            return false;
        }

        ReadOnlySpan<char> spanString = versionString;

        int i = 0;

        if (!TryParseVersionPart(spanString, ref i, out var major) ||
            !TryParseLiteral(spanString, ref i, '.') || 
            !TryParseVersionPart(spanString, ref i, out var minor))
        {
            return false;
        }

		if (TryParseLiteral(spanString, ref i, '.') && !TryParseVersionPart(spanString, ref i, out patch))
			return false;

		if (TryParseLiteral(spanString, ref i, '-') && !TryParseTag(spanString, ref i, out prerelease))
			return false;

		if (i != versionString.Length)
			return false;

        version = new SemanticVersion(major, minor, patch, prerelease);
        return true;
    }

    private static bool TryParseVersionPart(ReadOnlySpan<char> span, ref int index, out int part)
    {
        part = 0;

        var str = string.Empty;

        for (int i = index; i < span.Length && char.IsDigit(span[i]); i++)
        {
            str += span[i];
        }

        if (str.Length == 0)
        {
            return false;
        }

        if (str.Length > 1 && str[0] == '0')
        {
            return false;
        }

        part = int.Parse(str);
        index += str.Length;
        return true;
    }

    private static bool TryParseLiteral(ReadOnlySpan<char> span, ref int index, char ch)
    {
        if (index >= span.Length || span[index] != ch)
        {
            return false;
        }
        index += 1;
        return true;
    }

	private static bool TryParseTag(ReadOnlySpan<char> span, ref int index, out string? tag)
	{
		var length = 0;
		for (var i = index; i < span.Length && (char.IsLetterOrDigit(span[i]) || span[i] == '-' || span[i] == '.'); i++)
			length++;

		if (length == 0)
		{
			tag = null;
			return false;
		}

		tag = new string(span.ToArray(), index, length);
		index += length;
		return true;
	}

    public int CompareTo(SemanticVersion other)
    {
        if (Major != other.Major)
        {
            return Major.CompareTo(other.Major);
        }
        if (Minor != other.Minor)
        {
            return Minor.CompareTo(other.Minor);
        }
        if (Patch != other.Patch)
        {
            return Minor.CompareTo(other.Patch);
        }
        if (Equals(Prerelease, other.Prerelease))
        {
            return 0;
        }
        if (string.IsNullOrEmpty(Prerelease))
        {
            return 1;
        }
        if (string.IsNullOrEmpty(other.Prerelease))
        {
            return -1;
        }

        // this is where we hardcode things

        var self = Prerelease?.Split('.', '-') ?? [];
        var otherSelf = other.Prerelease?.Split('.', '-') ?? [];
        var length = Math.Max(self.Length, otherSelf.Length);

        for (int i = 0; i < length; i++)
        {
            if (self.Length <= i)
            {
                return -1;
            }

            if (otherSelf.Length <= i)
            {
                return 1;
            }

            if (self[i] == otherSelf[i])
            {
                if (i == length - 1)
                {
                    return 0;
                }
                continue;
            }

            if (otherSelf[i].Equals("unofficial", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }
            if (self[i].Equals("unofficial", StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }

            if (int.TryParse(self[i], out int selfNum) && int.TryParse(otherSelf[i], out int otherNum))
            {
                return selfNum.CompareTo(otherNum);
            }

            return string.Compare(self[i], otherSelf[i], StringComparison.OrdinalIgnoreCase);
        }

        return string.Compare($"{this}", $"{other}", StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(SemanticVersion other)
    {
        return Major == other.Major &&
            Minor == other.Minor &&
            Patch == other.Patch &&
            Equals(Prerelease, other.Prerelease);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Prerelease);
    }

    public override bool Equals(object? obj)
    {
        return obj is SemanticVersion version && Equals(version);
    }

    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (Prerelease != null)
        {
            version += "-" + Prerelease;
        }

        return version;
    }
	public static bool operator ==(SemanticVersion left, SemanticVersion right)
    {
		return left.Equals(right);
    }

	public static bool operator !=(SemanticVersion left, SemanticVersion right)
    {
        return !(left == right);
    }

	public static bool operator <(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) < 0;
    }

	public static bool operator <=(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) <= 0;
    }

	public static bool operator >(SemanticVersion left, SemanticVersion right)
    {
        return left.CompareTo(right) > 0;
    }

	public static bool operator >=(SemanticVersion left, SemanticVersion right)
    {
		return left.CompareTo(right) >= 0;
    }
}

public class SemanticVersionConverter : JsonConverter<SemanticVersion>
{
    public override SemanticVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (!SemanticVersion.TryParse(reader.GetString(), out SemanticVersion version)) 
        {
            throw new InvalidOperationException("SemanticVersion is in invalid format!");
        }
        return version;
    }

    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}