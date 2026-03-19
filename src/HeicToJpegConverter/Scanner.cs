using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeicToJpegConverter;

public static class Scanner
{
    public static (List<string> ToConvert, int SkippedCount, string OutputDir) Scan(string directoryPath)
    {
        var directoryInfo = new DirectoryInfo(directoryPath);
        var dirName = directoryInfo.Name;
        if (string.IsNullOrEmpty(dirName))
        {
            dirName = "root";
        }
        var outputDir = Path.Combine(directoryPath, $"{dirName}_output");
        
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        var toConvert = new List<string>();
        int skippedCount = 0;

        var allFiles = directoryInfo.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);
        foreach (var file in allFiles)
        {
            if (!file.Extension.Equals(".heic", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Skip hidden or system files
            if (file.Attributes.HasFlag(FileAttributes.Hidden) || file.Attributes.HasFlag(FileAttributes.System))
            {
                continue;
            }

            var outputFileName = Path.GetFileNameWithoutExtension(file.Name) + ".jpg";
            var outputFilePath = Path.Combine(outputDir, outputFileName);

            if (File.Exists(outputFilePath))
            {
                skippedCount++;
            }
            else
            {
                toConvert.Add(file.FullName);
            }
        }

        return (toConvert, skippedCount, outputDir);
    }
}
