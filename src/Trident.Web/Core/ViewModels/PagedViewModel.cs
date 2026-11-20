using System.Collections.Generic;

namespace Trident.Web.Core.ViewModels
{
    public class PagedViewModel<T> where T : class
    {
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int RowsPerPage { get; set; }
        public int CurrentPageNumber { get; set; }
        public int PreviousPage => CurrentPageNumber - 1;
        public int NextPage => CurrentPageNumber + 1;
        public bool HasPreviousPage => CurrentPageNumber > 1;
        public bool HasNextPage => CurrentPageNumber < TotalPages;

        public IList<T> Items { get; set; }
    }
}
