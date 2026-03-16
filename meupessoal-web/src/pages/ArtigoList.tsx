import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import type { Artigo } from '../models/Artigo';
import { getArtigos } from '../services/artigoService';

/**
 * Component responsible for fetching and displaying the list of blog articles.
 * @returns A structured list of articles with navigation to details.
 */
export const ArtigoList = () => {
    const [artigos, setArtigos] = useState<Artigo[]>([]);
    const [loading, setLoading] = useState<boolean>(true);
    const navigate = useNavigate();

    useEffect(() => {
        const carregarArtigos = async () => {
            try {
                const data = await getArtigos();
                setArtigos(data.items);
            } catch (error) {
                console.error("Error fetching articles:", error);
            } finally {
                setLoading(false);
            }
        };
        void carregarArtigos();
    }, []);

    if (loading) return <p className="p-8 text-center text-gray-500">Loading articles...</p>;

    return (
        <div className="max-w-4xl mx-auto p-8 font-sans">
        <h1 className="text-3xl font-bold text-gray-900">My Personal Blog</h1>
    <hr className="my-6 border-gray-200" />

        {artigos.length === 0 ? (
                <p className="text-gray-600">No articles found.</p>
) : (
        <div className="flex flex-col gap-6">
            {artigos.map(artigo => (
                    <article key={artigo.id} className="border border-gray-200 p-6 rounded-lg shadow-sm hover:shadow-md transition-shadow flex flex-col items-start">
                <h2 className="text-2xl font-semibold text-indigo-600 mb-2">{artigo.titulo}</h2>
                    <p className="text-gray-700 italic mb-4">{artigo.resumo}</p>

                    <div className="flex flex-wrap gap-2 mb-6">
                    {artigo.tags.map(tag => (
                            <span key={tag} className="bg-gray-100 text-gray-600 px-3 py-1 rounded-full text-xs font-medium">
                            {tag}
                            </span>
    ))}
    </div>

    {/* Navigation Button */}
    <button
        onClick={() => navigate(`/artigo/${artigo.id}`)}
    className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition-colors font-medium text-sm"
        >
        Read Full Article
    </button>
    </article>
))}
    </div>
)}
    </div>
);
};