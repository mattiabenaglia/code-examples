using Elfo.FileImport.DTO;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.IO;

namespace Elfo.FileImport
{
    /// <summary>
    /// Utility class to encrypt/decrypt files using PGP encryption
    /// </summary>
    public static class PGPHelper
    {
        #region Key

        #region Public

        public static PgpPublicKey GetPublicKey(Stream publicKeyStream)
        {
            PgpPublicKeyRingBundle pgpPub = GetPgpPublicKeyRingBundle(publicKeyStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        return key;
                    }
                }
            }

            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        private static PgpPublicKeyRingBundle GetPgpPublicKeyRingBundle(Stream publicKeyStream)
        {
            return new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(publicKeyStream));
        }

        #endregion

        #region Private

        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);

            return pgpSecKey != null ? pgpSecKey.ExtractPrivateKey(pass) : null;
        }

        public static PgpSecretKeyRingBundle GetPgpSecretKeyRingBundle(Stream privateKeyFileStream)
        {
            return new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyFileStream));
        }

        #endregion

        #endregion

        #region Encrypt

        ///// <summary>
        ///// Encrypt an input file
        ///// </summary>
        ///// <param name="inputFilePath">Path of the file to be encrypted</param>
        ///// <param name="outputFilePath">The path where the encrypted file will be saved</param>
        ///// <param name="publicKeyFilePath">The path of the public key</param>
        ///// <param name="armor">ASCII armor encryption</param>
        ///// <param name="withIntegrityCheck">Determine whether or not the resulting encrypted data will be protected using an integrity packet</param>
        //public static void EncryptPgpFile(string inputFilePath, string outputFilePath, string publicKeyFilePath, bool armor, bool withIntegrityCheck)
        //{
        //PgpPublicKey pubKey = GetPublicKey(publicKeyFilePath);

        //using (MemoryStream outputBytes = new MemoryStream())
        //{
        //PgpCompressedDataGenerator dataCompressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);

        //PgpUtilities.WriteFileToLiteralData(dataCompressor.Open(outputBytes), PgpLiteralData.Binary, new FileInfo(inputFilePath));
        //dataCompressor.Close();

        //PgpEncryptedDataGenerator dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Cast5, withIntegrityCheck, new SecureRandom());
        //dataGenerator.AddMethod(pubKey);
        //byte[] dataBytes = outputBytes.ToArray();

        //using (Stream outputStream = File.Create(outputFilePath))
        //{
        //    if (armor)
        //    {
        //        using (ArmoredOutputStream armoredStream = new ArmoredOutputStream(outputStream))
        //        {
        //            IoHelper.WriteStream(dataGenerator.Open(armoredStream, dataBytes.Length), ref dataBytes);
        //        }
        //    }
        //    else
        //    {
        //        IoHelper.WriteStream(dataGenerator.Open(outputStream, dataBytes.Length), ref dataBytes);
        //    }
        //}

        //dataGenerator.Close();
        //}
        //}

        #endregion

        #region Decrypt

        private static PgpEncryptedDataList GetPgpEncryptedDataList(Stream inputFileStream)
        {
            PgpObjectFactory pgpFactory = GetPgpObjectFactory(inputFileStream);
            PgpObject firstPgpObject = pgpFactory.NextPgpObject();

            // the first object might be a PGP marker packet.
            if (firstPgpObject is PgpEncryptedDataList)
                return (PgpEncryptedDataList)firstPgpObject;
            else
                return (PgpEncryptedDataList)pgpFactory.NextPgpObject();
        }

        private static PgpObjectFactory GetPgpObjectFactory(PgpEncryptedDataList filePgpEncryptedDataList, PgpSecretKeyRingBundle pgpSecretKeyRingBundle, string passPhrase)
        {
            PgpPrivateKey privateKey = null;
            PgpPublicKeyEncryptedData publicKeyEncryptedData = null;

            foreach (PgpPublicKeyEncryptedData pked in filePgpEncryptedDataList.GetEncryptedDataObjects())
            {
                privateKey = FindSecretKey(pgpSecretKeyRingBundle, pked.KeyId, passPhrase.ToCharArray());

                if (privateKey != null)
                {
                    publicKeyEncryptedData = pked;
                    break;
                }
            }

            if (privateKey == null)
                throw new ArgumentException("Secret key for message not found.");

            return GetPgpObjectFactory(publicKeyEncryptedData, privateKey);
        }

        private static PgpObjectFactory GetPgpObjectFactory(PgpObject message)
        {
            PgpCompressedData compressedData = (PgpCompressedData)message;
            PgpObjectFactory pgpObjectFactory;

            using (Stream compDataIn = compressedData.GetDataStream())
            {
                pgpObjectFactory = new PgpObjectFactory(compDataIn);
            }

            return pgpObjectFactory;
        }
        
        private static PgpObjectFactory GetPgpObjectFactory(Stream inputFileStream)
        {
            //Lo stream viene passato per riferimento, quindi non deve essere disposto in questo metodo
            return new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputFileStream));
        }

        private static PgpObjectFactory GetPgpObjectFactory(PgpPublicKeyEncryptedData publicKeyEncryptedData, PgpPrivateKey privateKey)
        {
            //Lo stream viene passato per riferimento, quindi non deve essere disposto in questo metodo
            return new PgpObjectFactory(publicKeyEncryptedData.GetDataStream(privateKey));
        }

        private static DecryptedFileDTO CreateDecryptedFileDTO(PgpObject message)
        {
            PgpLiteralData ld = (PgpLiteralData)message;

            return new DecryptedFileDTO(
                fileName: ld.FileName,
                fileStream: ld.GetInputStream());
        }
        
        public static DecryptedFileDTO Decrypt(Stream encryptedFileStream, PgpSecretKeyRingBundle pgpSecretKeyRingBundle, string passPhrase)
        {
            PgpEncryptedDataList filePgpEncryptedDataList = GetPgpEncryptedDataList(encryptedFileStream);

            PgpObjectFactory pgpObjectFactory = GetPgpObjectFactory(filePgpEncryptedDataList, pgpSecretKeyRingBundle, passPhrase);

            PgpObject message = pgpObjectFactory.NextPgpObject();

            //verifico se il file è compresso o literal
            if (message is PgpCompressedData)
            {
                PgpObjectFactory pgpCompressedFactory = GetPgpObjectFactory(message);
                message = pgpCompressedFactory.NextPgpObject();

                if (message is PgpOnePassSignatureList)
                {
                    message = pgpCompressedFactory.NextPgpObject();
                    return CreateDecryptedFileDTO(message);
                }
                else
                {
                    return CreateDecryptedFileDTO(message);
                }
            }
            else if (message is PgpLiteralData)
            {
                return CreateDecryptedFileDTO(message);
            }
            else if (message is PgpOnePassSignatureList)
            {
                throw new PgpException("Encrypted message contains a signed message - not literal data.");
            }
            else
            {
                throw new PgpException("Message is not a simple encrypted file - type unknown.");
            }
        }

        #endregion
    }
}
