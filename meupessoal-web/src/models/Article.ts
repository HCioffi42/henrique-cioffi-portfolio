// Define a estrutura de um artigo vindo da API para garantir tipagem forte no frontend.
export interface Article {
    id: string;
    title: string;
    content: string;
    summary: string;
    tags: string[];
    createdAt: string;
}

// Representa a estrutura de paginação que o seu PagedList.cs retorna.
export interface PagedArticles {
    items: Article[];
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalCount: number;
    hasPrevious: boolean;
    hasNext: boolean;
}