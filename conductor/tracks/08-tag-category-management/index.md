# Track: Tag & Category Management

## Overview
Initialization and implementation of specialized routing for tags and category-based filtering. This track extends the existing tag system by introducing a dedicated category field for articles and a dynamic URL-driven page for single-tag discovery.

## Status
- [x] Specification drafted
- [x] Backend: Article entity updated with Category property
- [x] Backend: GetArticlesQuery updated to support Category filtering
- [x] Frontend: /tags/:tag route implemented
- [x] Frontend: Create/Edit article forms updated with Category
- [x] Validation and Tests completed

## Results
- **Category Enum:** Defined with Technology, Tutorial, Life, News, Opinion, Projects.
- **SQL Intersection:** Optimized tag intersection logic using `Aggregate` on `IQueryable`.
- **Dynamic Routing:** `/tags/:tag` route captures and filters articles dynamically.
- **Form Integration:** Dropdown selects in admin forms ensure data consistency.
- **Enhanced UI:** Dynamic headers and document titles reflect active filters.

## Related Tracks
- [Track 03: Tag-based Filtering](../03-tag-based-filtering/index.md) (The base multi-tag logic).
- [Track 05: Article CRUD](../05-article-crud-edit-delete/index.md) (Form updates).
