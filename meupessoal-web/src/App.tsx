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
 * Main application entry point that manages routing and wraps content in a global Layout and AuthProvider.
 * It defines public routes for readers and protected routes for administrative tasks.
 */
export default function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Layout>
                    <Routes>
                        {/* Public Routes accessible to all visitors */}
                        <Route path="/" element={<ArticleList />} />
                        <Route path="/article/:id" element={<ArticleDetails />} />
                        <Route path="/login" element={<Login />} />

                        {/* Protected Routes that require a valid JWT session */}
                        <Route element={<ProtectedRoute />}>
                            {/* Central administrative panel for article management */}
                            <Route path="/admin/dashboard" element={<Dashboard />} />
                            
                            {/* Route for creating new content */}
                            <Route path="/admin/articles/new" element={<CreateArticle />} />
                            
                            {/* Dynamic route for editing existing articles by ID */}
                            <Route path="/admin/articles/edit/:id" element={<EditArticle />} />
                        </Route>
                    </Routes>
                </Layout>
            </BrowserRouter>
        </AuthProvider>
    );
}