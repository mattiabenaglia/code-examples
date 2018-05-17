using System.Collections.Generic;

namespace Elfo.FileImport
{
    public static class StringExtension
    {
        private static List<string> charactersToRemoveFromColumnName = new List<string>() { "_", " ", "+", "/", "&","\n" };

        public static string GetColumnNameToCompare(this string thisColumnName)
        {
            string thisColumnNameClean = thisColumnName.ToLower().Trim();

            foreach (string toRemoveString in charactersToRemoveFromColumnName)
            {
                thisColumnNameClean = thisColumnNameClean.Replace(toRemoveString, "");
            }

            return thisColumnNameClean;
        }

        public static bool IsColumnNameEqualTo(this string thisColumnName, string columnNameToCompare)
        {
            string thisColumnNameClean = thisColumnName.GetColumnNameToCompare();
            string columnNameToCompareClean = columnNameToCompare.GetColumnNameToCompare();

            return thisColumnNameClean == columnNameToCompareClean;
        }
    }
}
