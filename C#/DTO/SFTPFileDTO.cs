using System;

namespace Elfo.FileImport.DTO
{
    public class SFTPFileDTO
    {
        public SFTPFileDTO(string fileName, string fileFullName, DateTime fileDate, byte[] file)
        {
            FileName = fileName;
            FileFullName = fileFullName;
            FileDate = fileDate;
            File = file;
        }

        public string FileName { get; }
        public string FileFullName { get; }
        public DateTime FileDate { get; }
        public byte[] File { get; }
    }
}
