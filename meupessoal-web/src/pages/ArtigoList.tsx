import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getArtigoSummaries } from '../services/artigoService';
import type { ArtigoSummary } from '../models/ArtigoSummary';
import { ArticleCard } from '../components/ArticleCard';
import Pagination from '../components/Pagination';

/**
 * The main article listing page (Home).
 * It fetches paginated summarized data from the optimized backend endpoint and renders a grid of cards.
 */
export const ArtigoList = () => {
    // Initialized URL search parameters state to drive pagination
    const [searchParams, setSearchParams] = useSearchParams();
    
    // State to track the total number of pages returned by the API
    const [totalPages, setTotalPages] = useState<number>(0);

    const [articles, setArticles] = useState<ArtigoSummary[]>([]);    
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    // Derives the current page directly from the URL (e.g., ?page=2), defaulting to 1
    const currentPage = Number(searchParams.get('page')) || 1;
    // Defines the fixed page size for the grid
    const pageSize = 6;

    useEffect(() => {
        /**
         * Orchestrates the paginated data fetching process from the backend.
         */
        const loadArticles = async () => {
            // Ensures loading state is active when transitioning between pages
            setLoading(true);
            try {
                // Passes pagination arguments and expects a PagedResult structure
                const data = await getArtigoSummaries(currentPage, pageSize);

                // Accesses the inner arrays and metadata from the PagedResult
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

        // Forces the browser window to scroll to the top upon changing pages
        window.scrollTo(0, 0);
    }, [currentPage]); // Re-runs the effect whenever the URL page parameter changes

    /**
     * Updates the URL search parameters to trigger a page transition.
     */
    const handlePageChange = (newPage: number) => {
        setSearchParams({ page: newPage.toString() });
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

            {articles.length === 0 ? (
                <div className="text-center py-20 bg-gray-50 rounded-2xl border-2 border-dashed border-gray-200">
                    <p className="text-gray-500 text-lg">No articles found yet. Stay tuned!</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    {articles.map(article => (
                        <ArticleCard key={article.id} article={article} />
                    ))}
                </div>
            )}

            {/* Conditionally renders the Pagination component if there is more than one page */}
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