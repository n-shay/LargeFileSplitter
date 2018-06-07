namespace LargeFileSplitter
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

	/// <summary>
	/// Usages:
	/// <para>
	/// Splits the file into pre-defined size files (the reminder is placed in an additional last file):
	/// <example>LargeFileSplitter.exe "filename.txt" -size <![CDATA[<size in bytes>]]></example>
	/// </para>
	/// <para>
	/// Splits the file into equally sized files:
	/// <example>LargeFileSplitter.exe "filename.txt" -count <![CDATA[<number of files>]]></example>
	/// </para>
	/// </summary>
	internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(AppDomain.CurrentDomain.FriendlyName);
            
            if (args.Length == 3)
            {
                var filepath = args[0].Trim('"');

	            if (!long.TryParse(args[2], out var total) || total <= 0)
                {
                    Console.WriteLine($"ERROR: Argument '{args[2]}' is not a valid Int64.");
                    return;
                }

                switch (args[1].ToLower())
                {
                    case "-size":
                        if (SplitBySize(filepath, total).Result)
                            return;
                        break;
                    case "-count":
                        if (SplitByCount(filepath, (int)total).Result)
                            return;
                        break;
                }
            }

            DisplayHelp();
        }

        private static async Task<bool> SplitByCount(string filepath, int count)
        {
            try
            {
                var fi = new FileInfo(filepath);

                var largeFileSize = fi.Length;

                var fileSize = largeFileSize/count;
                var lastFileSize = fileSize + largeFileSize%count;

                using (var oldStream = fi.OpenRead())
                {
                    for (var i = 1; i <= count; i++)
                    {
                        var size = i == count ? lastFileSize : fileSize;
	                    var newFilePath =
		                    $"{fi.DirectoryName}\\{Path.GetFileNameWithoutExtension(filepath)}.{i}{fi.Extension}";

                        using (var newStream = File.Create(newFilePath))
                        {
                            var buffer = new byte[size];
                            var readCount = await oldStream.ReadAsync(buffer, 0, (int) size);

                            await newStream.WriteAsync(buffer, 0, readCount);
                            await newStream.FlushAsync();
                        }

                        Console.WriteLine($"{i}: {newFilePath} ({size} bytes)");
                    }
                }
                
                Console.WriteLine("Completed!");
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("ERROR: Access is denied.");
                return false;
            }
        }

        private static async Task<bool> SplitBySize(string filepath, long sizeBytes)
        {
            try
            {
                var fi = new FileInfo(filepath);

                using (var oldStream = fi.OpenRead())
                {
                    int readCount;
                    var i = 1;
                    do
                    {
                        var buffer = new byte[sizeBytes];
                        var newFilePath = $"{fi.DirectoryName}\\{Path.GetFileNameWithoutExtension(filepath)}.{i}{fi.Extension}";

                        readCount = await oldStream.ReadAsync(buffer, 0, (int)sizeBytes);

                        if (readCount > 0)
                        {
                            using (var newStream = File.Create(newFilePath))
                            {

                                await newStream.WriteAsync(buffer, 0, readCount);
                                await newStream.FlushAsync();
                            }

                            Console.WriteLine($"{i}: {newFilePath} ({readCount} bytes)");
                        }

                        i++;
                    } while (readCount > 0);
                }

                Console.WriteLine("Completed!");
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("ERROR: Access is denied.");
                return false;
            }
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("Usages:");
            Console.WriteLine();
            Console.WriteLine("Option 1 - Splits the file into pre-defined size files (the reminder is placed in an additional last file):");
            Console.WriteLine("LargeFileSplitter.exe <file path> -size <size in bytes>");
            Console.WriteLine();
            Console.WriteLine("Option 2 - Splits the file into equally sized files:");
            Console.WriteLine("LargeFileSplitter.exe <file path> -count <number of files>");
            Console.WriteLine();
        }
    }
}
