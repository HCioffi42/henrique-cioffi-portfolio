import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ArtigoList } from './pages/ArtigoList';
import { ArtigoDetalhes } from './pages/ArtigoDetalhes';
import { CreateArtigo } from './pages/CreateArtigo';
import Login from './pages/Login';
import { Layout } from './components/Layout';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

/**
 * Main application entry point that manages routing and wraps content in a global Layout and AuthProvider.
 */
export default function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Layout>
                    <Routes>
                        <Route path="/" element={<ArtigoList />} />
                        <Route path="/artigo/:id" element={<ArtigoDetalhes />} />
                        <Route path="/login" element={<Login />} />

                        {/* Protected Routes */}
                        <Route element={<ProtectedRoute />}>
                            <Route path="/admin/new-post" element={<CreateArtigo />} />
                        </Route>
                    </Routes>
                </Layout>
            </BrowserRouter>
        </AuthProvider>
    );
}