import type {ReactNode} from 'react';
import { Link } from 'react-router-dom';

interface LayoutProps {
    children: ReactNode;
}

/**
 * Provides a persistent header and container for all pages in the application.
 * It uses the Link component from react-router-dom to handle internal navigation.
 */
export const Layout = ({ children }: LayoutProps) => {
    return (
        <div className="min-h-screen bg-gray-50 flex flex-col">
        <header className="bg-white border-b border-gray-200 shadow-sm sticky top-0 z-10">
        <nav className="max-w-5xl mx-auto px-6 py-4 flex justify-between items-center">
        <Link to="/" className="text-xl font-bold text-indigo-600 hover:text-indigo-500 transition-colors">
        MeuSitePessoal
        </Link>
        <div className="flex gap-6">
    <Link to="/" className="text-gray-600 hover:text-indigo-600 font-medium">Blog</Link>
        <button className="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-semibold hover:bg-indigo-700 transition-all">
        New Post
    </button>
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