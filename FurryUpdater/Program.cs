using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace FurryUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("ZIP путь не передан");
                Thread.Sleep(150);
                return;
            }

            string zipPath = args[0];

            if (!File.Exists(zipPath))
            {
                Console.WriteLine("ZIP не найден");
                return;
            }

            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            Console.WriteLine("Ожидание закрытия программы...");
            Thread.Sleep(150);

            Console.WriteLine("Распаковка прямо в корень...");

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.EndsWith("Updater.exe", StringComparison.OrdinalIgnoreCase))
                    continue;
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    string destinationPath = Path.Combine(appDir, entry.FullName);

                    // Создаём папку, если её нет
                    string directoryPath = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Перезаписываем файл
                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
            }

            Console.WriteLine("Распаковка завершена!");

            Console.WriteLine("Запуск новой версии...");

            string exePath = Directory.GetFiles(appDir, "*.exe")
                .FirstOrDefault(f => !f.Contains("Updater"));

            if (exePath != null)
            {
                Process.Start(exePath);
            }

            Console.WriteLine("Готово!");
        }
    }
}
