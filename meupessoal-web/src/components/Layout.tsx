import type { ReactNode } from 'react';
import { useCallback, useMemo } from 'react';
import { Link, useNavigate, useSearchParams, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { SearchBar } from './SearchBar';
import { ThemeToggle } from './ThemeToggle';

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

        // 1. If there is a term, navigate to the home page with the search
        if (normalizedTerm) {
            navigate(`/?q=${encodeURIComponent(normalizedTerm)}&page=1`);
        } 
        // 2. If the term is empty, we are on the home page and there was a search, we clear it
        else if (isHomePage && currentSearchParam) {
            setSearchParams(prev => {
                const next = new URLSearchParams(prev);
                next.delete('q');
                next.set('page', '1');
                return next;
            });
        }
        // 3. If we are in ArticleDetails and the search is empty, we do NOTHING.
        // This prevents navigation hijacking and allows reading the article in peace.
    }, [navigate, location.pathname, currentSearchParam, setSearchParams]);

    return (
        <div className="min-h-screen bg-gray-50 dark:bg-slate-950 text-gray-900 dark:text-slate-100 flex flex-col transition-colors duration-300">
            <header className="bg-white dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 shadow-sm sticky top-0 z-10 transition-colors duration-300">
                <nav className="max-w-5xl mx-auto px-6 py-4 flex justify-between items-center">
                    <Link to="/" className="text-xl font-bold text-indigo-600 dark:text-indigo-400 hover:text-indigo-500 transition-colors">
                        Cioffi's Projects
                    </Link>
                    
                    <div className="flex items-center gap-6">
                        <Link to="/" className="text-gray-600 dark:text-gray-300 hover:text-indigo-600 dark:hover:text-indigo-400 font-medium transition-colors">Blog</Link>

                         <SearchBar onSearch={handleGlobalSearch} initialValue={currentSearchParam} />
                        
                        <ThemeToggle />
                        
                        {isAuthenticated ? (
                            <div className="flex items-center gap-4 border-l border-gray-100 dark:border-slate-800 pl-6">
                                <span className="text-sm text-gray-500 dark:text-gray-400 hidden sm:inline">
                                    Hello, <span className="font-semibold">{user?.username}</span>
                                </span>
                                <Link to="/admin/new-post" className="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-semibold hover:bg-indigo-700 transition-all">
                                    New Post
                                </Link>
                                <button 
                                    onClick={handleLogout}
                                    className="text-sm font-semibold text-red-600 dark:text-red-400 hover:text-red-500 transition-colors cursor-pointer">
                                    Logout
                                </button>
                            </div>
                        ) : (
                            <Link to="/login" className="text-gray-600 dark:text-gray-300 hover:text-indigo-600 dark:hover:text-indigo-400 font-medium transition-colors">Admin</Link>
                        )}
                    </div>
                </nav>
            </header>

            <main className="flex-grow pb-16">
                {children}
            </main>

            <footer className="fixed bottom-0 left-0 right-0 bg-white/80 dark:bg-slate-900/80 backdrop-blur-sm border-t border-gray-200 dark:border-slate-800 py-3 transition-colors duration-300 z-10">
                <div className="max-w-5xl mx-auto px-6 text-center text-gray-400 dark:text-slate-500 text-[10px] sm:text-xs">
                    &copy; {new Date().getFullYear()} Henrique Cioffi - Built with .NET & React
                    {import.meta.env.VITE_APP_VERSION && (
                        <span className="ml-2 border-l border-gray-400 dark:border-slate-700 pl-2">
                            {import.meta.env.VITE_APP_VERSION}
                        </span>
                    )}
                </div>
            </footer>
        </div>
    );
};