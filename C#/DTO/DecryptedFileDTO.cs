using System.IO;

namespace Elfo.FileImport.DTO
{
    public class DecryptedFileDTO
    {
        public DecryptedFileDTO(string fileName, Stream fileStream)
        {
            FileName = fileName;
            FileStream = fileStream;
        }

        public string FileName { get; }

        public Stream FileStream { get; }
    }
}