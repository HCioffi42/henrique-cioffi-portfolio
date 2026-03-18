using System.Collections.Generic;
using MeuSitePessoal.Application.Common.Models;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Common;

/// <summary>
/// Unit tests for the PagedResult generic class, focusing on metadata calculation accuracy.
/// </summary>
public class PagedResultTests
{
    [Theory]
    [InlineData(25, 10, 3)] // 2.5 rounded up to 3
    [InlineData(20, 10, 2)] // Exactly 2 pages
    [InlineData(5, 10, 1)]  // Less than one page
    [InlineData(0, 10, 0)]  // Empty set
    [InlineData(31, 10, 4)] // 3.1 rounded up to 4
    public void Constructor_ShouldCalculateTotalPagesCorrectly(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange: Prepares a dummy list of items.
        var items = new List<int>();

        // Act: Initializes the PagedResult with the provided metrics.
        var pagedResult = new PagedResult<int>(items, totalCount, 1, pageSize);

        // Assert: Verifies that the total page count is calculated correctly.
        Assert.Equal(expectedTotalPages, pagedResult.TotalPages);
    }

    [Fact]
    public void Constructor_ShouldAssignPropertiesCorrectly()
    {
        // Arrange: Prepares data for a standard pagination scenario.
        var items = new List<string> { "a", "b" };
        var totalCount = 50;
        var pageNumber = 2;
        var pageSize = 5;

        // Act: Initializes the PagedResult.
        var result = new PagedResult<string>(items, totalCount, pageNumber, pageSize);

        // Assert: Verifies that all property assignments match the input data.
        Assert.Equal(items, result.Items);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
    }
}
