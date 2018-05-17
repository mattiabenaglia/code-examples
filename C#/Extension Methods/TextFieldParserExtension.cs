using Elfo.FileImport.DTO;
using Elfo.FileImport.Exceptions;
using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Elfo.FileImport
{
    public static class TextFieldParserExtension
    {
        private static void CheckColumnStructure(this string[] colFields, ReadOnlyCollection<ColumnConfigurationDTO> columnList, int columnIndex)
        {
            var columnConfig = Utilities.FindConfigurationByColumnNum(columnList, columnIndex);
            if (columnConfig == null)
                throw new FileStructureIncorrectException($"Unable to find a ColumnConfigurationDTO for index: {columnIndex}");

            string worksheetColumnName = colFields[columnIndex];

            if (!columnConfig.ColumnNamesList.Any(x => x.IsColumnNameEqualTo(worksheetColumnName)))
                throw new FileStructureIncorrectException($"Unable to find a ColumnConfigurationDTO named : {worksheetColumnName}");
        }

        private static DataTable GetDataTableStructure(this TextFieldParser csvReader, RowConfigurationDTO rowConfig)
        {
            var dataTable = new DataTable();
            var columnList = rowConfig.ColumnConfigurations;
            var colFields = csvReader.ReadFields();

            if (colFields.Length != columnList.Count)
                throw new FileStructureIncorrectException("Columns number of worksheet is different from number of ColumnConfigurations specified in the input parameter");

            for (var columnIndex = 0; columnIndex < colFields.Length; columnIndex++)
            {
                colFields.CheckColumnStructure(columnList, columnIndex);

                var columnConfig = Utilities.FindConfigurationByColumnNum(columnList, columnIndex);

                if (columnConfig.CustomStructureChecks == null)
                {
                    var column = new DataColumn(colFields[columnIndex], columnConfig.ColumnDataType);

                    dataTable.Columns.Add(column);
                }
                else
                {
                    columnConfig.CustomStructureChecks(dataTable);
                }
            }

            var rowNumberColumn = new DataColumn(Utilities.ErrorTableRowNumberColumnName, typeof(int));
            var errorDescriptionColumn = new DataColumn(Utilities.ErrorTableErrorDescriptionColumnName, typeof(string));
            var inErrorColumn = new DataColumn(Utilities.ErrorTableInErrorColumnName, typeof(bool));
            dataTable.Columns.Add(rowNumberColumn);
            dataTable.Columns.Add(errorDescriptionColumn);
            dataTable.Columns.Add(inErrorColumn);

            return dataTable;
        }

        /// <summary>
        /// Manage the start of the file 
        /// </summary>
        /// <param name="csvReader"></param>
        /// <param name="startRow"></param>
        /// <returns></returns>
        private static int SkipRows(this TextFieldParser csvReader, int startRow)
        {
            var rowIndex = 1;
            while (rowIndex < startRow)
            {
                csvReader.ReadLine();
                rowIndex++;
            }

            return rowIndex;
        }

        private static DataRow GetDataRow(this TextFieldParser csvReader, int rowIndex, RowConfigurationDTO rowConfig, DataTable dataTableStructure)
        {
            var dataTableFilledCloned = dataTableStructure.Clone();

            var dataRow = dataTableFilledCloned.NewRow();
            dataRow[Utilities.ErrorTableRowNumberColumnName] = rowIndex;
            var colFields = csvReader.ReadFields();

            var columnList = rowConfig.ColumnConfigurations;

            for (var columnIndex = 0; columnIndex < colFields.Length; columnIndex++)
            {
                var columnConfig = Utilities.FindConfigurationByColumnNum(columnList, columnIndex);
                var culture = columnConfig.Culture ?? rowConfig.Culture ?? CultureInfo.InvariantCulture;

                string valueFromCsv = colFields[columnIndex];

                if (columnConfig.ColumnValidations != null)
                {
                    string columnValidationError = columnConfig.ColumnValidations(columnConfig, valueFromCsv, columnConfig.TypeToParse);
                    if (!string.IsNullOrWhiteSpace(columnValidationError))
                    {
                        string currentError = dataRow[Utilities.ErrorTableErrorDescriptionColumnName].ToString();
                        dataRow.SetColumnValue(Utilities.ErrorTableErrorDescriptionColumnName, string.Concat(currentError, columnValidationError));
                        // Se nella colonna posso inserire il valore che mi ha mandato in errore la validazione lo inserisco, altrimenti no
                        if (Utilities.CanParseValue(valueFromCsv, culture, dataRow.Table.Columns[columnIndex - 1].DataType))
                            Utilities.ParseCellValue(dataRow, columnIndex, columnConfig, valueFromCsv, culture);
                    }
                    else
                        Utilities.ParseCellValue(dataRow, columnIndex, columnConfig, valueFromCsv, culture);
                }
                else
                    Utilities.ParseCellValue(dataRow, columnIndex, columnConfig, valueFromCsv, culture);
            }

            dataRow[Utilities.ErrorTableInErrorColumnName] = !string.IsNullOrWhiteSpace(dataRow[Utilities.ErrorTableErrorDescriptionColumnName].ToString());

            if (rowConfig.RowValidations != null)
            {
                string rowError = rowConfig.RowValidations(dataRow, dataRow[Utilities.ErrorTableErrorDescriptionColumnName].ToString());
                if (!string.IsNullOrWhiteSpace(rowError))
                {
                    string currentError = dataRow[Utilities.ErrorTableErrorDescriptionColumnName].ToString();
                    dataRow.SetColumnValue(Utilities.ErrorTableErrorDescriptionColumnName, string.Concat(currentError, rowError));
                }
                else
                {
                    dataTableFilledCloned.Rows.Add(dataRow);
                }
            }
            else
            {
                dataTableFilledCloned.Rows.Add(dataRow);
            }

            return dataRow;
        }

        private static DataTable FillDataTable(this TextFieldParser csvReader, int rowIndex, DataTable dataTableStructure, RowConfigurationDTO rowConfig)
        {
            var dataTableFilled = dataTableStructure.Clone();

            rowIndex++;
            while (!csvReader.EndOfData)
            {
                var dataRow = csvReader.GetDataRow(rowIndex, rowConfig, dataTableStructure);

                if (!dataRow.IsEmpty())
                    dataTableFilled.ImportRow(dataRow);
                rowIndex++;
            }
            return dataTableFilled;
        }

        public static DataTable GetDataTable(this TextFieldParser csvReader, RowConfigurationDTO rowConfig, int startRow) =>
            csvReader.FillDataTable(csvReader.SkipRows(startRow), csvReader.GetDataTableStructure(rowConfig), rowConfig);
    }
}
