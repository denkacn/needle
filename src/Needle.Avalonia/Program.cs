namespace Needle.Avalonia;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        new ApplicationBootstrapper().Run(args);
    }
}
