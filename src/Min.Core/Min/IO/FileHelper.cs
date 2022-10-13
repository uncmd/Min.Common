using Min.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Min.IO;

public static class FileHelper
{
    public static bool DeleteIfExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    public static string? GetExtension([NotNull] string fileNameWithExtension)
    {
        Check.NotNull(fileNameWithExtension);

        var lastDotIndex = fileNameWithExtension.LastIndexOf('.');
        if (lastDotIndex < 0)
        {
            return null;
        }

        return fileNameWithExtension.Substring(lastDotIndex + 1);
    }

    public static async Task<string> ReadAllTextAsync(string path)
    {
        using (var reader = File.OpenText(path))
        {
            return await reader.ReadToEndAsync();
        }
    }

    public static async Task<byte[]> ReadAllBytesAsync(string path)
    {
        using (var stream = File.Open(path, FileMode.Open))
        {
            var result = new byte[stream.Length];
            await stream.ReadAsync(result, 0, (int)stream.Length);
            return result;
        }
    }

    public static async Task<string[]> ReadAllLinesAsync(string path,
        Encoding? encoding = null,
        FileMode fileMode = FileMode.Open,
        FileAccess fileAccess = FileAccess.Read,
        FileShare fileShare = FileShare.Read,
        int bufferSize = 4096,
        FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan)
    {
        if (encoding == null)
        {
            encoding = Encoding.UTF8;
        }

        var lines = new List<string>();

        using (var stream = new FileStream(
            path,
            fileMode,
            fileAccess,
            fileShare,
            bufferSize,
            fileOptions))
        {
            using (var reader = new StreamReader(stream, encoding))
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lines.Add(line);
                }
            }
        }

        return lines.ToArray();
    }

    public static async Task<string?> ReadFileWithoutBomAsync(string path)
    {
        var content = await ReadAllBytesAsync(path);

        return StringHelper.ConvertFromBytesWithoutBom(content);
    }
}
