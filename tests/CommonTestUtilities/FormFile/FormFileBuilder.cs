using Microsoft.AspNetCore.Http;

namespace CommonTestUtilities.FormFile;

public class FormFileBuilder
{
    public static IFormFile Build(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", $"{fileName}");
        var content = extension switch
        {
            ".jpg" => [0xFF, 0xD8, 0xFF], // JPEG header
            ".png" => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], // PNG header
            ".txt" => "This is a text file"u8.ToArray(),
            _ => throw new InvalidDataException("Unsupported file type")
        };

        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);
        File.WriteAllBytes(filePath, content);

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new Microsoft.AspNetCore.Http.FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = extension switch
            {
                ".jpg" => "image/jpeg",
                ".png" => "image/png",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            }
        };
    }
}