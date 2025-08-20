using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;

namespace HealthFit.Models.Models
{
    public class Pager
    {
        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }

        public Pager() { }

        public Pager(int totalItems, int currentPage = 1, int pageSize = 10)
        {
            TotalItems = totalItems;
            PageSize = pageSize;

            TotalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            CurrentPage = currentPage;

            int startPage = currentPage - 5;
            int endPage = currentPage + 4;

            if (startPage <= 0)
            {
                endPage = endPage - (startPage - 1);
                startPage = 1;
            }

            if (endPage > TotalPages)
            {
                endPage = TotalPages;
                if (endPage > 10)
                {
                    startPage = endPage - 9;
                }
            }

            StartPage = startPage;
            EndPage = endPage;
        }
    }

}
