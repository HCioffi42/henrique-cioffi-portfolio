import type { ReactNode } from 'react';
import { useCallback, useMemo } from 'react';
import { Link, useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { SearchBar } from './SearchBar';

interface LayoutProps {
    children: ReactNode;
}

export const Layout = ({ children }: LayoutProps) => {
    const { isAuthenticated, logout, user } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const [searchParams, setSearchParams] = useSearchParams();

    const currentSearchParam = useMemo(() => searchParams.get('q') || '', [searchParams]);

    const handleLogout = () => {
        logout();
        navigate('/');
    };

    const handleGlobalSearch = useCallback((searchTerm: string) => {
        const isHomePage = location.pathname === '/';
        const normalizedTerm = searchTerm.trim();

        // 1. Se tem termo, navega para a home com a busca
        if (normalizedTerm) {
            navigate(`/?q=${encodeURIComponent(normalizedTerm)}&page=1`);
        } 
        // 2. Se o termo está vazio, estamos na home e existia uma busca, limpamos
        else if (isHomePage && currentSearchParam) {
            setSearchParams(prev => {
                const next = new URLSearchParams(prev);
                next.delete('q');
                next.set('page', '1');
                return next;
            });
        }
        // 3. Se estivermos no ArticleDetails e a busca for vazia, NÃO fazemos nada.
        // Isso impede o "sequestro" da navegação e permite ler o artigo em paz.
    }, [navigate, location.pathname, currentSearchParam, setSearchParams]);

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col">
            <header className="bg-white border-b border-gray-200 shadow-sm sticky top-0 z-10">
                <nav className="max-w-5xl mx-auto px-6 py-4 flex justify-between items-center">
                    <Link to="/" className="text-xl font-bold text-indigo-600 hover:text-indigo-500 transition-colors">
                        Cioffi's Projects
                    </Link>
                    
                    <div className="flex items-center gap-6">
                        <Link to="/" className="text-gray-600 hover:text-indigo-600 font-medium">Blog</Link>

                         <SearchBar onSearch={handleGlobalSearch} initialValue={currentSearchParam} />
                        
                        {isAuthenticated ? (
                            <div className="flex items-center gap-4 border-l border-gray-100 pl-6">
                                <span className="text-sm text-gray-500 hidden sm:inline">
                                    Hello, <span className="font-semibold">{user?.username}</span>
                                </span>
                                <Link to="/admin/new-post" className="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-semibold hover:bg-indigo-700 transition-all">
                                    New Post
                                </Link>
                                <button 
                                    onClick={handleLogout}
                                    className="text-sm font-semibold text-red-600 hover:text-red-500 transition-colors cursor-pointer">
                                    Logout
                                </button>
                            </div>
                        ) : (
                            <Link to="/login" className="text-gray-600 hover:text-indigo-600 font-medium">Admin</Link>
                        )}
                    </div>
                </nav>
            </header>

            <main className="flex-grow">
                {children}
            </main>

            <footer className="bg-white border-t border-gray-200 py-6">
                <div className="max-w-5xl mx-auto px-6 text-center text-gray-400 text-sm">
                    &copy; {new Date().getFullYear()} Henrique Cioffi - Built with .NET & React
                    {import.meta.env.VITE_APP_VERSION && (
                        <span className="ml-2 border-l border-gray-400 pl-2">
                            {import.meta.env.VITE_APP_VERSION}
                        </span>
                    )}
                </div>
            </footer>
        </div>
    );
};