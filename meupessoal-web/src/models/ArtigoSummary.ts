/**
 * Represents a summary of an article retrieved from the backend.
 * This model corresponds to the `ArtigoSummaryDto` class in the C# project.
 */
export interface ArtigoSummary {
    id: string;
    titulo: string;
    resumo: string;
    dataCriacao: string;
    tags: string[];
}
