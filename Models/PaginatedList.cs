namespace BugscapeMVC.Models
{
    public class PaginatedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalItems { get; private set; }
        public int TotalPages { get; private set; }
        public int FirstPageButton { get; private set; }
        public int LastPageButton { get; private set; }

        public PaginatedList(List<T> items, int currentPage, int resultsPerPage)
        {
            int totalItems = items.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)resultsPerPage);

            int startPage = currentPage - 2;
            int endPage = currentPage + 2;

            if (endPage > totalPages)
            {
                endPage = totalPages;
                startPage = Math.Max(endPage - 4, 1);    
            }
            else if (startPage < 1)
            {
                startPage = 1;
                endPage = Math.Min(5, totalPages);
            }

            items = items.Skip((currentPage - 1) * resultsPerPage).Take(resultsPerPage).ToList();

            AddRange(items);

            CurrentPage = currentPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
            FirstPageButton = startPage;
            LastPageButton = endPage;
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}