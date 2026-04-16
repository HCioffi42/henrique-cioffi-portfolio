import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { HelmetProvider } from 'react-helmet-async';
import ArticleList from './pages/ArticleList';
import { ArticleDetails } from './pages/ArticleDetails';
import { CreateArticle } from './pages/CreateArticle';
import { EditArticle } from './pages/EditArticle';
import Dashboard from './pages/Dashboard';
import Login from './pages/Login';
import Register from './pages/Register';
import CheckEmail from './pages/CheckEmail';
import UnsubscribeSuccess from './pages/UnsubscribeSuccess';
import VerifyEmail from './pages/VerifyEmail';
import ConfirmNewsletter from './pages/ConfirmNewsletter';
import OAuthCallback from './pages/OAuthCallback';
import { Layout } from './components/Layout';
import { AuthProvider } from './context/AuthProvider';
import { ThemeProvider } from './context/ThemeProvider';
import ProtectedRoute from './components/ProtectedRoute';

/**
 * Main application entry point.
 * Defines all public, protected, and utility routes including registration and OAuth callbacks.
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
                                {/* 1. Public Routes */}
                                <Route path="/" element={<ArticleList />} />
                                <Route path="/article/:id" element={<ArticleDetails />} />
                                <Route path="/login" element={<Login />} />
                                <Route path="/register" element={<Register />} />
                                <Route path="/check-email" element={<CheckEmail />} />
                                <Route path="/unsubscribe-success" element={<UnsubscribeSuccess />} />
                                <Route path="/verify-email" element={<VerifyEmail />} />
                                <Route path="/newsletter/confirm" element={<ConfirmNewsletter />} />

                                {/* 2. OAuth2 Callback — receives token from backend redirect */}
                                <Route path="/oauth/callback" element={<OAuthCallback />} />

                                {/* 3. Protected Admin Routes */}
                                <Route element={<ProtectedRoute requiredRole="Admin" />}>
                                    <Route path="/admin/dashboard" element={<Dashboard />} />
                                    <Route path="/admin/new-post" element={<CreateArticle />} />
                                    <Route path="/admin/articles/edit/:id" element={<EditArticle />} />
                                </Route>

                                {/* 4. Fallback for undefined routes */}
                                <Route path="*" element={<div className="p-8 text-center">Page not found.</div>} />
                            </Routes>
                        </Layout>
                    </BrowserRouter>
                </AuthProvider>
            </ThemeProvider>
        </HelmetProvider>
    );
}
