import { useNavigate } from 'react-router-dom';
import type { ArtigoSummary } from '../models/ArtigoSummary';

interface ArticleCardProps {
    article: ArtigoSummary;
}

/**
 * A reusable card component for displaying article summaries.
 * It provides a clean, minimalist layout with navigation to the full article.
 */
export const ArticleCard = ({ article }: ArticleCardProps) => {
    const navigate = useNavigate();

    // Formats the ISO date string into a localized, human-readable format.
    const formattedDate = new Intl.DateTimeFormat('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    }).format(new Date(article.dataCriacao));

    return (
        <article className="bg-white border border-gray-100 rounded-xl p-6 shadow-sm hover:shadow-lg transition-all duration-300 flex flex-col h-full">
            <header className="mb-4">
                <div className="flex items-center justify-between text-xs text-gray-400 mb-2">
                    <time dateTime={article.dataCriacao}>{formattedDate}</time>
                </div>
                <h2 className="text-xl font-bold text-gray-900 leading-tight hover:text-indigo-600 transition-colors cursor-pointer"
                    onClick={() => navigate(`/artigo/${article.id}`)}>
                    {article.titulo}
                </h2>
            </header>

            <p className="text-gray-600 text-sm line-clamp-3 mb-6 flex-grow">
                {article.resumo}
            </p>

            <footer className="mt-auto">
                <div className="flex flex-wrap gap-1.5 mb-4">
                    {article.tags.map(tag => (
                        <span key={tag} className="px-2.5 py-0.5 bg-indigo-50 text-indigo-600 rounded-full text-[10px] font-semibold uppercase tracking-wider">
                            {tag}
                        </span>
                    ))}
                </div>

                <button
                    onClick={() => navigate(`/artigo/${article.id}`)}
                    className="w-full py-2 px-4 bg-gray-50 text-gray-700 text-sm font-semibold rounded-lg hover:bg-indigo-600 hover:text-white transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2"
                >
                    Read More
                </button>
            </footer>
        </article>
    );
};
