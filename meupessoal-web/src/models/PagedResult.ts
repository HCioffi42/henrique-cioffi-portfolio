/**
 * Represents a standardized paginated response from the backend API.
 * This interface mirrors the C# PagedResult<T> class.
 */
export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}