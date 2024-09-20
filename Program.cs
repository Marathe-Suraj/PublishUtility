using Microsoft.Extensions.Configuration;
using System.IO.Compression;

namespace FileZipperUtility
{
    class Program
    {
        static string folderPath = "D:\\publish\\Product QA";
        static void Main(string[] args)
        {
            // Step 1: Load paths (files and folders) from appsettings.json
            Console.WriteLine("Getting files...");
            var paths = LoadPaths();
            Console.WriteLine("Files paths loaded");

            // Step 2: Create a temporary folder to store all copied files
            string tempFolder = Path.Combine(folderPath, "ZippedFiles_" + DateTime.Now.Ticks);
            Directory.CreateDirectory(tempFolder);

            Console.WriteLine("Files copy started...");
            // Step 3: Copy files and folders to the temporary folder
            foreach (var path in paths)
            {
                if (File.Exists(path)) // If it's a file
                {
                    CopyFileToTempFolder(path, tempFolder);
                }
                else if (Directory.Exists(path)) // If it's a directory
                {
                    CopyDirectoryToTempFolder(path, tempFolder);
                }
                else
                {
                    Console.WriteLine($"Path not found: {path}");
                }
            }
            Console.WriteLine("Files copy completed...");

            // Step 4: Create a zip file
            Console.WriteLine("Creating package...");
            string zipFilePath = Path.Combine(folderPath, "CalmanacFullBuild.zip");
            ZipFile.CreateFromDirectory(tempFolder, zipFilePath);

            // Clean up temporary folder
            Directory.Delete(tempFolder, true);

            Console.WriteLine($"Package successfully created! Package located at: {zipFilePath}");
        }

        static List<string> LoadPaths()
        {
            // Load appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfiguration config = builder.Build();

            // Get paths (both files and folders) from appsettings.json
            var paths = config.GetSection("FilePaths").Get<List<string>>();
            return paths;
        }

        static void CopyFileToTempFolder(string filePath, string tempFolder)
        {
            var fileName = Path.GetFileName(filePath);
            var destinationPath = Path.Combine(tempFolder, fileName);
            File.Copy(filePath, destinationPath);
        }

        static void CopyDirectoryToTempFolder(string sourceDir, string tempFolder)
        {
            // Get the name of the directory
            var dirName = Path.GetFileName(sourceDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            // Create a directory inside the temp folder with the same name
            var destDir = Path.Combine(tempFolder, dirName);
            Directory.CreateDirectory(destDir);

            // Recursively copy all files and subdirectories
            foreach (var filePath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, filePath); // Preserve folder structure
                var destFilePath = Path.Combine(destDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFilePath)); // Ensure subdirectories exist
                File.Copy(filePath, destFilePath);
            }
        }
    }
}
