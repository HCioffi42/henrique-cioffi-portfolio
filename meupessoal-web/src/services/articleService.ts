import api from './api';
import type { Article, PagedArticles } from '../models/Article';
import type { ArticleSummary } from '../models/ArticleSummary';
import type { PagedResult } from '../models/PagedResult';
import { ArticleCategory } from '../models/ArticleCategory';

export interface CreateArticleCommand {
  title: string;
  summary: string;
  content: string;
  tags: string[];
  category: ArticleCategory;
}

export type UpdateArticleCommand = CreateArticleCommand;

/**
 * Retrieves a paginated list of all articles.
 */
export const getArticles = async (page: number = 1, size: number = 10): Promise<PagedArticles> => {
    const response = await api.get<PagedArticles>('/Articles', {
        params: { pageNumber: page, pageSize: size }
    });
    return response.data;
};

/**
 * Retrieves a paginated list of article summaries.
 */
export const getArticleSummaries = async (
    pageNumber: number = 1,
    pageSize: number = 6,
    tags?: string[],
    category?: ArticleCategory
): Promise<PagedResult<ArticleSummary>> => {
    const response = await api.get<PagedResult<ArticleSummary>>('/Articles/summaries', {
        params: {
            pageNumber,
            pageSize,
            tags, 
            category
        }
    });
    return response.data;
};

/**
 * Searches for articles using a search term.
 */
export const searchArticles = async (
    searchTerm: string,
    pageNumber: number = 1,
    pageSize: number = 6
): Promise<PagedResult<ArticleSummary>> => {
    const response = await api.get<PagedResult<ArticleSummary>>('/Articles/search', {
        params: {
            searchTerm,
            pageNumber,
            pageSize
        }
    });
    return response.data;
};

/**
 * Retrieves a list of related articles for a specific article.
 */
export const getRelatedArticles = async (id: string, limit: number = 4): Promise<ArticleSummary[]> => {
    const response = await api.get<ArticleSummary[]>(`/Articles/${id}/related`, {
        params: { limit }
    });
    return response.data;
};

/**
 * Fetches a single article by its unique identifier.
 */
export const getArticleById = async (id: string): Promise<Article> => {
    const response = await api.get<Article>(`/Articles/${id}`);
    return response.data;
};

/**
 * Sends a POST request to create a new article.
 */
export const createArticle = async (article: CreateArticleCommand): Promise<string> => {
    const response = await api.post<string>('/Articles', article);
    return response.data;
};

/**
 * Sends a DELETE request to remove an article.
 */
export const deleteArticle = async (id: string): Promise<void> => {
    await api.delete(`/Articles/${id}`);
};

/**
 * Sends a PUT request to update an existing article.
 */
export const updateArticle = async (id: string, article: UpdateArticleCommand): Promise<void> => {
    await api.put(`/Articles/${id}`, article);
};