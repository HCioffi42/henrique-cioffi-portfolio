import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import type {Artigo} from '../models/Artigo';
import { getArtigoById } from '../services/artigoService';

/**
 * Page component that displays the full content of a specific article.
 * @returns The detailed article view.
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
                console.error("Error fetching article details:", error);
            } finally {
                setLoading(false);
            }
        };

        void fetchArtigo();
    }, [id]);

    if (loading) return <div className="p-8 text-center text-gray-500">Loading article...</div>;
    if (!artigo) return <div className="p-8 text-center text-red-500">Article not found.</div>;

    return (
        <div className="max-w-3xl mx-auto p-8">
        <button
            onClick={() => navigate(-1)}
    className="mb-8 text-indigo-600 hover:text-indigo-800 font-medium flex items-center gap-2"
        >
                ← Back to list
    </button>

    <h1 className="text-4xl font-bold text-gray-900 mb-4">{artigo.titulo}</h1>
        <p className="text-sm text-gray-500 mb-8">Created at: {new Date(artigo.dataCriacao).toLocaleDateString()}</p>

    <div className="prose max-w-none text-gray-800 leading-relaxed text-lg">
        {artigo.conteudo}
        </div>

        <div className="mt-12 flex gap-2">
        {artigo.tags.map(tag => (
                <span key={tag} className="bg-indigo-50 text-indigo-600 px-3 py-1 rounded-full text-sm">
#{tag}
    </span>
))}
    </div>
    </div>
);
};