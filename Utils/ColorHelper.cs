using System.Text.RegularExpressions;

namespace NeedSystem.Utils;

public static class ColorHelper
{
    public static int ConvertHexToColor(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex[1..];
        }
        return int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
    }

    public static string StripColorCodes(string text)
    {
        return Regex.Replace(text, @"\{[A-Z_]+\}", "");
    }
}