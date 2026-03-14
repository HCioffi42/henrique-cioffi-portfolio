using System.Collections.Generic;
using MeuSitePessoal.Application.Common.Models;
using Xunit;

namespace MeuSitePessoal.Tests.Unit.Application.Common
{
    public class PagedListTests
    {
        [Fact]
        public void PagedList_ShouldCalculateMetadataCorrectly()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3 };
            var count = 25;
            var pageNumber = 1;
            var pageSize = 10;

            // Act
            var pagedList = new PagedList<int>(items, count, pageNumber, pageSize);

            // Assert
            Assert.Equal(3, pagedList.TotalPages);
            Assert.Equal(1, pagedList.CurrentPage);
            Assert.Equal(10, pagedList.PageSize);
            Assert.Equal(25, pagedList.TotalCount);
            Assert.True(pagedList.HasNext);
            Assert.False(pagedList.HasPrevious);
        }

        [Fact]
        public void PagedList_ShouldHandlePartialPageCorrectly()
        {
            // Arrange
            var items = new List<int> { 1, 2, 3, 4, 5 };
            var count = 5;
            var pageNumber = 1;
            var pageSize = 10;

            // Act
            var pagedList = new PagedList<int>(items, count, pageNumber, pageSize);

            // Assert
            Assert.Equal(1, pagedList.TotalPages);
            Assert.False(pagedList.HasNext);
            Assert.False(pagedList.HasPrevious);
        }

        [Fact]
        public void PagedList_LastPage_ShouldHavePreviousAndNoNext()
        {
            // Arrange
            var items = new List<int> { 21, 22, 23, 24, 25 };
            var count = 25;
            var pageNumber = 3;
            var pageSize = 10;

            // Act
            var pagedList = new PagedList<int>(items, count, pageNumber, pageSize);

            // Assert
            Assert.Equal(3, pagedList.TotalPages);
            Assert.False(pagedList.HasNext);
            Assert.True(pagedList.HasPrevious);
        }
    }
}
