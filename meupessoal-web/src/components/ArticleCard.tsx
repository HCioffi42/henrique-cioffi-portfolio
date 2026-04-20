import { useNavigate, useSearchParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import type { ArticleSummary } from '../models/ArticleSummary';

interface ArticleCardProps {
    article: ArticleSummary;
}

/**
 * A reusable card component for displaying article summaries.
 * It provides a clean, minimalist layout with navigation to the full article and clickable tags for filtering.
 */
export const ArticleCard = ({ article }: ArticleCardProps) => {
    const { t, i18n } = useTranslation();
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    // Formats the ISO date string into a localized, human-readable format.
    const formattedDate = new Intl.DateTimeFormat(i18n.language, {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    }).format(new Date(article.createdAt));

    // Localized category mapping
    const getCategoryLabel = (cat: number) => {
        const categories: Record<number, string> = {
            1: t('categories.technology'),
            2: t('categories.tutorial'),
            3: t('categories.life'),
            4: t('categories.news'),
            5: t('categories.opinion'),
            6: t('categories.projects')
        };
        return categories[cat] || t('categories.technology');
    };

    /**
     * Handles tag clicks by appending the new tag to current filters and navigating back to the home list with all parameters.
     */
    const handleTagClick = (tag: string) => {
        const newParams = new URLSearchParams(searchParams);
        const currentTags = newParams.getAll('tags');
        const lowerTag = tag.toLowerCase();

        // Only append if the tag is not already active
        if (!currentTags.includes(lowerTag)) {
            newParams.append('tags', lowerTag);
            newParams.set('page', '1'); // Reset pagination when adding a filter
        }

        navigate(`/?${newParams.toString()}`);
    };

    return (
        <article className="bg-white dark:bg-slate-900 border border-gray-100 dark:border-slate-800 rounded-xl p-6 shadow-sm hover:shadow-lg transition-all duration-300 flex flex-col h-full group">
            <header className="mb-4">
                <div className="flex items-center justify-between text-xs text-gray-400 dark:text-slate-500 mb-2">
                    <time dateTime={article.createdAt}>{formattedDate}</time>
                    <button 
                        onClick={(e) => {
                            e.stopPropagation();
                            navigate(`/?category=${article.category}`);
                        }}
                        className="px-2 py-0.5 bg-amber-50 dark:bg-amber-950/30 text-amber-700 dark:text-amber-500 rounded-md font-bold uppercase tracking-tighter hover:bg-amber-100 dark:hover:bg-amber-900/50 transition-colors"
                    >
                        {getCategoryLabel(article.category)}
                    </button>
                </div>
                <h2 className="text-xl font-bold text-gray-900 dark:text-slate-100 leading-tight group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors cursor-pointer"
                    onClick={() => navigate(`/article/${article.id}`)}>
                    {article.title}
                </h2>
            </header>

            <p className="text-gray-600 dark:text-slate-400 text-sm line-clamp-3 mb-6 flex-grow">
                {article.summary}
            </p>

            <footer className="mt-auto">
                <div className="flex flex-wrap gap-1.5 mb-4">
                    {article.tags.map(tag => (
                        <button
                            key={tag}
                            onClick={(e) => {
                                e.stopPropagation();
                                handleTagClick(tag);
                            }}
                            className="px-2.5 py-0.5 bg-indigo-50 dark:bg-indigo-950/30 text-indigo-600 dark:text-indigo-400 rounded-full text-[10px] font-semibold capitalize
                                        tracking-wider hover:bg-indigo-100 dark:hover:bg-indigo-900/50 hover:text-indigo-800 dark:hover:text-indigo-300 transition-colors cursor-pointer">
                            #{tag}
                        </button>
                    ))}
                </div>

                <button
                    onClick={() => navigate(`/article/${article.id}`)}
                    className="w-full py-2 px-4 bg-gray-50 dark:bg-slate-800 text-gray-700 dark:text-slate-300 text-sm font-semibold rounded-lg hover:bg-indigo-600 
                            hover:text-white transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2">
                    {t('common.readMore')}
                </button>
            </footer>
        </article>
    );
};