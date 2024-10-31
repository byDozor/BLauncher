using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Аргументи: шляхи до основної програми та тимчасової папки з оновленням
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: Updater.exe <MainAppPath> <TempUpdatePath>");
            return;
        }

        string mainAppPath = args[0];
        string tempUpdatePath = args[1];
        WaitForMainAppExit(mainAppPath);
        CopyFiles(tempUpdatePath, Path.GetDirectoryName(mainAppPath));
        //Console.WriteLine("BLauncher успiшно оновлено. Натиснiть будь-яку клавiшу, щоб запустити оновлену версiю....");
        //Console.ReadKey();
        if (Directory.Exists(tempUpdatePath))
        {
            Directory.Delete(tempUpdatePath, true);
        }
        Process.Start(mainAppPath);
    }

    private static void WaitForMainAppExit(string mainAppPath)
    {
        var mainAppName = Path.GetFileNameWithoutExtension(mainAppPath);
        while (Process.GetProcessesByName(mainAppName).Any())
        {
            Thread.Sleep(1000);
        }
    }

    private static void CopyFiles(string sourceDir, string targetDir)
    {
        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            var relativePath = file.Substring(sourceDir.Length + 1);
            var targetFilePath = Path.Combine(targetDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
            File.Copy(file, targetFilePath, overwrite: true);
        }
    }
}