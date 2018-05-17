using System.Data;

namespace Elfo.FileImport
{
    public class DataTableAndErrorsDTO
    {
        public DataTable FilledDataTable { get; set; }
        public DataTable ErrorsDataTable { get; set; }
    }
}
