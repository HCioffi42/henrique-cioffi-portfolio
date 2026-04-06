import { useEffect, useState, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { getArticleSummaries, searchArticles } from '../services/articleService';
import type { ArticleSummary } from '../models/ArticleSummary';
import { ArticleCard } from '../components/ArticleCard';
import Pagination from '../components/Pagination';
import { ArticleCategory, ArticleCategoryLabels } from '../models/ArticleCategory';
import { SEO } from '../components/SEO';
import { NewsletterBox } from '../components/NewsletterBox';

const PAGE_SIZE = 6;

/**
 * The main article listing page (Home).
 * It fetches paginated summarized data and supports URL-based filtering for tags, categories, and keywords.
 */
const ArticleList = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const [totalPages, setTotalPages] = useState<number>(0);
    const [articles, setArticles] = useState<ArticleSummary[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const [error, setError] = useState<string | null>(null);

    const currentPage = Number(searchParams.get('page')) || 1;
    const queryCategory = searchParams.get('category');
    const category = queryCategory ? (Number(queryCategory) as ArticleCategory) : undefined;
    
    // HC: Retrieves the search term 'q' from the URL.
    const searchTerm = searchParams.get('q') || '';

    const currentTags = useMemo(() => {
        return searchParams.getAll('tags');
    }, [searchParams]);

    const tagsKey = currentTags.join(',');

    useEffect(() => {
        const loadArticles = async () => {
            setLoading(true);
            try {
                // HC: Logic branch: if there is a search term, use the search service. 
                // Otherwise, use the standard summary listing with tags/category.
                const data = searchTerm.trim()
                    ? await searchArticles(searchTerm, currentPage, PAGE_SIZE)
                    : await getArticleSummaries(currentPage, PAGE_SIZE, currentTags, category);

                setArticles(data.items);
                setTotalPages(data.totalPages);
                setError(null);
            } catch (err) {
                console.error("Failed to load articles:", err);
                setError("Unable to load articles at this time.");
            } finally {
                setLoading(false);
            }
        };

        void loadArticles();
        window.scrollTo(0, 0);
    }, [currentPage, tagsKey, category, searchTerm, currentTags]); 

    const { seoTitle, seoDescription } = useMemo(() => {
        let title = 'Insights & Articles';
        let description = 'Exploring the intersection of technology, design, and software engineering. Portfolio and blog by Henrique Cioffi.';
        
        if (searchTerm) {
            title = `Search results for: ${searchTerm}`;
            description = `Viewing articles matching the search term "${searchTerm}".`;
        } else if (currentTags.length > 0) {
            title = `Articles tagged #${currentTags.join(', #')}`;
            description = `Discovering content related to ${currentTags.join(' and ')}. Articles and insights on software development and design.`;
        } else if (category !== undefined) {
            title = `Category: ${ArticleCategoryLabels[category]}`;
            description = `All articles filed under the ${ArticleCategoryLabels[category]} category. Focused insights on technology and engineering.`;
        }
        
        return { seoTitle: title, seoDescription: description };
    }, [currentTags, category, searchTerm]);

    const handlePageChange = (newPage: number) => {
        const newParams = new URLSearchParams(searchParams);
        newParams.set('page', newPage.toString());
        setSearchParams(newParams);
    };

    const removeTagFilter = (tagToRemove: string) => {
        const newParams = new URLSearchParams(searchParams);
        const remaining = currentTags.filter(t => t !== tagToRemove);
        newParams.delete('tags');
        remaining.forEach(t => newParams.append('tags', t));
        newParams.set('page', '1');
        setSearchParams(newParams);
    };

    const removeCategoryFilter = () => {
        const newParams = new URLSearchParams(searchParams);
        newParams.delete('category');
        newParams.set('page', '1');
        setSearchParams(newParams);
    };

    // HC: New function to clear the search term from the URL.
    const removeSearchFilter = () => {
        const newParams = new URLSearchParams(searchParams);
        newParams.delete('q');
        newParams.set('page', '1');
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
            <SEO title={seoTitle} description={seoDescription} />
            <header className="mb-12 text-center">
                <h1 className="text-4xl font-extrabold text-gray-900 dark:text-slate-100 tracking-tight sm:text-5xl mb-4">
                    {searchTerm 
                        ? `Results for "${searchTerm}"`
                        : currentTags.length > 0 
                            ? `Filtering by ${currentTags.length} tags`
                            : category !== undefined 
                                ? `Browsing: ${ArticleCategoryLabels[category]}`
                                : 'Insights & Articles'}
                </h1>
                <p className="text-lg text-gray-500 dark:text-slate-400 max-w-2xl mx-auto">
                    {searchTerm
                        ? `Found ${articles.length} articles that match your search.`
                        : currentTags.length > 0 
                            ? `Discovering content related to ${currentTags.join(' and ')}.`
                            : category !== undefined
                                ? `All articles filed under the ${ArticleCategoryLabels[category]} category.`
                                : 'Exploring the intersection of technology, design, and software engineering.'}
                </p>
                <div className="mt-8 flex justify-center">
                    <div className="w-24 h-1 bg-indigo-600 rounded-full"></div>
                </div>
            </header>

            {(currentTags.length > 0 || category !== undefined || searchTerm) && (
                <div className="mb-8 flex flex-col items-center justify-center gap-3">
                    <span className="text-sm text-gray-500 dark:text-slate-500 uppercase tracking-widest font-semibold">Active Filters</span>
                    <div className="flex flex-wrap gap-2 justify-center">
                        {/* HC: Renders the active search filter if present. */}
                        {searchTerm && (
                            <div className="inline-flex items-center gap-2 bg-indigo-600 px-4 py-1.5 rounded-full shadow-sm">
                                <span className="text-sm text-white font-bold tracking-wide">
                                    Search: {searchTerm}
                                </span>
                                <button onClick={removeSearchFilter} className="text-indigo-200 hover:text-white rounded-full p-0.5 transition-colors cursor-pointer">
                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                                </button>
                            </div>
                        )}

                        {category !== undefined && (
                             <div className="inline-flex items-center gap-2 bg-amber-50 dark:bg-amber-950/30 border border-amber-100 dark:border-amber-900/50 px-4 py-1.5 rounded-full shadow-sm">
                                <span className="text-sm text-amber-800 dark:text-amber-500 font-bold tracking-wide">
                                    Category: {ArticleCategoryLabels[category]}
                                </span>
                                <button onClick={removeCategoryFilter} className="text-amber-400 hover:text-red-500 rounded-full p-0.5 transition-colors cursor-pointer">
                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                                </button>
                            </div>
                        )}
                        {currentTags.map(tag => (
                            <div key={tag} className="inline-flex items-center gap-2 bg-indigo-50 dark:bg-indigo-950/30 border border-indigo-100 dark:border-indigo-900/50 px-4 py-1.5 rounded-full shadow-sm">
                                <span className="text-sm text-indigo-800 dark:text-indigo-400 font-bold tracking-wide capitalize">#{tag}</span>
                                <button onClick={() => removeTagFilter(tag)} className="text-indigo-400 hover:text-red-500 rounded-full p-0.5 transition-colors cursor-pointer">
                                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
                                </button>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {articles.length === 0 ? (
                <div className="text-center py-20 bg-gray-50 dark:bg-slate-900/50 rounded-2xl border-2 border-dashed border-gray-200 dark:border-slate-800">
                    <p className="text-gray-500 dark:text-slate-400 text-lg">No articles match these combined filters.</p>
                </div>
            ) : (
                <>
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                        {articles.map(article => (
                            <ArticleCard key={article.id} article={article} />
                        ))}
                    </div>
                </>
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

export default ArticleList;