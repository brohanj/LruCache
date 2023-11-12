namespace CompanyName.Sdk.Utils.Examples.Utils;

internal static class ConsoleUtils
{
    public static void WriteEvictedMessage(string message)
    {
        var currentForegroundColor = ForegroundColor;
        ForegroundColor = ConsoleColor.Red;
        Error.WriteLine(message);
        ForegroundColor = currentForegroundColor;
    }

    public static void WriteInfoMessage(string message)
    {
        var currentForegroundColor = ForegroundColor;
        ForegroundColor = ConsoleColor.Green;
        WriteLine(message);
        ForegroundColor = currentForegroundColor;
    }
}
