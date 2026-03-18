import type { ReactNode } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface LayoutProps {
    children: ReactNode;
}

/**
 * Provides a persistent header and container for all pages in the application.
 * It manages the visibility of navigation links based on the user's authentication state.
 * * @param {LayoutProps} props The component props containing children.
 * @returns {JSX.Element} The rendered layout with header, main content, and footer.
 */
export const Layout = ({ children }: LayoutProps) => {
    const { isAuthenticated, logout, user } = useAuth();
    const navigate = useNavigate();

    /**
     * Handles the logout process by clearing the session and redirecting to the home page.
     */
    const handleLogout = () => {
        logout();
        navigate('/');
    };

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col">
            <header className="bg-white border-b border-gray-200 shadow-sm sticky top-0 z-10">
                <nav className="max-w-5xl mx-auto px-6 py-4 flex justify-between items-center">
                    <Link to="/" className="text-xl font-bold text-indigo-600 hover:text-indigo-500 transition-colors">
                        MyPersonalSite
                    </Link>
                    
                    <div className="flex items-center gap-6">
                        <Link to="/" className="text-gray-600 hover:text-indigo-600 font-medium">Blog</Link>
                        
                        {isAuthenticated ? (
                            <div className="flex items-center gap-4">
                                <span className="text-sm text-gray-500 hidden sm:inline">
                                    Hello, <span className="font-semibold">{user?.username}</span>
                                </span>
                                <Link to="/admin/new-post" className="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-semibold hover:bg-indigo-700 transition-all">
                                    New Post
                                </Link>
                                <button 
                                    onClick={handleLogout}
                                    className="text-sm font-semibold text-red-600 hover:text-red-500 transition-colors">
                                    Logout
                                </button>
                            </div>
                        ) : (
                            <Link to="/login" className="text-gray-600 hover:text-indigo-600 font-medium">Login</Link>
                        )}
                    </div>
                </nav>
            </header>

            <main className="flex-grow">
                {children}
            </main>

            <footer className="bg-white border-t border-gray-200 py-8">
                <div className="max-w-5xl mx-auto px-6 text-center text-gray-400 text-sm">
                    &copy; {new Date().getFullYear()} Henrique Cioffi - Built with .NET & React
                </div>
            </footer>
        </div>
    );
};