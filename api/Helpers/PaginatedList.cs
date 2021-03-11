using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Helpers
{
    public class PaginatedList<T>: List<T>
    {
        public PaginatedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count /(double) pageSize);
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);
        }
        /// <summary>
        /// The current page that the user is on
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// The total amount of pages available from the query
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// The amount of items per page
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// The total amount of items available from the query
        /// </summary>
        public int TotalCount { get; set; }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
