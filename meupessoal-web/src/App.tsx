import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ArticleList } from './pages/ArticleList';
import { ArticleDetails } from './pages/ArticleDetails';
import { CreateArticle } from './pages/CreateArticle';
import { EditArticle } from './pages/EditArticle';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import { Layout } from './components/Layout';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

/**
 * HC: Main application entry point.
 * Synchronized paths with Dashboard and Layout buttons to avoid blank screens.
 */
export default function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Layout>
                    <Routes>
                        {/* 1. Public Routes */}
                        <Route path="/" element={<ArticleList />} />
                        <Route path="/tags/:tag" element={<ArticleList />} />
                        
                        {/* Matches the "Read" button from Dashboard. URL: /article/90fba8f2... */}
                        <Route path="/article/:id" element={<ArticleDetails />} />
                        
                        <Route path="/login" element={<Login />} />

                        {/* 2. Protected Admin Routes */}
                        <Route element={<ProtectedRoute />}>
                            <Route path="/admin/dashboard" element={<Dashboard />} />
                            
                            <Route path="/admin/new-post" element={<CreateArticle />} />
                            
                            {/* Matches the "Edit" button from Dashboard */}
                            <Route path="/admin/articles/edit/:id" element={<EditArticle />} />
                        </Route>

                        {/* 3. Fallback for undefined routes (Optional) */}
                        <Route path="*" element={<div className="p-8 text-center">Page not found.</div>} />
                    </Routes>
                </Layout>
            </BrowserRouter>
        </AuthProvider>
    );
}