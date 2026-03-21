import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext'; 
import { DeleteModal } from '../components/DeleteModal';
import { deleteArticle, getArticleById } from '../services/articleService';
import type { Article } from '../models/Article';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';

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
 * Component responsible for rendering code blocks.
 * Bypasses Tailwind's prose background interference and uses a dark IDE theme.
 */
const CodeBlock = ({ inline, className, children, ...props }: any) => {
    const match = /language-(\w+)/.exec(className || '');
    const language = match ? match[1] : '';
    // State that tracks whether the code was recently copied to provide visual feedback.
    const [isCopied, setIsCopied] = useState(false);

    /**
     * Function that extracts the raw text from the code block and
     * uses the modern Clipboard API to copy it to the user's clipboard.
     */
    const handleCopy = async () => {
        const codeText = String(children).replace(/\n$/, '');
        try {
            await navigator.clipboard.writeText(codeText);
            setIsCopied(true);
            setTimeout(() => setIsCopied(false), 2000);
        } catch (err) {
            console.error("Failed to copy text:", err);
        }
    };

    if (!inline && match) {
        return (
            <div className="my-6 rounded-lg overflow-hidden bg-[#24292e] shadow-md">
                <div className="bg-[#1b1f23] px-4 py-2 text-xs font-mono text-gray-200 capitalize tracking-wider border-b border-gray-700/50 flex justify-between items-center">
                    <span>{language}</span>
                    <button
                        onClick={handleCopy}
                        className="text-gray-200 hover:text-white transition-colors focus:outline-none cursor-pointer"
                        aria-label="Copy code to clipboard"
                        title="Copy code">
                        {isCopied ? "Copied!" : "Copy"}
                    </button>
                </div>

                <SyntaxHighlighter
                    style={vscDarkPlus}
                    language={language}
                    PreTag="div"
                    customStyle={{
                        margin: 0,
                        padding: '1rem',
                        background: 'transparent',
                        fontSize: '1rem',
                        fontWeight: 'bold',
                        lineHeight: '1.4',
                        overflowX: 'auto',
                    }}
                    codeTagProps={{
                        style: {
                            backgroundColor: 'transparent',
                            borderWidth: 0,
                            padding: 0
                        }
                    }}
                    {...props}
                >
                    {String(children).replace(/\n$/, '')}
                </SyntaxHighlighter>
            </div>
        );
    }

    return (
        <code className="text-indigo-600 bg-indigo-50 px-1.5 py-0.5 rounded font-mono text-sm" {...props}>
            {children}
        </code>
    );
};

/**
 * Component responsible for rendering the Markdown content.
 * The developer centralized markdown configuration here, injecting the custom CodeBlock.
 */
const MarkdownRenderer = ({ content }: { content: string }) => (
    <section className="markdown-content prose max-w-none text-gray-800 leading-relaxed text-lg">
        <ReactMarkdown
            remarkPlugins={[remarkGfm]}
            components={{ code: CodeBlock, pre: ({ children }) => <>{children}</> }} >
            {content}
        </ReactMarkdown>
    </section>
);

/**
 * Component that renders the article header containing the title and publication date.
 */
const ArticleHeader = ({ title, createdAt }: { title: string; createdAt: string }) => (
    <header className="mb-10">
        <h1 className="text-4xl font-extrabold text-gray-900 mb-4 tracking-tight">
            {title}
        </h1>
        <p className="text-sm text-gray-500">
            Published on {new Date(createdAt).toLocaleDateString()}
        </p>
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

    return (
        <div className="max-w-3xl mx-auto p-8 animate-in fade-in duration-500">
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

            <ArticleHeader title={article.title} createdAt={article.createdAt} />

            <MarkdownRenderer content={article.content} />

            <footer className="mt-12 pt-8 border-t border-gray-100 flex flex-wrap gap-2">
                {article.tags.map(tag => (
                    // I updated the query parameter key from 'tag' to 'tags' to match the new multi-tag routing logic.
                    <button
                        key={tag}
                        onClick={() => navigate(`/?page=1&tags=${encodeURIComponent(tag)}`)}
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