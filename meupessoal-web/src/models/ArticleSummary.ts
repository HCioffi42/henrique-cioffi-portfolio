import { ArticleCategory } from './ArticleCategory';

/**
 * Represents a summary of an article retrieved from the backend.
 * This model corresponds to the `ArticleSummaryDto` class in the C# project.
 */
export interface ArticleSummary {
    id: string;
    title: string;
    summary: string;
    createdAt: string;
    tags: string[];
    category: ArticleCategory;
}
