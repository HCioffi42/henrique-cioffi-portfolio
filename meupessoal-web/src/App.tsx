import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { HelmetProvider } from 'react-helmet-async';
import ArticleList from './pages/ArticleList';
import { ArticleDetails } from './pages/ArticleDetails';
import { CreateArticle } from './pages/CreateArticle';
import { EditArticle } from './pages/EditArticle';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import { Layout } from './components/Layout';
import { AuthProvider } from './context/AuthProvider';
import { ThemeProvider } from './context/ThemeProvider';
import ProtectedRoute from './components/ProtectedRoute';

/**
 * Main application entry point.
 * Synchronized paths with Dashboard and Layout buttons to avoid blank screens.
 */
export default function App() {
    return (
        <HelmetProvider>
            <ThemeProvider>
                <AuthProvider>
                    <Toaster position="top-right" />
                    <BrowserRouter>
                        <Layout>
                            <Routes>
                                {/* 1. Public Routes - ArticleList now handles all filtering via search params */}
                                <Route path="/" element={<ArticleList />} />
                                
                                <Route path="/article/:id" element={<ArticleDetails />} />
                                <Route path="/login" element={<Login />} />

                                {/* 2. Protected Admin Routes */}
                                <Route element={<ProtectedRoute />}>
                                    <Route path="/admin/dashboard" element={<Dashboard />} />
                                    <Route path="/admin/new-post" element={<CreateArticle />} />
                                    <Route path="/admin/articles/edit/:id" element={<EditArticle />} />
                                </Route>

                                {/* 3. Fallback for undefined routes */}
                                <Route path="*" element={<div className="p-8 text-center">Page not found.</div>} />
                            </Routes>
                        </Layout>
                    </BrowserRouter>
                </AuthProvider>
            </ThemeProvider>
        </HelmetProvider>
    );
}