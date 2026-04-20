import { ArticleCategory } from './ArticleCategory';

// Defines the structure of an API article to ensure strong typing in the frontend.
export interface Article {
    id: string;
    title: string;
    titleEn: string;
    titlePt: string;
    content: string;
    contentEn: string;
    contentPt: string;
    summary: string;
    summaryEn: string;
    summaryPt: string;
    tags: string[];
    category: ArticleCategory;
    createdAt: string;
}

// Represents the pagination structure returned by PagedList.cs.
export interface PagedArticles {
    items: Article[];
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalCount: number;
    hasPrevious: boolean;
    hasNext: boolean;
}