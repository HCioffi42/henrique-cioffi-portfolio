// Define a estrutura de um artigo vindo da API para garantir tipagem forte no frontend.
export interface Artigo {
    id: string;
    titulo: string;
    conteudo: string;
    resumo: string;
    tags: string[];
    dataCriacao: string;
}

// Representa a estrutura de paginação que o seu PagedList.cs retorna.
export interface PagedArtigos {
    items: Artigo[];
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalCount: number;
    hasPrevious: boolean;
    hasNext: boolean;
}