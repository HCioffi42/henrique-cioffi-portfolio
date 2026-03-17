import api from './api';
import type { Artigo, PagedArtigos } from '../models/Artigo';
import type { ArtigoSummary } from '../models/ArtigoSummary';
import type { PagedResult } from '../models/PagedResult';

/**
 * Retrieves a paginated list of all articles (full entities).
 * @param page The page number to retrieve.
 * @param size The number of items per page.
 */
export const getArtigos = async (page: number = 1, size: number = 10): Promise<PagedArtigos> => {
    const response = await api.get<PagedArtigos>(`/Artigos?pageNumber=${page}&pageSize=${size}`);
    return response.data;
};

/**
 * Retrieves a paginated list of article summaries from the optimized backend endpoint.
 * This is the primary method for the home page article listing.
 * @param pageNumber The page number to fetch.
 * @param pageSize The number of items to display per page.
 * @param tags An optional array of tags to filter the results.
 * @returns A promise that resolves to a PagedResult containing ArtigoSummary objects.
 */
export const getArtigoSummaries = async (
    pageNumber: number = 1,
    pageSize: number = 6,
    tags?: string[] 
): Promise<PagedResult<ArtigoSummary>> => {
    const params = new URLSearchParams();
    params.append('pageNumber', pageNumber.toString());
    params.append('pageSize', pageSize.toString());

    if (tags && tags.length > 0) {
        tags.forEach(tag => params.append('tags', tag));
    }

    const response = await api.get<PagedResult<ArtigoSummary>>(`/Artigos/summaries?${params.toString()}`);
    return response.data;
};

/**
 * Fetches a single article by its unique identifier.
 * @param id The unique GUID of the article.
 * @returns A promise containing the article details.
 */
export const getArtigoById = async (id: string): Promise<Artigo> => {
    const response = await api.get<Artigo>(`/Artigos/${id}`);
    return response.data;
};

/**
 * Sends a POST request to create a new article.
 * @param artigo An object containing the article's title, summary, content, and tags.
 * @returns A promise that resolves to the ID of the created article.
 */
export const createArtigo = async (artigo: {
    titulo: string;
    resumo: string;
    conteudo: string;
    tags: string[]
}): Promise<string> => {
    const response = await api.post<string>('/Artigos', artigo);
    return response.data;
};