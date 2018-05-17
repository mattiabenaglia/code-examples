using Elfo.FileImport.DTO;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.IO;

namespace Elfo.FileImport
{
    public static class CsvUtilities
    {
        /// <summary>
        /// Return the Data Table filled with file data.
        /// If the template is different from the columnList input parameter throws a FileStructureIncorrectException 
        /// </summary>
        /// <param name="file">the file that will be processed</param>
        /// <param name="csvDelimiter">the csv delimiter</param>
        /// <param name="rowConfig">the row configuration</param>
        /// <param name="startRow">the row index for the header row</param>
        /// <returns></returns>
        public static DataTable GetDataTableFromCsvFile(Stream file, char csvDelimiter, RowConfigurationDTO rowConfig, int startRow)
        {
            DataTable dataTable;

            file.Position = 0;

            using (TextFieldParser csvReader = new TextFieldParser(file, System.Text.Encoding.UTF8))
            {
                csvReader.SetDelimiters(csvDelimiter.ToString());
                csvReader.HasFieldsEnclosedInQuotes = true;
                csvReader.TrimWhiteSpace = true;

                dataTable = csvReader.GetDataTable(rowConfig, startRow);
            }

            return dataTable;
        }
    }
}
