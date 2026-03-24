import api from './api';
import type { Article, PagedArticles } from '../models/Article';
import type { ArticleSummary } from '../models/ArticleSummary';
import type { PagedResult } from '../models/PagedResult';
import { ArticleCategory } from '../models/ArticleCategory';

/**
 * Retrieves a paginated list of all articles (full entities).
 * @param page The page number to retrieve.
 * @param size The number of items per page.
 */
export const getArticles = async (page: number = 1, size: number = 10): Promise<PagedArticles> => {
    const response = await api.get<PagedArticles>(`/Articles?pageNumber=${page}&pageSize=${size}`);
    return response.data;
};

/**
 * Retrieves a paginated list of article summaries from the optimized backend endpoint.
 * This is the primary method for the home page article listing.
 * @param pageNumber The page number to fetch.
 * @param pageSize The number of items to display per page.
 * @param tags An optional array of tags to filter the results.
 * @param category An optional category to filter the results.
 * @returns A promise that resolves to a PagedResult containing ArticleSummary objects.
 */
export const getArticleSummaries = async (
    pageNumber: number = 1,
    pageSize: number = 6,
    tags?: string[],
    category?: ArticleCategory
): Promise<PagedResult<ArticleSummary>> => {
    const params = new URLSearchParams();
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    if (tags && tags.length > 0) {
        tags.forEach(tag => params.append('tags', tag));
    }

    if (category !== undefined) {
        params.append('category', category.toString());
    }

    const response = await api.get<PagedResult<ArticleSummary>>(`/Articles/summaries?${params.toString()}`);
    return response.data;
};

/**
 * Fetches a single article by its unique identifier.
 * @param id The unique GUID of the article.
 * @returns A promise containing the article details.
 */
export const getArticleById = async (id: string): Promise<Article> => {
    const response = await api.get<Article>(`/Articles/${id}`);
    return response.data;
};

/**
 * Sends a POST request to create a new article.
 * @param article An object containing the article's title, summary, content, tags, and category.
 * @returns A promise that resolves to the ID of the created article.
 */
export const createArticle = async (article: {
    title: string;
    summary: string;
    content: string;
    tags: string[];
    category: ArticleCategory;
}): Promise<string> => {
    const response = await api.post<string>('/Articles', article);
    return response.data;
};

/**
 * Sends a DELETE request to remove an article from the database.
 * @param id The unique identifier of the article to be deleted.
 */
export const deleteArticle = async (id: string): Promise<void> => {
    // HC: Using the centralized api instance which already includes the JWT token.
    await api.delete(`/articles/${id}`);
};

/**
 * Sends a PUT request to update an existing article.
 * @param id The ID of the article.
 * @param article The updated data matching the UpdateArticleCommand.
 */
export const updateArticle = async (id: string, article: any): Promise<void> => {
    await api.put(`/articles/${id}`, article);
};