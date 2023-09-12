namespace BugscapeMVC.Models
{
    public class Pagination
    {
        public Pagination() {}

        public Pagination(int totalItems, int currentPage, int resultsPerPage = 10)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)resultsPerPage);
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

            if (currentPage != 1)
            {
                PreviousPage = currentPage - 1;
            }

            if (currentPage != totalPages)
            {
                NextPage = currentPage + 1;
            }

            TotalItems = totalItems;
            TotalPages = totalPages;
            CurrentPage = currentPage;
            ResultsPerPage = resultsPerPage;
            StartPage = startPage;
            EndPage = endPage;
        }

        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int ResultsPerPage { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public int NextPage { get; set; }
        public int PreviousPage { get; set; }
    }
}