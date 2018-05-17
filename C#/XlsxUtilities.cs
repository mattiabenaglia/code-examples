using Elfo.FileImport.DTO;
using Elfo.FileImport.Exceptions;
using OfficeOpenXml;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Elfo.FileImport.XLSX
{
    public static class XlsxUtilities
    {
        /// <summary>
        /// Returns the specified Worksheet as a datatable
        /// If the Worhsheet passed is an empty string returns the first one.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rowConfig"></param>
        /// <param name="worksheetName"></param>
        /// <param name="startRow"></param>
        /// <returns></returns>
        public static DataTable GetDataTableFromExcelFile(Stream file, RowConfigurationDTO rowConfig, string worksheetName, int startRow)
        {
            using (var package = new ExcelPackage(file))
            {
                if (!string.IsNullOrEmpty(worksheetName) && !package.Workbook.Worksheets.Any(x => string.Equals(x.Name, worksheetName, StringComparison.CurrentCultureIgnoreCase)))
                    throw new FileStructureIncorrectException($"Unable to find a worksheet named : {worksheetName}");

                using (var worksheet = string.IsNullOrEmpty(worksheetName) ? package.Workbook.Worksheets[1] : package.Workbook.Worksheets[worksheetName])
                {
                    var dataTableStructure = GetDataTableStructure(worksheet, rowConfig, startRow);

                    var dataTable = FillDataTable(worksheet, rowConfig, dataTableStructure, startRow);

                    return dataTable;
                }
            }
        }

        /// <summary>
        /// Returns the first Worksheet as a datatable
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rowConfig"></param>
        /// <param name="startRow"></param>
        /// <returns></returns>
        public static DataTable GetDataTableFromExcelFile(Stream file, RowConfigurationDTO rowConfig, int startRow) =>
            GetDataTableFromExcelFile(file, rowConfig, string.Empty, startRow);

        /// <summary>
        /// Fills the DataTable parsing the Worksheet according to the condiguration specified in RowConfig - including the customizable validations and structure checks
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="rowConfig"></param>
        /// <param name="dataTableStructure"></param>
        /// <param name="startRow"></param>
        /// <returns></returns>
        private static DataTable FillDataTable(ExcelWorksheet worksheet, RowConfigurationDTO rowConfig, DataTable dataTableStructure, int startRow)
        {
            DataTable dataTableToReturn = dataTableStructure.Clone();
            var columnList = rowConfig.ColumnConfigurations;

            for (int rowIndex = startRow + 1;
                      rowIndex <= worksheet.Dimension.End.Row;
                      rowIndex++)
            {
                var dataRow = dataTableToReturn.NewRow();
                dataRow[Utilities.ErrorTableRowNumberColumnName] = rowIndex;
                for (int columnIndex = worksheet.Dimension.Start.Column;
                    columnIndex <= worksheet.Dimension.End.Column;
                    columnIndex++)
                {
                    var columnConfig = Utilities.FindConfigurationByColumnNum(columnList, (columnIndex - 1));

                    var culture = columnConfig.Culture ?? rowConfig.Culture ?? CultureInfo.InvariantCulture;
                    var cell = worksheet.Cells[rowIndex, columnIndex];
                    var cellValue = cell.Value;
                    var columnIndex_zeroBased = columnIndex - 1;

                    if (columnConfig.ColumnValidations != null)
                    {
                        string columnValidationError = columnConfig.ColumnValidations(columnConfig, cellValue, columnConfig.TypeToParse);
                        if (!string.IsNullOrWhiteSpace(columnValidationError))
                        {
                            string currentError = dataRow[Utilities.ErrorTableErrorDescriptionColumnName].ToString();
                            dataRow.SetColumnValue(Utilities.ErrorTableErrorDescriptionColumnName, string.Concat(currentError, columnValidationError));

                            // Se nella colonna posso inserire il valore che mi ha mandato in errore la validazione lo inserisco, altrimenti no
                            if (Utilities.CanParseValue(cellValue, culture, dataRow.Table.Columns[columnIndex_zeroBased].DataType))
                                Utilities.ParseCellValue(dataRow, columnIndex_zeroBased, columnConfig, cellValue, culture);
                        }
                        else
                            Utilities.ParseCellValue(dataRow, columnIndex_zeroBased, columnConfig, cellValue, culture);
                    }
                    else
                        Utilities.ParseCellValue(dataRow, columnIndex_zeroBased, columnConfig, cellValue, culture);
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
                        if (!dataRow.IsEmpty())
                            dataTableToReturn.Rows.Add(dataRow);
                    }
                }
                else
                {
                    if (!dataRow.IsEmpty())
                        dataTableToReturn.Rows.Add(dataRow);
                }
            }
            return dataTableToReturn;
        }

        /// <summary>
        /// Returns a DataTable built according to the RowConfigurationDTO passed.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="rowConfig"></param>
        /// <param name="startRow"></param>
        /// <returns></returns>
        private static DataTable GetDataTableStructure(ExcelWorksheet worksheet, RowConfigurationDTO rowConfig, int startRow)
        {
            var dataTableStructure = new DataTable();
            var columnList = rowConfig.ColumnConfigurations;

            //If the number of the Excel column is different from the number of the column set in the column configuration list trow a FileStructureIncorrectException
            if (worksheet.Dimension.End.Column != columnList.Count)
            {
                throw new FileStructureIncorrectException("The number of the Excel column is different from the number of the column set in the column configuration list");
            }

            for (int columnIndex = worksheet.Dimension.Start.Column;
                    columnIndex <= worksheet.Dimension.End.Column;
                    columnIndex++)
            {
                var columnConfig = Utilities.FindConfigurationByColumnNum(columnList, columnIndex - 1);

                if (columnConfig.ShouldCheckColumnName && !columnConfig.ColumnNamesList.Any(x => x.IsColumnNameEqualTo(worksheet.Cells[startRow, columnIndex].Value.ToString())))
                    throw new FileStructureIncorrectException(
                        $"Unable to find a column with name equal to a column name in the column names list specified in the ColumnConfigurationDTO with ColumnNumber: {columnConfig.ColumnNumber}");

                if (columnConfig.CustomStructureChecks == null)
                {
                    var dataColumn = new DataColumn(columnConfig.ColumnNamesList.First(x => x.IsColumnNameEqualTo(worksheet.Cells[startRow, columnIndex].Value.ToString())), columnConfig.ColumnDataType);
                    dataTableStructure.Columns.Add(dataColumn);
                }
                else
                {
                    columnConfig.CustomStructureChecks(dataTableStructure);
                }
            }

            var rowNumberColumn = new DataColumn(Utilities.ErrorTableRowNumberColumnName, typeof(int));
            var errorDescriptionColumn = new DataColumn(Utilities.ErrorTableErrorDescriptionColumnName, typeof(string));
            var inErrorColumn = new DataColumn(Utilities.ErrorTableInErrorColumnName, typeof(bool));
            dataTableStructure.Columns.Add(rowNumberColumn);
            dataTableStructure.Columns.Add(errorDescriptionColumn);
            dataTableStructure.Columns.Add(inErrorColumn);

            return dataTableStructure;
        }
    }
}
