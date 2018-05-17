using Elfo.FileImport.DTO;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Elfo.FileImport
{
    /// <summary>
    /// SFTP Client used to download files from a SFTP server
    /// </summary>
    public class SFTPClient
    {
        #region Properties

        private readonly string FtpHost,
            FtpUserName,
            FtpPassword;
        private readonly int FtpPort;

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="ftpHost">SFTP server to connect to</param>
        /// <param name="ftpPort">Port to connect to</param>
        /// <param name="ftpUserName">Username to connect with</param>
        /// <param name="ftpPassword">Password to connect with</param>
        public SFTPClient(string ftpHost, int ftpPort, string ftpUserName, string ftpPassword)
        {
            FtpHost = ftpHost;
            FtpPort = ftpPort;
            FtpUserName = ftpUserName;
            FtpPassword = ftpPassword;
        }
        
        private SftpClient SftpClientConnection => new SftpClient(FtpHost, FtpPort, FtpUserName, FtpPassword);

        private void SftpClientAction(Action<SftpClient> action)
        {
            using (SftpClient sftp = SftpClientConnection)
            {
                sftp.Connect();

                action(sftp);

                sftp.Disconnect();
            }
        }

        /// <summary>
        /// Downloads all the files found in the specified directory, filtered by the regex argument
        /// and deletes them according to the deleteFileOnSFTP flag.
        /// </summary>
        /// <param name="ftpDirectory">Directory to downloads file from</param>
        /// <param name="regexesFileName">Regex to filter the file names</param>
        /// <returns></returns>
        public List<SFTPFileDTO> DownloadFiles(string ftpDirectory, List<Regex> regexesFileName)
        {
            List<SFTPFileDTO> sftpFileDTOList = new List<SFTPFileDTO>();

            SftpClientAction((sftp) => {

                foreach (var file in GetFiles(sftp, ftpDirectory, regexesFileName))
                {
                    using (MemoryStream fileStream = new MemoryStream())
                    {
                        sftp.DownloadFile(file.FullName, fileStream);
                        fileStream.Position = 0;

                        sftpFileDTOList.Add(new SFTPFileDTO(
                            fileName: file.Name,
                            fileFullName: file.FullName,
                            fileDate: file.LastWriteTime,
                            file: fileStream.ToArray()
                        ));
                    }
                }

            });
            
            return sftpFileDTOList;
        }

        /// <summary>
        /// Delete the files from the SFTP server
        /// </summary>
        public void DeleteFiles(List<string> fileFullNamesList)
        {
            SftpClientAction((sftp) =>
            {
                foreach (var fileFullName in fileFullNamesList)
                {
                    sftp.DeleteFile(fileFullName);
                }
            });
        }

        private List<SftpFile> GetFiles(SftpClient sftp, string ftpDirectory, List<Regex> regexesFileName)
        {
            //Get a list of files found on the server in the specified directory
            IEnumerable<SftpFile> filesList = sftp.ListDirectory(ftpDirectory);

            //Filter the list of found files with the provided regexes
            return filesList.Where(f => regexesFileName.Any(x => x.IsMatch(f.Name))).ToList();
        }
    }
}
