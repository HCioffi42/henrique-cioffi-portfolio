import React from 'react';
import { Link } from 'react-router-dom';
import { X, LogOut, PlusSquare, BookText, Shield, LayoutDashboard } from 'lucide-react';
import { PermissionGate } from './PermissionGate';
import type { User } from '../models/Auth';

interface MobileMenuProps {
    isOpen: boolean;
    onClose: () => void;
    isAuthenticated: boolean;
    user: User | null;
    onLogout: () => void;
}

/**
 * Renders a full-screen slide-over menu for mobile devices.
 * Contains all navigation links and administrative actions.
 */
export const MobileMenu: React.FC<MobileMenuProps> = ({ 
    isOpen, 
    onClose, 
    isAuthenticated, 
    user, 
    onLogout 
}) => {
    return (
        <>
            {/* HC: Backdrop overlay that handles menu closure when clicked outside the content. */}
            <div 
                className={`
                    fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-40 transition-opacity duration-300
                    ${isOpen ? 'opacity-100' : 'opacity-0 pointer-events-none'}
                `}
                onClick={onClose}
            />

            {/* HC: Main sliding menu content. Uses fixed positioning and transforms for animations. */}
            <div className={`
                fixed top-0 right-0 bottom-0 w-80 max-w-[85vw] bg-white dark:bg-slate-900 z-50 shadow-2xl transition-transform duration-300 ease-in-out transform
                ${isOpen ? 'translate-x-0' : 'translate-x-full'}
            `}>
                <div className="flex flex-col h-full">
                    {/* HC: Header area of the mobile menu with the close trigger. */}
                    <div className="flex items-center justify-between p-6 border-b border-gray-100 dark:border-slate-800">
                        <span className="text-lg font-bold text-indigo-600 dark:text-indigo-400">Navigation</span>
                        <button 
                            onClick={onClose}
                            className="p-2 rounded-lg bg-gray-50 dark:bg-slate-800 text-gray-500 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors"
                            aria-label="Close menu"
                        >
                            <X className="w-6 h-6" />
                        </button>
                    </div>

                    {/* HC: List of navigation links. Navigation triggers closure. */}
                    <nav className="flex-grow p-6 space-y-4">
                        <Link 
                            to="/" 
                            onClick={onClose}
                            className="flex items-center gap-4 text-lg font-semibold text-gray-700 dark:text-gray-200 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors py-2"
                        >
                            <BookText className="w-5 h-5" />
                            Blog
                        </Link>

                        <PermissionGate requiredRole="Admin">
                            <Link 
                                to="/admin/dashboard" 
                                onClick={onClose}
                                className="flex items-center gap-4 text-lg font-semibold text-gray-700 dark:text-gray-200 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors py-2"
                            >
                                <LayoutDashboard className="w-5 h-5" />
                                Dashboard
                            </Link>
                            <Link 
                                to="/admin/new-post" 
                                onClick={onClose}
                                className="flex items-center gap-4 text-lg font-semibold text-gray-700 dark:text-gray-200 hover:text-indigo-600 dark:hover:text-indigo-400 transition-colors py-2"
                            >
                                <PlusSquare className="w-5 h-5" />
                                New Post
                            </Link>
                        </PermissionGate>
                    </nav>

                    {/* HC: Footer area for authentication status and actions. */}
                    <div className="p-6 border-t border-gray-100 dark:border-slate-800 bg-gray-50/50 dark:bg-slate-800/20">
                        {isAuthenticated ? (
                            <div className="space-y-6">
                                <div className="flex items-center gap-3">
                                    <div className="w-10 h-10 rounded-full bg-indigo-100 dark:bg-indigo-900/40 flex items-center justify-center text-indigo-600 dark:text-indigo-400 font-bold uppercase">
                                        {user?.username.charAt(0)}
                                    </div>
                                    <div className="flex flex-col">
                                        <span className="text-sm font-bold text-gray-900 dark:text-white">{user?.username}</span>
                                        <span className="text-xs text-gray-500 dark:text-gray-400">{user?.role}</span>
                                    </div>
                                </div>
                                <button 
                                    onClick={() => {
                                        onLogout();
                                        onClose();
                                    }}
                                    className="w-full flex items-center justify-center gap-2 bg-red-50 dark:bg-red-950/30 text-red-600 dark:text-red-400 px-4 py-3 rounded-xl text-sm font-bold hover:bg-red-100 dark:hover:bg-red-900/50 transition-colors"
                                >
                                    <LogOut className="w-4 h-4" />
                                    Logout Session
                                </button>
                            </div>
                        ) : (
                            <Link 
                                to="/login" 
                                onClick={onClose}
                                className="flex items-center justify-center gap-2 w-full bg-indigo-600 text-white px-4 py-3 rounded-xl text-sm font-bold hover:bg-indigo-700 transition-all shadow-md"
                            >
                                <Shield className="w-4 h-4" />
                                Login
                            </Link>
                        )}
                    </div>
                </div>
            </div>
        </>
    );
};
