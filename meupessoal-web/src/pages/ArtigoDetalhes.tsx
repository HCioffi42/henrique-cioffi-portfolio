import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import type { Artigo } from '../models/Artigo';
import { getArtigoById } from '../services/artigoService';

/**
 * Custom hook to handle article data fetching logic.
 * Isolates the side effect and state management from the component.
 */
const useArtigo = (id: string | undefined) => {
    const [artigo, setArtigo] = useState<Artigo | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchArtigo = async () => {
            if (!id) return;
            try {
                const data = await getArtigoById(id);
                setArtigo(data);
            } catch (err) {
                console.error("Error fetching article details:", err);
                setError("Article not found.");
            } finally {
                setLoading(false);
            }
        };

        void fetchArtigo();
    }, [id]);

    return { artigo, loading, error };
};

/**
 * Component responsible for rendering code blocks.
 * Bypasses Tailwind's prose background interference and uses a dark IDE theme.
 */
const CodeBlock = ({ inline, className, children, ...props }: any) => {
    const match = /language-(\w+)/.exec(className || '');
    const language = match ? match[1] : '';

    if (!inline && match) {
        return (
            // Outer div to control the rounded borders and the dark background of the frame.
            <div className="my-6 rounded-lg overflow-hidden bg-[#24292e] shadow-md">
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
                    }}
                    codeTagProps={{
                        style: { backgroundColor: 'transparent' }
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
            components={{ code: CodeBlock }}
        >
            {content}
        </ReactMarkdown>
    </section>
);

/**
 * Component that renders the article header.
 */
const ArticleHeader = ({ titulo, dataCriacao }: { titulo: string; dataCriacao: string }) => (
    <header className="mb-10">
        <h1 className="text-4xl font-extrabold text-gray-900 mb-4 tracking-tight">
            {titulo}
        </h1>
        <p className="text-sm text-gray-500">
            Published on {new Date(dataCriacao).toLocaleDateString()}
        </p>
    </header>
);

/**
 * Main Page component for Article Details.
 * Orchestrates logic and sub-components.
 */
export const ArtigoDetalhes = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { artigo, loading, error } = useArtigo(id);

    if (loading) return <div className="p-8 text-center text-gray-500">Loading article...</div>;
    if (error || !artigo) return <div className="p-8 text-center text-red-500">{error || "Not found"}</div>;

    return (
        <div className="max-w-3xl mx-auto p-8 animate-in fade-in duration-500">
            <button
                onClick={() => navigate(-1)}
                className="mb-8 text-indigo-600 hover:text-indigo-800 font-medium flex items-center gap-2 transition-colors"
            >
                &larr; Back to list
            </button>

            <ArticleHeader titulo={artigo.titulo} dataCriacao={artigo.dataCriacao} />

            <MarkdownRenderer content={artigo.conteudo} />

            <footer className="mt-12 pt-8 border-t border-gray-100 flex flex-wrap gap-2">
                {artigo.tags.map(tag => (
                    <span
                        key={tag}
                        className="bg-indigo-50 text-indigo-600 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider"
                    >
                        #{tag}
                    </span>
                ))}
            </footer>
        </div>
    );
};