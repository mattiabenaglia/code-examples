using System;
using System.Data;
using System.Linq;

namespace Elfo.FileImport
{
    public static class DataRowExtension
    {
        public static bool IsEmpty(this DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                if (!Utilities.DefaultColumnsList.Contains(column.ColumnName) && !string.IsNullOrWhiteSpace(row[column.ColumnName].ToString()))
                    return false;
            }

            return true;
        }

        public static void SetColumnValue(this DataRow row, int columnIndex, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                row[columnIndex] = DBNull.Value;
            else
                row[columnIndex] = value;
        }

        public static void SetColumnValue(this DataRow row, string columnName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                row[columnName] = DBNull.Value;
            else
                row[columnName] = value;
        }
    }
}
