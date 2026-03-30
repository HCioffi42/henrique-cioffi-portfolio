import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext'; 
import { DeleteModal } from '../components/DeleteModal';
import { MarkdownRenderer } from '../components/MarkdownRenderer';
import { deleteArticle, getArticleById } from '../services/articleService';
import type { Article } from '../models/Article';
import { ArticleCategoryLabels } from '../models/ArticleCategory';
import { SEO } from '../components/SEO';

/**
 * Custom hook to handle article data fetching logic.
 * Isolates the side effect and state management from the component.
 */
const useArticle = (id: string | undefined) => {
    const [article, setArticle] = useState<Article | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchArticle = async () => {
            if (!id) return;
            try {
                const data = await getArticleById(id);
                setArticle(data);
            } catch (err) {
                console.error("Error fetching article details:", err);
                setError("Article not found.");
            } finally {
                setLoading(false);
            }
        };

        void fetchArticle();
    }, [id]);

    return { article: article, loading, error };
};

/**
 * Component that renders the article header containing the title and publication date.
 */
const ArticleHeader = ({ title, createdAt, category }: { title: string; createdAt: string; category: number }) => (
    <header className="mb-10">
        <div className="flex items-center gap-3 mb-4">
            <span className="px-3 py-1 bg-amber-50 text-amber-700 text-xs font-bold uppercase tracking-wider rounded-md">
                {ArticleCategoryLabels[category as keyof typeof ArticleCategoryLabels]}
            </span>
            <span className="text-gray-300">|</span>
            <p className="text-sm text-gray-500 font-medium">
                Published on {new Date(createdAt).toLocaleDateString()}
            </p>
        </div>
        <h1 className="text-4xl font-extrabold text-gray-900 mb-4 tracking-tight leading-tight">
            {title}
        </h1>
    </header>
);

/**
 * Main Page component for Article Details.
 * Orchestrates logic and sub-components.
 */
export const ArticleDetails = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { user } = useAuth();
    const { article, loading, error } = useArticle(id);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const handleDelete = async () => {
        if (!id) return;
        try {
            await deleteArticle(id);
            navigate('/admin/dashboard');
        } catch (err) {
            console.error("Failed to delete article", err);
        }
    };

    if (loading) return <div className="p-8 text-center text-gray-500">Loading article...</div>;
    if (error || !article) return <div className="p-8 text-center text-red-500">{error || "Not found"}</div>;

    // SEO specific derived data
    const description = article.content.substring(0, 160).replace(/[#*`]/g, '').trim() + '...';
    const keywords = article.tags.join(', ');

    return (
        <div className="max-w-3xl mx-auto p-8 animate-in fade-in duration-500">
            <SEO 
                title={article.title}
                description={description}
                keywords={keywords}
                type="article"
                articleData={{
                    publishedTime: article.createdAt,
                    tags: article.tags,
                    section: ArticleCategoryLabels[article.category as keyof typeof ArticleCategoryLabels]
                }}
            />

            <button
                onClick={() => navigate(-1)}
                className="mb-8 text-indigo-600 hover:text-indigo-800 font-medium
                            flex items-center gap-2 transition-colors focus:outline-none">
                &larr; Back to list
            </button>

            {/* HC: Conditional rendering of admin actions if a user session is active. */}
                {user && (
                    <div className="flex gap-3">
                        <Link 
                            to={`/admin/articles/edit/${article.id}`}
                            className="bg-amber-50 text-amber-700 px-4 py-2 rounded-lg font-bold text-sm hover:bg-amber-100 transition">
                            Edit Article
                        </Link>
                        <button 
                            onClick={() => setIsModalOpen(true)}
                            className="bg-red-50 text-red-700 px-4 py-2 rounded-lg font-bold text-sm hover:bg-red-100 transition">
                            Delete
                        </button>
                    </div>
                )}

            <ArticleHeader 
                title={article.title} 
                createdAt={article.createdAt} 
                category={article.category} 
            />

            <MarkdownRenderer content={article.content} />

            <footer className="mt-12 pt-8 border-t border-gray-100 flex flex-wrap gap-2">
                {article.tags.map(tag => (
                    <button
                        key={tag}
                        onClick={() => navigate(`/?tags=${tag.toLowerCase()}`)}
                        className="bg-indigo-50 text-indigo-600 px-3 py-1 
                                    rounded-full text-xs font-semibold capitalize tracking-wider 
                                    hover:bg-indigo-100 hover:text-indigo-800 transition-colors cursor-pointer 
                                    focus:outline-none focus:ring-2 focus:ring-indigo-500">
                        #{tag}
                    </button>
                ))}
            </footer>

            {/* HC: Safe confirmation modal triggered by the admin delete button. */}
            <DeleteModal 
                isOpen={isModalOpen}
                title={article.title}
                onConfirm={handleDelete}
                onCancel={() => setIsModalOpen(false)}/>
        </div>
    );
};