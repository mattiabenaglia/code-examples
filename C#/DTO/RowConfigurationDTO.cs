using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;

namespace Elfo.FileImport.DTO
{
    public class RowConfigurationDTO
    {
        /// <summary>
        /// A list of column configurations
        /// </summary>
        public ReadOnlyCollection<ColumnConfigurationDTO> ColumnConfigurations { get; set; }

        /// <summary>
        /// A func executed after parsing all columns. Arguments:
        /// 1) The Row with the parsed values
        /// 2) The errors found so far
        /// 
        /// The function should return an error description, or an empty string in case no error is found.
        /// </summary>
        public Func<DataRow, string, string> RowValidations { get; set; }

        /// <summary>
        /// The CultureInfo to use when parsing to the TypeToParse type.
        /// If the CultureInfo on a column is null the one specified for the row will be used, if that one is null too InvariantCulture will be used.
        /// </summary>
        public CultureInfo Culture { get; set; }
    }
}
