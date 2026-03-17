using System;
using System.Collections.Generic;

namespace MeuSitePessoal.Application.Common.Models;

/// <summary>
/// HC: A generic wrapper for paginated results.
/// </summary>
/// <typeparam name="T">The type of the items being paginated.</typeparam>
public class PagedResult<T>
{
    public List<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
