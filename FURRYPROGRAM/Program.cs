using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Diagnostics;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class Updater
{
    private static readonly HttpClient client = new HttpClient();

    static Updater()
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("FurryUpdater");
    }

    public static async Task<string> DownloadUpdateToTempAsync()
    {
        string url = "https://api.github.com/repos/maks535/FurryProgramm_History/releases/latest";

        try
        {
            string json = await client.GetStringAsync(url);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement assets = doc.RootElement.GetProperty("assets");

                if (assets.GetArrayLength() == 0)
                    return null;

                JsonElement? asset = null;

                foreach (var a in assets.EnumerateArray())
                {
                    string name = a.GetProperty("name").GetString();

                    if (name != null && name.EndsWith(".zip"))
                    {
                        asset = a;
                        break;
                    }
                }

                if (asset == null)
                    return null;

                string fileName = asset.Value.GetProperty("name").GetString();
                string downloadUrl = asset.Value.GetProperty("browser_download_url").GetString();


                string tempFolder = Path.Combine(
                    Path.GetTempPath(),
                    "FurryProgramm");

                Directory.CreateDirectory(tempFolder);

                string savePath = Path.Combine(tempFolder, fileName);

                using (var response = await client.GetAsync(downloadUrl))
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                return savePath;
            }
        }
        catch
        {
            return null;
        }
    }

    public static async Task<string> GetLatestVersionAsync()
    {
        string url = "https://api.github.com/repos/maks535/FurryProgramm_History/releases/latest";

        try
        {
            string json = await client.GetStringAsync(url);
            JsonDocument doc = JsonDocument.Parse(json);
            string latestTag = doc.RootElement.GetProperty("tag_name").GetString();

            return string.IsNullOrWhiteSpace(latestTag) ? null : latestTag.TrimStart('v');
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> IsNewUpdateAsync(string currentVersion)
    {
        string latestTag = await GetLatestVersionAsync();
        if (latestTag == null) return false;

        Version latest = Version.Parse(latestTag);
        Version current = Version.Parse(currentVersion.TrimStart('v'));

        return latest > current;
    }

    public static async Task<string> DownloadLatestReleaseAsync()
    {
        string url = "https://api.github.com/repos/maks535/FurryProgramm_History/releases/latest";

        try
        {
            string json = await client.GetStringAsync(url);

            JsonDocument doc = JsonDocument.Parse(json);

            JsonElement assets = doc.RootElement.GetProperty("assets");

            if (assets.GetArrayLength() == 0)
                return null;

            JsonElement asset = assets[0];

            string fileName = asset.GetProperty("name").GetString();
            string downloadUrl = asset.GetProperty("browser_download_url").GetString();

            string downloadsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");

            string savePath = Path.Combine(downloadsFolder, fileName);

            string path = await Updater.DownloadLatestReleaseAsync();

            if (path != null)
            {
                Console.WriteLine($"Скачано: {path}");
            }
            else
            {
                Console.WriteLine("Ошибка скачивания");
            }

            return savePath;
        }
        catch
        {
            return null;
        }
    }
}




class Program
{

    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_RESTORE = 9;

    static Random rnd = new Random();

    static string exeDir = AppDomain.CurrentDomain.BaseDirectory;
    static string presetsFile = Path.Combine(exeDir, "presets.fpf");
    static bool randomMain = false;
    static bool ConstRandomFG;
    static bool Appautorun = false;
    static bool mainIsHidden = false;
    static ConsoleColor mainFGC = Console.ForegroundColor;
    static ConsoleColor mainBGC = Console.BackgroundColor;

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        string[] args = Environment.GetCommandLineArgs();
        bool LaunchAdmin = args != null && Array.Exists(args, a => a.Equals("-admin", StringComparison.OrdinalIgnoreCase));         //---------Аргумент для запуска в режиме администратора
        bool HomeDir = args != null && Array.Exists(args, a => a.Equals("-homedir", StringComparison.OrdinalIgnoreCase));           //---------аргумент для запуска в домашней директории
        bool Converter = args != null && Array.Exists(args, a => a.Equals("-converter", StringComparison.OrdinalIgnoreCase));       //---------аргумент для запуска в режиме конвертера
        bool IsHidden = args != null && Array.Exists(args, a => a.Equals("-hidden", StringComparison.OrdinalIgnoreCase));           //---------аргумент для превращения фона и текста в чёрный цвет
        bool isTester = args != null && Array.Exists(args, a => a.Equals("-tester", StringComparison.OrdinalIgnoreCase));           //---------аргумент для превращения фона и текста в чёрный цвет
        bool SkipCheckUpdate = args != null && Array.Exists(args, a => a.Equals("-NoUpdate", StringComparison.OrdinalIgnoreCase));  //---------Аргумент для пропуска проверки наличия новой версии
        string cmdArg = args.FirstOrDefault(a => a.StartsWith("-c:", StringComparison.OrdinalIgnoreCase));                          //---------аргумент для Ввода команды в консоль (-c:"CMD")
        string[] requestedApps = Array.Empty<string>();                                                                             //---------аргумент для запуска приложений


        if (cmdArg != null)
        {
            string command = cmdArg.Replace("-c:", "").Trim('"');
            string appCommand = command;
            ExecuteCommand(command, mainBGC, mainFGC, randomMain);
        }

        if (Converter)
        {
            LaunchConverter();
        }

        if (args != null)
        {
            string appArg = args.FirstOrDefault(a => a.StartsWith("-app:", StringComparison.OrdinalIgnoreCase)); //------------Аргумент для запуска приложений
            if (appArg != null)
            {
                Appautorun = true;
                requestedApps = appArg
                    .Replace("-app:", "") // убираем префикс
                    .Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .ToArray();
            }
        }

        if (IsHidden)
        {
            mainBGC = ConsoleColor.Black;
            mainFGC = ConsoleColor.Black;
            mainIsHidden = true;
            Console.ForegroundColor = mainFGC;
            Console.BackgroundColor = mainBGC;
        }
        else
        {
            ConstRandomFG = LoadStyleCMD();
            randomMain = ConstRandomFG;
            mainFGC = Console.ForegroundColor;
            mainBGC = Console.BackgroundColor;
        }
        

        TryFindDublicate(HomeDir, Converter);

        if (IsRunningAsAdministrator())
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Запущено в режиме администратора");

        }
        if (isTester)
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Запущено в режиме эксперементальных возможностей");
        }

        if (!IsRunningAsAdministrator() && LaunchAdmin == true)
        {
            launchAsAdministrator();
        }

        SetConsoleColor(mainBGC, mainFGC, randomMain);

        string folderPath = GetOrCreateFolderPath();

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("Папка не найдена.");
            return;
        }
        else
        {
            if (IsHidden)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Режим Hiddenn включён");
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Console.WriteLine("Текущая дата: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\nДля вызова списка комманд введите: '?'");
            if (!SkipCheckUpdate)
            {
                CheckUpdate().Wait();
            }
        }
        if (Appautorun)
        {
            string[] files = Directory.GetFiles(folderPath);
            LaunchFiles(requestedApps, files);
        }

        while (true)
        {
            SetConsoleColor(mainBGC, mainFGC, randomMain);
            Console.Write(">>> ");
            string input = Console.ReadLine()?.Trim();
            string[] files = Directory.GetFiles(folderPath);

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Clear();
                Console.WriteLine("Доступные файлы:");

                for (int i = 0; i < files.Length; i++)
                {
                    SetConsoleColor(mainBGC, mainFGC, randomMain);
                    Console.WriteLine(Path.GetFileNameWithoutExtension(files[i]));
                }
                Console.WriteLine("\n");
                continue;
            }

            //-----------------------------------------Тестовые функции-------------------------------------
            if (isTester)
            {
                if (input.StartsWith("-karaoke ") && isTester)
                {
                    MadeKaraokeSound(input);
                    continue;
                }
            }
            //-----------------------------------------Тестовые функции-------------------------------------

            if (input.StartsWith("?"))
            {
                Console.Clear();
                Console.WriteLine("Чтобы вывести список доступных приложений, нажмите 'Enter'");
                Console.WriteLine("Чтобы запустить приложение, просто введите его название(не обязательно полностью)");
                Console.WriteLine("\n\nДоступные команды:\n\n-p [имя пресета] --> Загружает пресет");
                Console.WriteLine("-s [имя пресета] [названия приложений через ':'] --> Сохраняет пресет");
                Console.WriteLine("-с [Команда в консоль] --> Напрямую выполняет команду в CMD");
                Console.WriteLine("\nТак же можно запускать приложения через ':'. Пример: Steam:Discord");
                Console.WriteLine("Писать полные названия приложений не требуется. Пример: stea:dis");
                Console.WriteLine("\n-style --> Позволяет выбрать стиль для консоли");
                Console.WriteLine("\n-convert/-con --> переводит программу в режим конвертора audio/video файлов\n");
            }
            else if (input.StartsWith("-p "))
            {
                string presetName = input.Substring(3).Trim();
                LoadPreset(presetName, files);
            }
            else if (input.StartsWith("-s "))
            {
                string[] parts = input.Substring(3).Split(' ');
                if (parts.Length < 2)
                {
                    Console.WriteLine("Неправильный формат команды сохранения.");
                    continue;
                }

                string presetName = parts[0];
                string presetData = string.Join(":", parts.Skip(1));

                SavePreset(presetName, presetData);
            }
            else if (input.StartsWith("-c "))
            {
                string command = input.Substring(3).Trim();
                ExecuteCommand(command, mainBGC, mainFGC, randomMain);
            }
            else if (input.StartsWith("-style"))
            {
                Console.Clear();
                StyleForProgram();
            }
            else if (input.StartsWith("-convert") || input.StartsWith("-con"))
            {
                Console.Clear();
                LaunchConverter();
            }
            else if (input == "Exit" || input == "exit")
            {
                return;
            }
            else
            {
                LaunchFiles(input.Split(':'), files);
            }
        }
    }
    static void StyleForProgram()
    {
        bool random = false;
        ConsoleColor originalBg = Console.BackgroundColor;
        ConsoleColor originalFg = Console.ForegroundColor;
        string[] MassivNamesBackgroundColors = {"Red", "Green", "Yellow", "White",
            "Gray", "Black", "Blue", "Cyan", "DarkBlue",
            "DarkCyan", "DarkGray", "DarkGreen", "DarkMagenta", "DarkRed", "DarkYellow"};

        string[] MassivNamesForegroundColors = {
        "Red", "Green", "Yellow", "White",
        "Gray", "Black", "Blue", "Cyan", "DarkBlue",
        "DarkCyan", "DarkGray", "DarkGreen", "DarkMagenta", "DarkRed", "DarkYellow"
};
        while (true)
        {
            SetConsoleColor(mainBGC, mainFGC, randomMain);
            Console.WriteLine("*Включён режим редактирования стилей*");
            Console.WriteLine("Выберите что редактировать: Background/Foreground или Exit");



            string stylechoose = Console.ReadLine();
            switch (stylechoose)
            {
                case "Background":
                case "background":
                case "B":
                case "b":
                    Console.Clear();
                    bool IsBGchoose = false;
                    while (!IsBGchoose)
                    {
                        bool Isbg = true;
                        Console.WriteLine("Выберите цвет Фона:");
                        Console.WriteLine("0: Отмена");
                        WriteColored("1: Red", ConsoleColor.Red,Isbg);
                        WriteColored("2: Green", ConsoleColor.Green, Isbg);
                        WriteColored("3: Yellow", ConsoleColor.Yellow, Isbg);
                        WriteColored("4: White", ConsoleColor.White, Isbg);
                        WriteColored("5: Gray", ConsoleColor.Gray, Isbg);
                        WriteColored("6: Black", ConsoleColor.Black, Isbg);
                        WriteColored("7: Blue", ConsoleColor.Blue, Isbg);
                        WriteColored("8: Cyan", ConsoleColor.Cyan, Isbg);
                        WriteColored("9: DarkBlue", ConsoleColor.DarkBlue, Isbg);
                        WriteColored("10: DarkCyan", ConsoleColor.DarkCyan, Isbg);
                        WriteColored("11: DarkGray", ConsoleColor.DarkGray, Isbg);
                        WriteColored("12: DarkGreen", ConsoleColor.DarkGreen, Isbg);
                        WriteColored("13: DarkMagenta", ConsoleColor.DarkMagenta, Isbg);
                        WriteColored("14: DarkRed", ConsoleColor.DarkRed, Isbg);
                        WriteColored("15: DarkYellow", ConsoleColor.DarkYellow, Isbg);

                        try
                        {
                            Console.Write(">>> ");
                            int chooseBColor = Convert.ToInt32(Console.ReadLine());
                            if (chooseBColor == 0)
                            {
                                IsBGchoose = true;
                                Console.Clear();
                                continue;
                            }
                            else if (chooseBColor < 1 || chooseBColor > MassivNamesBackgroundColors.Length)
                            {
                                Console.Clear();
                                Console.WriteLine("Ваше число не верно");
                                continue;
                            }

                            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), MassivNamesBackgroundColors[chooseBColor - 1]);
                            Console.Clear();
                            Console.WriteLine("*Цвет фона успешно применён*");
                            IsBGchoose = true;
                        }
                        catch
                        {
                            Console.Clear();
                            Console.WriteLine("Неверный ввод");
                        }
                    }
                    break;

                case "Foreground":
                case "foreground":
                case "F":
                case "f":
                    Console.Clear();
                    bool IsFGchoose = false;
                    while (!IsFGchoose)
                    {
                        bool Isbg = false;
                        Console.WriteLine("Выберите цвет текста:");
                        Console.WriteLine("0: Отмена");
                        WriteColored("1: Red", ConsoleColor.Red, Isbg);
                        WriteColored("2: Green", ConsoleColor.Green, Isbg);
                        WriteColored("3: Yellow", ConsoleColor.Yellow, Isbg);
                        WriteColored("4: White", ConsoleColor.White, Isbg);
                        WriteColored("5: Gray", ConsoleColor.Gray, Isbg);
                        WriteColored("6: Black", ConsoleColor.Black, Isbg);
                        WriteColored("7: Blue", ConsoleColor.Blue, Isbg);
                        WriteColored("8: Cyan", ConsoleColor.Cyan, Isbg);
                        WriteColored("9: DarkBlue", ConsoleColor.DarkBlue, Isbg);
                        WriteColored("10: DarkCyan", ConsoleColor.DarkCyan, Isbg);
                        WriteColored("11: DarkGray", ConsoleColor.DarkGray, Isbg);
                        WriteColored("12: DarkGreen", ConsoleColor.DarkGreen, Isbg);
                        WriteColored("13: DarkMagenta", ConsoleColor.DarkMagenta, Isbg);
                        WriteColored("14: DarkRed", ConsoleColor.DarkRed, Isbg);
                        WriteColored("15: DarkYellow", ConsoleColor.DarkYellow, Isbg);
                        PrintRainbowText("16: Переменный");
                        try
                        {
                            Console.Write(">>> ");
                            int chooseFColor = Convert.ToInt32(Console.ReadLine());
                            if (chooseFColor == 0)
                            {
                                IsFGchoose = true;
                                Console.Clear();
                                continue;
                            }
                             else if (chooseFColor == MassivNamesForegroundColors.Length + 1)
                            {
                                random = true;
                                IsBGchoose = true;
                                Console.Clear();
                                Console.WriteLine("Переменный цвет будет динамически менятся на главной странице");
                                break;
                            }
                            else if (chooseFColor < 1 || chooseFColor > MassivNamesForegroundColors.Length)
                            {
                                Console.Clear();
                                Console.WriteLine("Неверный ввод\n");
                            }
                            else {
                                randomMain = false;
                                random = false;
                                Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), MassivNamesForegroundColors[chooseFColor - 1]);
                                Console.Clear();
                                mainFGC = Console.ForegroundColor;
                                Console.WriteLine("*Цвет текста успешно применён*");
                                IsFGchoose = true;
                            }
                            
                        }
                        catch
                        {
                            Console.Clear();
                            Console.WriteLine("Неверный ввод\n");
                        }
                    }
                    break;

                case "Exit":
                case "exit":
                case "E":
                case "e":
                    SaveStyleCMD(originalBg, originalFg, random);
                    Console.Clear();
                    return;

                default:
                    Console.Clear();
                    Console.WriteLine("Действие не распознано. Попробуйте снова.\n");
                    break;
            }
        }

    }
    static bool LoadStyleCMD()
    {
        bool random = false;
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.fpf");

        if (!File.Exists(filePath))
        {
            return false;
        }

        string[] lines = File.ReadAllLines(filePath);
        string backgroundColor = null;
        string foregroundColor = null;

        foreach (string line in lines)
        {
            if (line.StartsWith("Background="))
                backgroundColor = line.Substring("Background=".Length);
            else if (line.StartsWith("Foreground="))
                foregroundColor = line.Substring("Foreground=".Length);
        }

        try
        {
            if (backgroundColor != null)
            {
                Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), backgroundColor);
            }
            if (foregroundColor != null)
            {
                if (foregroundColor.Equals("random", StringComparison.OrdinalIgnoreCase))
                {
                    SetRandomConsoleFGColor();
                    return true;
                }
                else
                {
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), foregroundColor);
                }
            }
        }
        catch
        {
            Console.WriteLine("Ошибка при применении настроек. Проверьте содержимое файла config.fpf.");
        }
        return random;
    }
    static void SaveStyleCMD(ConsoleColor originalBg, ConsoleColor originalFg, bool random)
    {
        Console.Clear();
        if (random)
        {
            Console.Clear();
            string backgroundColor = Console.BackgroundColor.ToString();
            string foregroundColor = "random";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.fpf");
            bool bgFound = false;
            bool fgFound = false;
            string[] existingLines = File.Exists(filePath)
                            ? File.ReadAllLines(filePath)
                            : new string[0];

            for (int i = 0; i < existingLines.Length; i++)
            {
                if (existingLines[i].StartsWith("Background="))
                {
                    existingLines[i] = $"Background={backgroundColor}";
                    bgFound = true;
                }
                else if (existingLines[i].StartsWith("Foreground="))
                {
                    existingLines[i] = $"Foreground={foregroundColor}";
                    fgFound = true;
                }
            }
            string[] newLines;
            if (random)
            {
                Console.Clear();
                Console.WriteLine("N - отменить изменения\nY - Сохранить и применить\nC - Применить");
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.Y:
                        {
                            int extraLines = (!bgFound ? 1 : 0) + (!fgFound ? 1 : 0);
                            newLines = new string[existingLines.Length + extraLines];
                            Array.Copy(existingLines, newLines, existingLines.Length);

                            int index = existingLines.Length;
                            if (!bgFound)
                                newLines[index++] = $"Background={backgroundColor}";
                            if (!fgFound)
                                newLines[index++] = $"Foreground={foregroundColor}";
                            randomMain = true;
                            File.WriteAllLines(filePath, newLines);
                        }
                        break;
                    case ConsoleKey.C: 
                        randomMain = true;
                        Console.Clear();
                        Console.WriteLine("Стиль применён");
                        break;

                    default:
                        Console.WriteLine("Отмена");
                        Console.ReadLine();
                        break;
                }

            }
            else if (!bgFound || !fgFound)
            {
                int extraLines = (!bgFound ? 1 : 0) + (!fgFound ? 1 : 0);
                newLines = new string[existingLines.Length + extraLines];
                Array.Copy(existingLines, newLines, existingLines.Length);

                int index = existingLines.Length;
                if (!bgFound)
                    newLines[index++] = $"Background={backgroundColor}";
                if (!fgFound)
                    newLines[index++] = $"Foreground={foregroundColor}";
                File.WriteAllLines(filePath, newLines);
            }
            else
            {
                newLines = existingLines;
                File.WriteAllLines(filePath, newLines);
            }



            Console.WriteLine("*Настройки сохранены*");
            Console.WriteLine($"randomColor = {randomMain}");
            return;
        }
        else if (originalFg != Console.ForegroundColor || originalBg != Console.BackgroundColor && 1 == 1)
        {
            Console.WriteLine("Использовать эти настройки при следующем запуске?");
            string ChooseStyleSettings = Console.ReadLine();

            while (true)
            {
                switch (ChooseStyleSettings)
                {
                    case "Yes":
                    case "yes":
                    case "Y":
                    case "y":
                        Console.Clear();
                        string backgroundColor = Console.BackgroundColor.ToString();
                        string foregroundColor = Console.ForegroundColor.ToString();

                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.fpf");

                        string[] existingLines = File.Exists(filePath)
                            ? File.ReadAllLines(filePath)
                            : new string[0];

                        bool bgFound = false;
                        bool fgFound = false;

                        for (int i = 0; i < existingLines.Length; i++)
                        {
                            if (existingLines[i].StartsWith("Background="))
                            {
                                existingLines[i] = $"Background={backgroundColor}";
                                bgFound = true;
                            }
                            else if (existingLines[i].StartsWith("Foreground="))
                            {
                                existingLines[i] = $"Foreground={foregroundColor}";
                                fgFound = true;
                            }
                        }

                        string[] newLines;
                        if (!bgFound || !fgFound)
                        {
                            int extraLines = (!bgFound ? 1 : 0) + (!fgFound ? 1 : 0);
                            newLines = new string[existingLines.Length + extraLines];
                            Array.Copy(existingLines, newLines, existingLines.Length);

                            int index = existingLines.Length;
                            if (!bgFound)
                                newLines[index++] = $"Background={backgroundColor}";
                            if (!fgFound)
                                newLines[index++] = $"Foreground={foregroundColor}";
                        }
                        else
                        {
                            newLines = existingLines;
                        }

                        File.WriteAllLines(filePath, newLines);

                        Console.WriteLine("*Настройки сохранены*");
                        return;

                    case "No":
                    case "no":
                    case "N":
                    case "n":
                        Console.Clear();
                        return;

                    default:
                        Console.Clear();
                        Console.WriteLine("Возможные ответы: Yes, yes, Y, y, No, no, N, n");
                        ChooseStyleSettings = Console.ReadLine();
                        break;
                }
            }
        }
    }
    static void ExecuteCommand(string command, ConsoleColor BGcolor, ConsoleColor FGColor, bool random)
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.WriteLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Data);
                };

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
                SetConsoleColor(BGcolor, FGColor, random);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при выполнении команды: {ex.Message}");
        }
    }
    static string GetOrCreateFolderPath()
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string configFile = Path.Combine(exeDir, "config.fpf");
        int i = 0;

        try
        {
            string[] lines = File.Exists(configFile)
                ? File.ReadAllLines(configFile)
                : new string[0];

            if (lines.Length > 0)
            {
                string savedPath = lines[0].Trim();
                if (Directory.Exists(savedPath))
                    return savedPath;

                Console.Clear();
                Console.WriteLine("Сохранённый путь не существует. Введите его корректно");
            }

            Console.Write("Введите путь к папке: ");
            string folderPath = Console.ReadLine()?.Trim();

            while (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                Console.Clear();
                i++;

                Console.WriteLine("Неверный путь. Попробуйте ещё раз");
                if (i >= 2)
                {
                    Console.WriteLine("Если нужна помощь, обратитесь к Фембою");
                }
                Console.Write("Введите путь к папке: ");
                folderPath = Console.ReadLine()?.Trim();
            }

            string[] newLines;
            if (lines.Length > 1)
            {
                newLines = new string[lines.Length];
                newLines[0] = folderPath;
                Array.Copy(lines, 1, newLines, 1, lines.Length - 1);
            }
            else
            {
                newLines = new string[] { folderPath };
            }

            File.WriteAllLines(configFile, newLines);
            Console.Clear();
            Console.WriteLine("*Папка найдена*");
            return folderPath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine("Целевая папка для записи: " + exeDir);
            Console.WriteLine("Попробуйте флаг запуска -homedir, если не поможет то -admin");
            Console.ReadKey();
            return string.Empty;
        }
    }
    static void LaunchFiles(string[] requestedFiles, string[] files)
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        Console.Clear();
        foreach (string fileName in requestedFiles)
        {
            string[] matches = files.Where(f => Path.GetFileName(f).ToLower().Contains(fileName.ToLower().Trim())).ToArray();

            if (matches.Length == 1)
            {
                LaunchFile(matches[0]);
            }
            else if (matches.Length > 1)
            {
                Console.WriteLine($"Найдено несколько файлов для \"{fileName}\":");
                Console.WriteLine($"[0] - отмена");
                for (int i = 0; i < matches.Length; i++)
                {
                    SetConsoleColor(mainBGC, mainFGC, randomMain);
                    Console.WriteLine($"[{i + 1}] {Path.GetFileName(matches[i])}");
                }
                SetConsoleColor(mainBGC, mainFGC, randomMain);
                Console.Write("Выберите номер файла: ");
                try
                {
                    int choice = Convert.ToInt32(Console.ReadLine());
                    if (choice == 0)
                    {
                        Console.Clear();
                        break;
                    }
                    else if (choice > 0 && choice <= matches.Length)
                    {
                        Console.Clear();
                        LaunchFile(matches[choice - 1]);
                    }
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Нет совпадений");
                    }
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Ошибка ввода");
                }
            }
            else
            {
                Console.WriteLine($"Файл \"{fileName}\" не найден");
            }

        }
    }
    static void SavePreset(string presetName, string presetData)
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        Console.Clear();

        string[] lines = File.Exists(presetsFile) ? File.ReadAllLines(presetsFile) : Array.Empty<string>();
        bool presetExists = false;

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(presetName + ":"))
            {
                Console.WriteLine("Обнаружено совпадение в имени, производится перезапись...");
                lines[i] = $"{presetName}:{presetData}";
                presetExists = true;
                break;
            }
        }

        if (!presetExists)
        {
            var updatedLines = lines.ToList();
            updatedLines.Add($"{presetName}:{presetData}");
            lines = updatedLines.ToArray();
        }

        File.WriteAllLines(presetsFile, lines);
        Console.WriteLine($"Пресет \"{presetName}\" сохранен");
    }
    static void LoadPreset(string presetName, string[] files)
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        Console.Clear();
        if (!File.Exists(presetsFile))
        {
            Console.WriteLine("Файл с пресетами не найден");
            return;
        }

        var lines = File.ReadAllLines(presetsFile);
        var presetLine = lines.FirstOrDefault(line => line.StartsWith($"{presetName}:"));

        if (presetLine != null)
        {
            string[] presetFiles = presetLine.Substring(presetName.Length + 1).Split(':');
            LaunchFiles(presetFiles, files);
        }
        else
        {
            Console.WriteLine($"Пресет \"{presetName}\" не найден");
        }
    }
    static void LaunchFile(string filePath)
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден.");
            return;
        }

        string extension = Path.GetExtension(filePath)?.ToLower();
        if (extension != ".bat")
        {
            Console.WriteLine($"Запуск {Path.GetFileNameWithoutExtension(filePath)}...");
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ошибка запуска файла: {ex.Message}");
                if (extension == ".lnk")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    try
                    {
                        var shell = new IWshRuntimeLibrary.WshShell();
                        var shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(filePath);

                        Console.WriteLine($"Ярлык ведёт к: {shortcut.TargetPath}");

                        if (!string.IsNullOrEmpty(shortcut.Arguments))
                            Console.WriteLine($"Аргументы: {shortcut.Arguments}");

                        if (!string.IsNullOrEmpty(shortcut.WorkingDirectory))
                            Console.WriteLine($"Рабочая папка: {shortcut.WorkingDirectory}");
                        Console.WriteLine("Удалить этот файл? Y/N");
                        var key = Console.ReadKey().Key;
                        Console.Clear();

                        if (key == ConsoleKey.Y)
                        {
                            try
                            {
                                File.Delete(filePath);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Файл успешно удалён.");
                            }
                            catch (Exception delEx)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Ошибка при удалении файла: {delEx.Message}");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine("Файл не был удалён.");
                        }
                        SetConsoleColor(mainBGC, mainFGC, randomMain);
                    }
                    catch (Exception shortcutEx)
                    {
                        Console.WriteLine($"Не удалось прочитать ярлык: {shortcutEx.Message}");
                    }
                }
            }
            return;

        }

        string fpText = null;
        string launchMode = "default"; // default, fpviev, fpcmdconst
        List<string> foundModes = new List<string>();

        try
        {
            foreach (var line in File.ReadLines(filePath))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("::FPtext="))
                {
                    fpText = trimmed.Substring("::FPtext=".Length).Trim();
                }
                if (trimmed.Contains("::FPVIEV"))
                {
                    foundModes.Add("fpviev");
                }
                if (trimmed.Contains("::FPCMDConst"))
                {
                    foundModes.Add("fpcmdconst");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка чтения батника: {ex.Message}");
            return;
        }

        if (foundModes.Count > 1)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Внимание: Обнаружены противоречивые команды запуска: " +
                string.Join(", ", foundModes.Select(m => $"::{m.ToUpper()}")) +
                $"\nБудет выполнена первая: ::{foundModes[0].ToUpper()}");
        }

        if (foundModes.Count > 0)
            launchMode = foundModes[0];

        if (string.IsNullOrEmpty(fpText))
        {
            Console.WriteLine("Подсказка: Вывод батника можно изменить, например вместо стандартного " +
                $"Запуск {Path.GetFileNameWithoutExtension(filePath)}..." +
                "\nМожно вывести 'Выключаю компьютер'" +
                "\nДля этого надо в батник написать строку '::FPtext=Выключаю компьютер'" +
                "\nТакже доступны режимы: '::FPVIEV' — вывод сюда, '::FPCMDConst' — отдельная консоль\n");

            fpText = $"Запуск {Path.GetFileNameWithoutExtension(filePath)}...";
        }

        switch (launchMode)
        {
            case "fpviev":
                Console.WriteLine(fpText);
                Console.ResetColor();
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C \"{filePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };
                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        Console.WriteLine(process.ExitCode == 0
                            ? "\n\n\nБатник завершился успешно."
                            : $"\n\n\nБатник завершился с ошибкой. Код: {process.ExitCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка запуска батника: {ex.Message}");
                }
                break;

            case "fpcmdconst":
                Console.WriteLine(fpText);
                Console.ResetColor();
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/K \"{filePath}\"",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка запуска в отдельной консоли: {ex.Message}");
                }
                break;

            default:
                Console.WriteLine(fpText);
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка фонового запуска батника: {ex.Message}");
                }
                break;
        }
    }
    static bool IsFfmpegAvailable()
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        Console.WriteLine("Идёт проверка наличия FFmpeg...");
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }
        catch
        {
            return false;
        }
    }
    static void InstallFfmpegWithWinget()
    {
        Console.WriteLine("FFmpeg не установлен. Хотите установить его через winget? (y/n): ");
        Console.WriteLine("Примечание: Будет запрошен доступ администратора. так же будет использование примерно 150–200 МБ трафика");
        string response = Console.ReadLine()?.Trim().ToLower();

        if (response == "y" || response == "yes")
        {
            try
            {
                Console.WriteLine("Запуск установки FFmpeg через winget...");
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c winget install ffmpeg -e --accept-package-agreements --accept-source-agreements",
                    UseShellExecute = true,
                    Verb = "runas"
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при запуске установки: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Установка отменена пользователем.");
        }
    }
    static void LaunchConverter()
    {
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string batsFolder = Path.Combine(appDirectory, "ConvertBats");

        if (!IsFfmpegAvailable())
        {
            InstallFfmpegWithWinget();
            return;
        }
        else
        {
            Console.WriteLine("Статус FFmpeg - ОК");
        }


        if (!Directory.Exists(batsFolder))
        {
            Console.WriteLine("Папка ConvertBats не найдена. Убедитесь, что она существует в директории приложения.");
            
            while (true)
            {
                Console.WriteLine("Режим ограниченной конвертации активен");
                Console.Write(">>> ");
                string conversionName = Console.ReadLine()?.Trim();
                if (conversionName.StartsWith("?"))
                {
                    Console.Clear();
                    Console.WriteLine("Для вывода спектрограммы песни на рабочем столе надо прописать следующие строки:");
                    Console.WriteLine("-s/-spectr/-spectrogram имя_файла_с_расширением SD/HD/4K");
                    Console.WriteLine("Для перехода в режим редактирования громкости введите -a audio.mp3");
                    Console.WriteLine("Для выхода введите 'exit'");
                }
                else if (conversionName.StartsWith("-spectr ") || conversionName.StartsWith("-s ") || conversionName.StartsWith("-spectrogram "))
                {
                    PaintSpectrogram(conversionName);
                }
                else if (conversionName.StartsWith("-spectr ") || conversionName.StartsWith("-s ") || conversionName.StartsWith("-spectrogram "))
                {
                    PaintSpectrogram(conversionName);
                }
                else if (conversionName.StartsWith("-a ") || conversionName.StartsWith("-audio "))
                {
                    HandleAudio(conversionName);
                }
                else if (conversionName.StartsWith("exit") || conversionName.StartsWith("e") || conversionName.StartsWith("E"))
                {
                    Console.Clear();
                    break;
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Ввод не распознан (? для справки)");
                }
            }
            return;
        }

        while (true)
        {
            Console.WriteLine("Режим конвертиции (? для справки)");
            Console.Write(">>> ");
            string conversionName = Console.ReadLine()?.Trim();

            if (conversionName.StartsWith("?"))
            {
                Console.Clear();
                Console.WriteLine("Для того чтобы сконвертировать файл, надо назвать его 'file', и поместить на рабочий стол");
                Console.WriteLine("После вы вводите ваше расширение и целевое, например 'MKVtoMP4'");
                Console.WriteLine("Для вывода списка доступных конвертаций нажмите Enter");
                Console.WriteLine("Для вывода спектрограммы песни на рабочем столе надо прописать следующие строки:");
                Console.WriteLine("-s/-spectr/-spectrogram имя_файла_с_расширением SD/HD/4K");
                Console.WriteLine("Для перехода в режим редактирования громкости введите -a audio.mp3");
                Console.WriteLine("Для выхода введите 'exit'");
            }
            else if (conversionName.StartsWith("exit") || conversionName.StartsWith("e") || conversionName.StartsWith("E"))
            {
                Console.Clear();
                return;
            }
            else if (conversionName.StartsWith("-spectr ") || conversionName.StartsWith("-s ") || conversionName.StartsWith("-spectrogram "))
            {
                PaintSpectrogram(conversionName);
            }
            else if (conversionName.StartsWith("-s") || conversionName.StartsWith("-spectr") || conversionName.StartsWith("-spectrogram"))
            {
                Console.Clear();
                Console.WriteLine($"Ошибка ввода, пример правильного ввода: -s \"audio.mp3\" HD");
            }
            else if (string.IsNullOrEmpty(conversionName))
            {
                Console.Clear();
                Console.WriteLine("Доступные режимы конвертации:\n");
                string[] files = Directory.GetFiles(batsFolder);
                foreach (string file in files)
                {
                    if (Path.GetExtension(file).Equals(".bat", StringComparison.OrdinalIgnoreCase))
                    {
                        SetConsoleColor(mainBGC, mainFGC, randomMain);
                        Console.WriteLine(Path.GetFileNameWithoutExtension(file));
                    }
                }
                Console.WriteLine("\n\n");
                continue;
            }
            else if (conversionName.StartsWith("-a ") || conversionName.StartsWith("-audio "))
            {
                HandleAudio(conversionName);
            }
            else
            {
                string batFilePath = Path.Combine(batsFolder, conversionName + ".bat");

                if (File.Exists(batFilePath))
                {
                    try
                    {
                        Console.WriteLine($"Запуск конвертации по принципу {conversionName}...");
                        using (Process process = new Process())
                        {
                            process.StartInfo = new ProcessStartInfo
                            {
                                FileName = batFilePath,
                                UseShellExecute = false
                            };

                            process.Start();
                            process.WaitForExit();
                            Console.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при запуске батника: {ex.Message}");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"Ошибка в вводе: '{conversionName}'");
                }
            }
        }
    }
    static void TryFindDublicate(bool HomeDir, bool converter)
    {
        if (!IsRunningAsAdministrator() || converter)
        {
            if (HomeDir)
            {
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                string currentProcessName = Process.GetCurrentProcess().ProcessName;
                Process[] running = Process.GetProcessesByName(currentProcessName);
                if (running.Length > 1)
                {
                    foreach (Process proc in running)
                    {
                        if (proc.Id != Process.GetCurrentProcess().Id)
                        {
                            IntPtr hWnd = proc.MainWindowHandle;
                            if (hWnd != IntPtr.Zero)
                            {
                                ShowWindow(hWnd, SW_RESTORE);
                                SetForegroundWindow(hWnd);
                            }
                            Environment.Exit(0);// Завершаем текущий экземпляр
                        }
                    }
                }
            }
        }
    }
    static bool IsRunningAsAdministrator()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
    static void launchAsAdministrator()
    {
        try
        {
            string exePath = Assembly.GetEntryAssembly().Location;

            var startInfo = new ProcessStartInfo(exePath)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при перезапуске: " + ex.Message);
        }
    }
    static void SetRandomConsoleFGColor()
    {
        var colors = Enum.GetValues(typeof(ConsoleColor));
        ConsoleColor randomColor;

        do
        {
            randomColor = (ConsoleColor)colors.GetValue(rnd.Next(colors.Length));
        }
        while (randomColor == Console.BackgroundColor
            || randomColor == Console.ForegroundColor);

        Console.ForegroundColor = randomColor;
    }
    static void WriteColored(string text, ConsoleColor color, bool IsBG)
    {
        if (!IsBG)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        else
        {
            Console.BackgroundColor = color;
            Console.WriteLine(text);
            SetConsoleColor(mainBGC, mainFGC, randomMain);
        }
        
    }
    static void PrintRainbowText(string text)
    {
        ConsoleColor[] colors = new ConsoleColor[]
        {
            ConsoleColor.Red,
            ConsoleColor.Green,
            ConsoleColor.Yellow,
            ConsoleColor.Blue,
            ConsoleColor.Magenta,
            ConsoleColor.Cyan,
            ConsoleColor.White
        };

        int colorIndex = 0;

        foreach (char c in text)
        {
            Console.ForegroundColor = colors[colorIndex];
            Console.Write(c);
            colorIndex = (colorIndex + 1) % colors.Length;
        }
        SetConsoleColor(mainBGC, mainFGC, randomMain);
        Console.WriteLine();
    }
    static void SetConsoleColor(ConsoleColor BGcolor, ConsoleColor FGColor, bool random)
    {
        if (random)
        {
            SetRandomConsoleFGColor();
        }
        else
        {
            Console.BackgroundColor = BGcolor;
            Console.ForegroundColor = FGColor;
        }
    }
    static void HandleAudio(string conversionName)
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // Разбор аргументов с кавычками
            var matches = Regex.Matches(conversionName, @"[\""].+?[\""]|[^ ]+");
            string[] parsedArgs = matches.Cast<Match>().Select(m => m.Value.Trim('"')).ToArray();

            if (parsedArgs.Length < 2)
            {
                Console.WriteLine("Укажите аудио файл. Пример: -audio \"audio.flac\"");
                return;
            }

            string inputFile = Path.Combine(desktopPath, parsedArgs[1]);
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Файл {inputFile} не найден на рабочем столе.");
                return;
            }

            // Анализ аудио через FFmpeg (пики и средняя громкость)
            string ffmpegArgs = $"-i \"{inputFile}\" -af volumedetect -f null NUL";
            var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = ffmpegArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // Извлекаем значения
            double maxVolume = ExtractValue(output, "max_volume");
            double meanVolume = ExtractValue(output, "mean_volume");
            if (mainIsHidden)
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            Console.Clear();
            Console.WriteLine($"Файл: {parsedArgs[1]}");
            Console.WriteLine($"Максимальная громкость: {maxVolume} dB");
            Console.WriteLine($"Средняя громкость: {meanVolume} dB");
            SetConsoleColor(mainBGC, mainFGC, randomMain);


            if (maxVolume < -0.5)
            {
                Console.WriteLine($"Можно поднять примерно на +{-1 * maxVolume - 0.5:F1} dB без клипинга.");
                Console.Write($"Введите значение усиления/ослабления: ");
            }
            else if (maxVolume > 0)
            {
                Console.WriteLine($"Рекомендуется опустить примерно на +{maxVolume - 0.5:F1} dB чтобы избежать клипинга.");
                Console.Write($"Введите значение усиления/ослабления: ");
            }
            else
            {
                Console.WriteLine("Громкость уже находится в хорошем диапазоне, изменять не рекомендуется.");
                Console.Write($"Введите значение усиления/ослабления: ");
            }


            
            string userInput = Console.ReadLine()?.Trim();

            if (!double.TryParse(userInput,
                NumberStyles.AllowLeadingSign | NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double userGain))
            {
                Console.WriteLine("Некорректный ввод.");
                return;
            }

            // Формируем имя выходного файла
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputFile);
            string extension = Path.GetExtension(inputFile);
            string outputFile = Path.Combine(desktopPath, fileNameWithoutExt + "_edited" + extension);

            // Применяем изменение громкости
            string ffmpegEditArgs = $"-y -i \"{inputFile}\" -af \"volume={userGain.ToString(CultureInfo.InvariantCulture)}dB\" \"{outputFile}\"";


            var processEdit = new Process();
            processEdit.StartInfo.FileName = "ffmpeg";
            processEdit.StartInfo.Arguments = ffmpegEditArgs;
            processEdit.StartInfo.UseShellExecute = false;
            processEdit.StartInfo.RedirectStandardError = true;
            processEdit.Start();

            string editOutput = processEdit.StandardError.ReadToEnd();
            processEdit.WaitForExit();

            Console.Clear();
            Console.WriteLine($"Аудио сохранено: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
    static void MadeKaraokeSound(string input)
    {
        string fileName = input.Substring("-karaoke ".Length).Trim().Trim('"');

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        string inputFile = Path.Combine(desktopPath, fileName);

        string extension = Path.GetExtension(fileName);

        string outputFileName = Path.GetFileNameWithoutExtension(fileName) + "_karaoke" + extension;
        string outputFile = Path.Combine(desktopPath, outputFileName);

        string arguments = $"-i \"{inputFile}\" -filter_complex " +
            "\"[0:a]asplit=2[low][high];" +
            "[low]lowpass=f=200:p=2[lowband];" +
            "[high]highpass=f=200:p=2,pan=stereo|c0=c0-c1|c1=c1-c0,volume=0.7[highband];" +
            "[lowband][highband]amix=inputs=2:normalize=0\" " +
            $"\"{outputFile}\"";

        Console.WriteLine("попытка создать караоке Версию песни " + fileName);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = new Process { StartInfo = psi })
        {
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine(e.Data);
            };

            process.Start();

            // Включаем асинхронное чтение
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }
    }
    static double ExtractValue(string output, string key)
    {
        foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Contains(key))
            {
                // Находим подстроку вида "mean_volume: -10.1 dB"
                var match = Regex.Match(line, key + @":\s*(-?\d+(\.\d+)?)\s*dB");
                if (match.Success)
                {
                    if (double.TryParse(match.Groups[1].Value,
                                        NumberStyles.Float,
                                        CultureInfo.InvariantCulture,
                                        out double val))
                    {
                        return val;
                    }
                }
            }
        }
        return 0.0;
    }
    static void PaintSpectrogram(string conversionName)
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var matches = Regex.Matches(conversionName, @"[\""].+?[\""]|[^ ]+");
            string[] args = matches.Cast<Match>().Select(m => m.Value.Trim('"')).ToArray();

            if (args.Length < 2)
            {
                Console.WriteLine("Укажите файл для анализа. Пример: -spectr \"audio.mp3\" HD");
                return;
            }

            string inputFile = Path.Combine(desktopPath, args[1]);
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Файл {inputFile} не найден на рабочем столе.");
            }

            string quality = args.Length > 2 ? args[2].ToUpper() : "SD";
            string resolution;

            if (quality == "4K")
                resolution = "3840x2160";
            else if (quality == "HD")
                resolution = "1920x1080";
            else if (quality == "SD")
                resolution = "1280x720";
            else
            {
                resolution = "1280x720";
                quality = "SD";
            }

            string baseFileName = "spectrogram.png";
            string outputFile = Path.Combine(desktopPath, baseFileName);

            int counter = 0;
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
            string extension = Path.GetExtension(baseFileName);

            while (File.Exists(outputFile))
            {
                counter++;
                string newFileName = $"{fileNameWithoutExt}{counter}{extension}";
                outputFile = Path.Combine(desktopPath, newFileName);
            }

            string ffmpegArgs = $"-i \"{inputFile}\" -lavfi showspectrumpic=s={resolution}:legend=1 \"{outputFile}\"";
            Console.WriteLine("FFmpeg рисует спектрограмму...");

            var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = ffmpegArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string output = process.StandardError.ReadToEnd();
            process.WaitForExit();

            Console.Clear();

            string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            string errorMessage = string.Join(Environment.NewLine, lines.Where(l => l.Contains("Error")));

            if (!string.IsNullOrEmpty(errorMessage))
                Console.WriteLine("Ошибка: " + errorMessage);
            else
                Console.WriteLine($"Спектрограмма ({quality}) сохранена: {outputFile}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static async Task CheckUpdate()
    {
        Version currentVersion = Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version;

        if (currentVersion == null)
        {
            Console.WriteLine("Не удалось определить версию программы");
            return;
        }

        string latestVersion = await Updater.GetLatestVersionAsync();

        if (latestVersion == null)
        {
            Console.WriteLine("Не удалось проверить обновления");
            return;
        }

        if (Version.Parse(latestVersion) > currentVersion)
        {
            Console.WriteLine($"Доступна новая версия! {currentVersion} ---> {latestVersion}");
            Console.WriteLine("Установить? y/Y");
            Console.Write(">>> ");

            var input = Console.ReadLine()?.Trim();
            if (input.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Начинаем установку...");
                Update();
            }
            else
            {
                Console.WriteLine("Обновление отменено.");
            }
        }

    }
    static void Update()
    {
        string updatePath = Updater.DownloadUpdateToTempAsync()
            .GetAwaiter()
            .GetResult();

        if (string.IsNullOrWhiteSpace(updatePath))
        {
            Console.WriteLine("Ошибка скачивания.");
            return;
        }

        Console.WriteLine("Обновление скачано:");
        Console.WriteLine(updatePath);

        string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FurryUpdater.exe");

        if (!File.Exists(updaterPath))
        {
            Console.WriteLine("Updater.exe не найден.");
            return;
        }

        Process.Start(updaterPath, $"\"{updatePath}\"");

        Environment.Exit(0);
    }

}