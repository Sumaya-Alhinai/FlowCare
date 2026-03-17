using FlowCare.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FlowCare.Extensions
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int page,
            int size)
        {
            var total = await query.CountAsync();

            var results = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PagedResult<T>
            {
                Results = results,
                Total = total
            };
        }
    }
}