using Elfo.FileImport.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;

namespace Elfo.FileImport
{
    public static class Utilities
    {
        public const string ErrorTableRowNumberColumnName = "Row Number";
        public const string ErrorTableErrorDescriptionColumnName = "Error Description";
        public const string ErrorTableInErrorColumnName = "In Error";

        public static readonly IEnumerable<string> DefaultColumnsList = new List<string> { ErrorTableRowNumberColumnName, ErrorTableErrorDescriptionColumnName, ErrorTableInErrorColumnName };

        public const string DefaultDateTimeFormat = "MM/dd/yyyy";

        public static ColumnConfigurationDTO FindConfigurationByColumnNum(IEnumerable<ColumnConfigurationDTO> columnConfigList, int columnNumber) =>
            columnConfigList.FirstOrDefault(column => column.ColumnNumber == columnNumber);

        public static bool CanParseValue(object value, CultureInfo culture, Type type)
        {
            bool canParse;
            string stringCellValue = value?.ToString();

            //se è nullo posso castarlo a tutto
            if (string.IsNullOrWhiteSpace(stringCellValue) || value.GetType() == type)
                return true;

            if (type == typeof(double))
            {
                double tryParse;
                canParse = double.TryParse(stringCellValue, NumberStyles.Float, culture, out tryParse);
            }
            else if (type == typeof(string))
            {
                canParse = true;
            }
            else if (type == typeof(DateTime))
            {
                DateTime tryParse;
                canParse = DateTime.TryParseExact(stringCellValue, DefaultDateTimeFormat, culture, DateTimeStyles.None, out tryParse);
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(type); //mi faccio dare il "tryparse" per il tipo passato
                return converter.IsValid(value); //controllo che il tipo passato sia un tipo vero in cui posso convertire i valori e controllo che il valore passato sia di quel tipo
            }

            return canParse;
        }

        internal static void ParseCellValue(DataRow dataRow, int columnIndex, ColumnConfigurationDTO columnConfig, object cellValue, IFormatProvider culture)
        {
            if (columnConfig.CustomValueReader == null)
            {
                string stringCellValue = cellValue?.ToString();

                if (!string.IsNullOrWhiteSpace(stringCellValue) && columnConfig.TypeToParse != null)
                {
                    if (columnConfig.TypeToParse == typeof(double))
                    {
                        if (cellValue.GetType() == columnConfig.TypeToParse)
                            stringCellValue = Convert.ToDouble(cellValue).ToString(culture);
                        else
                        {
                            double outDouble;
                            if (cellValue is string && double.TryParse(stringCellValue, NumberStyles.Float, culture, out outDouble))
                                stringCellValue = outDouble.ToString(culture);
                        }
                    }
                    else if (columnConfig.TypeToParse == typeof(DateTime))
                    {
                        if (cellValue.GetType() == columnConfig.TypeToParse)
                            stringCellValue = Convert.ToDateTime(cellValue).ToString(culture);
                        else
                        {
                            DateTime outDateTime;
                            if (cellValue is string && DateTime.TryParse(stringCellValue, culture, DateTimeStyles.None, out outDateTime))
                                stringCellValue = outDateTime.ToString(culture);
                        }
                    }
                }

                dataRow.SetColumnValue(columnIndex, stringCellValue);
            }
            else
            {
                string error = columnConfig.CustomValueReader(dataRow, cellValue, columnIndex);
                if (!string.IsNullOrWhiteSpace(error))
                    dataRow.SetColumnValue(ErrorTableRowNumberColumnName, error);
            }
        }
    }
}
