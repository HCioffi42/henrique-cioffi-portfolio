import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import type { Artigo } from '../models/Artigo';
import { getArtigoById } from '../services/artigoService';

/**
 * Specialized component to render Markdown code blocks with syntax highlighting.
 * Mimics the JetBrains Rider / GitHub Dark aesthetic.
 */
export const ArtigoDetalhes = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [artigo, setArtigo] = useState<Artigo | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchArtigo = async () => {
            if (!id) return;
            try {
                const data = await getArtigoById(id);
                setArtigo(data);
            } catch (error) {
                console.error("Erro ao carregar detalhes do artigo:", error);
            } finally {
                setLoading(false);
            }
        };

        void fetchArtigo();
    }, [id]);

    if (loading) return <div className="p-8 text-center text-gray-500">Loading article...</div>;
    if (!artigo) return <div className="p-8 text-center text-red-500">Article not found.</div>;

    return (
        <div className="max-w-3xl mx-auto p-8 animate-in fade-in duration-500">
            <button
                onClick={() => navigate(-1)}
                className="mb-8 text-indigo-600 hover:text-indigo-800 font-medium flex items-center gap-2 transition-colors"
            >
                &larr; Back to list
            </button>

            <header className="mb-10">
                <h1 className="text-4xl font-extrabold text-gray-900 mb-4 tracking-tight">
                    {artigo.titulo}
                </h1>
                <p className="text-sm text-gray-500">
                    Published on {new Date(artigo.dataCriacao).toLocaleDateString()}
                </p>
            </header>

            {/* Container que utiliza as regras do seu index.css (.markdown-content) */}
            <section className="markdown-content prose max-w-none text-gray-800 leading-relaxed text-lg">
                <ReactMarkdown remarkPlugins={[remarkGfm]}>
                    {artigo.conteudo}
                </ReactMarkdown>
            </section>

            <footer className="mt-12 pt-8 border-t border-gray-100 flex flex-wrap gap-2">
                {artigo.tags.map(tag => (
                    <span key={tag} className="bg-indigo-50 text-indigo-600 px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider">
                        #{tag}
                    </span>
                ))}
            </footer>
        </div>
    );
};