using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactoryOpsApp.Application.Common
{
    public static class PaginationExtension
    {
        public static GetAllPagedRecord<T> ToPagedResponse<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize
           )
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalCount = query.Count();
            var items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new GetAllPagedRecord<T>
            {
                GetAllDataPaged = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
