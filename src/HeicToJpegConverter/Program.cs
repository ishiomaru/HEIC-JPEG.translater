using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.CLSCTX;
using static TerraFX.Interop.Windows.COINIT;

namespace HeicToJpegConverter;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("HEIC to JPEG Converter");
        string currentDir = Environment.CurrentDirectory;
        Console.WriteLine($"Scanning directory: {currentDir}");

        var (toConvert, skippedCount, outputDir) = Scanner.Scan(currentDir);
        
        Console.WriteLine($"Found {toConvert.Count} HEIC files to convert.");
        if (skippedCount > 0)
        {
            Console.WriteLine($"Skipping {skippedCount} files (already exist).");
        }

        if (toConvert.Count == 0)
        {
            Console.WriteLine("Done.");
            Console.ReadLine();
            return;
        }

        int successCount = 0;
        int errorCount = 0;
        int processedCount = 0;
        int totalToConvert = toConvert.Count;

        object lockObj = new object();

        // Native AOT doesn't support COM Apartments as nicely implicitly, so initialize per thread
        await Parallel.ForEachAsync(toConvert, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (filePath, _) =>
        {
            unsafe
            {
            int hrInit = CoInitializeEx(null, (uint)COINIT_MULTITHREADED);
            bool isComInitialized = (hrInit >= 0 || (uint)hrInit == 0x80010106); // S_OK or RPC_E_CHANGED_MODE

            IWICImagingFactory* factory = null;
            Guid wicFactoryClsid = new Guid("cacaf262-9370-4615-a13b-9f5539da4c0a");
            Guid iidFactory = typeof(IWICImagingFactory).GUID;

            int hr = CoCreateInstance(
                &wicFactoryClsid,
                null,
                (uint)CLSCTX_INPROC_SERVER,
                &iidFactory,
                (void**)&factory);

            if (hr >= 0 && factory != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath) + ".jpg";
                string targetPath = Path.Combine(outputDir, fileName);

                bool success = Converter.ConvertHeicToJpeg(filePath, targetPath, factory);

                factory->Release();

                lock (lockObj)
                {
                    if (success) successCount++;
                    else errorCount++;
                    processedCount++;
                    Console.Write($"\rProgress: {processedCount}/{totalToConvert} (Success: {successCount}, Error: {errorCount})");
                }
            }
            else
            {
                lock (lockObj)
                {
                    errorCount++;
                    processedCount++;
                    Console.Write($"\rProgress: {processedCount}/{totalToConvert} (Success: {successCount}, Error: {errorCount})");
                }
            }

            if (isComInitialized && (uint)hrInit != 0x80010106)
            {
                CoUninitialize();
            }
            }
            return ValueTask.CompletedTask;
        });

        Console.WriteLine("\n\nConversion Finished!");
        Console.WriteLine($"Total Success: {successCount}");
        Console.WriteLine($"Total Errors: {errorCount}");
        Console.WriteLine($"Total Skipped: {skippedCount}");
        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }
}
