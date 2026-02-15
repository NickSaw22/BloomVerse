using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PaginatedResult<T>
    {
        public PaginatedMetadata Metadata { get; set; } = default!;
        public List<T> Items { get; set; } = [];

        public class PaginatedMetadata
        {
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
        }

        public class PaginationHelper
        {
            public static async Task<PaginatedResult<T>> CreateAsync(IQueryable<T> query, int pageNumber, int pageSize)
            {
                var count = await query.CountAsync();
                var items = await query.Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync();

                return new PaginatedResult<T>
                {
                    Metadata = new PaginatedMetadata
                    {
                        CurrentPage = pageNumber,
                        TotalPages = (int)Math.Ceiling(count/(double)pageSize),
                        PageSize = pageSize,
                        TotalCount = count
                    },
                    Items = items
                };
            }
        }
    }
}