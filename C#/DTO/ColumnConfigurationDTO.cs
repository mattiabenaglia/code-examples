using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace Elfo.FileImport.DTO
{
    public class ColumnConfigurationDTO
    {
        /// <summary>
        /// A list of all the possible headers for this column
        /// </summary>
        public List<string> ColumnNamesList { get; set; }

        /// <summary>
        /// Is this column mandatory?
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Index of this column
        /// </summary>
        public int ColumnNumber { get; set; }

        /// <summary>
        /// The type of this column
        /// </summary>
        public Type ColumnDataType { get; set; }

        /// <summary>
        /// The max length of this column (if it's a string, null otherwise)
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Boolean indicating if the validations should check for this column's name to be correct
        /// </summary>
        public bool ShouldCheckColumnName { get; set; }

        /// <summary>
        /// Action that defines a custom parsing behaviour specific to this column
        /// The parameters are:
        /// 1) The data row
        /// 2) The value for this column
        /// 3) The index of the column being parsed
        /// 
        /// The function should return an error description, or an empty string in case no error is found.
        /// </summary>
        public Func<DataRow, object, int, string> CustomValueReader { get; set; }

        /// <summary>
        /// Data validations for this column
        /// The parameters are:
        /// 1) The column configuration
        /// 2) The value to validate
        /// 3) The type to try to parse the value to
        /// 
        /// The function should return an error description, or an empty string in case no error is found.
        /// </summary>
        public Func<ColumnConfigurationDTO, object, Type, string> ColumnValidations { get; set; }

        /// <summary>
        /// Action defining a custom structure check behaviour for this column
        /// </summary>
        public Action<DataTable> CustomStructureChecks { get; set; }

        /// <summary>
        /// The type to try to parse the value to
        /// </summary>
        public Type TypeToParse { get; set; }

        /// <summary>
        /// The CultureInfo to use when parsing to the TypeToParse type.
        /// If the CultureInfo on a column is null the one specified for the row will be used, if that one is null too InvariantCulture will be used.
        /// </summary>
        public CultureInfo Culture { get; set; }
    }
}
