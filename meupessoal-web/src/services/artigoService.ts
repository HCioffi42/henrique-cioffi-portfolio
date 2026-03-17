import api from './api';
import type {Artigo, PagedArtigos} from '../models/Artigo';

// Centraliza as chamadas de busca de artigos para manter o componente App limpo.
export const getArtigos = async (page: number = 1, size: number = 10): Promise<PagedArtigos> => {
    const response = await api.get<PagedArtigos>(`/Artigos?pageNumber=${page}&pageSize=${size}`);
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
export const createArtigo = async (artigo: { titulo: string; resumo: string; conteudo: string; tags: string[] }): Promise<string> => {
    const response = await api.post<string>('/Artigos', artigo);
    return response.data;
};