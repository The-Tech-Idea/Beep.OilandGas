namespace TheTechIdea.Data.OilGas;

/// <summary>A connection name is used as a generated C# namespace segment by Beep RDB datasources.</summary>
public static class BeepConnectionName
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue",
        "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally",
        "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
        "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void",
        "volatile", "while"
    };

    public static bool IsValid(string? name) => !string.IsNullOrEmpty(name) && name.Length <= 128 &&
        (char.IsAsciiLetter(name[0]) || name[0] == '_') &&
        name.All(c => char.IsAsciiLetterOrDigit(c) || c == '_') && !Keywords.Contains(name);
}
