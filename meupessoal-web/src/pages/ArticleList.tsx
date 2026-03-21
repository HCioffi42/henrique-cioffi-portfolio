import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getArticleSummaries } from '../services/articleService';
import type { ArticleSummary } from '../models/ArticleSummary';
import { ArticleCard } from '../components/ArticleCard';
import Pagination from '../components/Pagination';

/**
 * The main article listing page (Home).
 * It fetches paginated summarized data from the optimized backend endpoint and renders a grid of cards.
 * It also supports URL-based tag filtering.
 */
export const ArticleList = () => {
    // Initializes URL search parameters state to drive pagination and filtering.
    const [searchParams, setSearchParams] = useSearchParams();

    // State to track the total number of pages returned by the API.
    const [totalPages, setTotalPages] = useState<number>(0);

    const [articles, setArticles] = useState<ArticleSummary[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    // Derives the current page directly from the URL (e.g., ?page=2), defaulting to 1.
    const currentPage = Number(searchParams.get('page')) || 1;

    // Extracts the active tag filter from the URL parameters.
    const currentTags = searchParams.getAll('tags');

    // Defines the fixed page size for the grid.
    const pageSize = 6;

    useEffect(() => {
        /**
         * Orchestrates the paginated data fetching process from the backend, applying filters if necessary.
         */
        const loadArticles = async () => {
            // Ensures loading state is active when transitioning between pages or tags.
            setLoading(true);
            try {
                // Passes both pagination arguments and the active tag to the API.
                const data = await getArticleSummaries(currentPage, pageSize, currentTags);

                // Accesses the inner arrays and metadata from the PagedResult.
                setArticles(data.items);
                setTotalPages(data.totalPages);
                setError(null);
            } catch (err) {
                console.error("Failed to load article summaries:", err);
                setError("Unable to load articles at this time. Please try again later.");
            } finally {
                setLoading(false);
            }
        };

        void loadArticles();

        window.scrollTo(0, 0);
    }, [currentPage, currentTags.join(',')]);

    /**
     * Updates the URL search parameters to trigger a page transition while preserving the active tag.
     */
    const handlePageChange = (newPage: number) => {
        const newParams = new URLSearchParams();
        newParams.append('page', newPage.toString());
        currentTags.forEach(tag => newParams.append('tags', tag));
        setSearchParams(newParams);
    };

    /**
     * Removes a specific tag from the active filters and resets the view to the first page.
     */
    const removeTagFilter = (tagToRemove: string) => {
        const newParams = new URLSearchParams();
        newParams.append('page', '1');

        const remainingTags = currentTags.filter(t => t !== tagToRemove);
        remainingTags.forEach(tag => newParams.append('tags', tag));

        setSearchParams(newParams);
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center min-h-[400px]">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600"></div>
                <span className="ml-4 text-gray-500 font-medium">Loading stories...</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="max-w-4xl mx-auto p-8 text-center">
                <p className="text-red-500 font-medium bg-red-50 p-4 rounded-lg border border-red-100">{error}</p>
            </div>
        );
    }

    return (
        <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
            <header className="mb-12 text-center">
                <h1 className="text-4xl font-extrabold text-gray-900 tracking-tight sm:text-5xl mb-4">
                    Insights & Articles
                </h1>
                <p className="text-lg text-gray-500 max-w-2xl mx-auto">
                    Exploring the intersection of technology, design, and software engineering.
                </p>
                <div className="mt-8 flex justify-center">
                    <div className="w-24 h-1 bg-indigo-600 rounded-full"></div>
                </div>
            </header>

            {currentTags.length > 0 && (
                <div className="mb-8 flex flex-col items-center justify-center gap-3">
                    <span className="text-sm text-gray-500 uppercase tracking-widest font-semibold">Active Filters</span>
                    <div className="flex flex-wrap gap-2 justify-center">
                        {currentTags.map(tag => (
                            <div key={tag} className="inline-flex items-center gap-2 bg-indigo-50 border border-indigo-100 px-4 py-1.5 rounded-full shadow-sm">
                                <span className="text-sm text-indigo-800 font-bold tracking-wide capitalize">
                                    {tag}
                                </span>
                                <button
                                    onClick={() => removeTagFilter(tag)}
                                    className="text-indigo-400 hover:text-red-500 hover:bg-red-50 rounded-full p-0.5 transition-colors focus:outline-none"
                                    aria-label={`Remove ${tag} filter`}>
                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                    </svg>
                                </button>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {articles.length === 0 ? (
                <div className="text-center py-20 bg-gray-50 rounded-2xl border-2 border-dashed border-gray-200">
                    <p className="text-gray-500 text-lg">No articles match the selected filters. Try removing some tags.</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    {articles.map(article => (
                        <ArticleCard key={article.id} article={article} />
                    ))}
                </div>
            )}

            {articles.length > 0 && totalPages > 1 && (
                <div className="mt-12">
                    <Pagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={handlePageChange}
                    />
                </div>
            )}
        </main>
    );
};