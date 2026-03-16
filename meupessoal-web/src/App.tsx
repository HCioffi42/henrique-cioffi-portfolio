import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ArtigoList } from './pages/ArtigoList';
import { ArtigoDetalhes } from './pages/ArtigoDetalhes';
import { Layout } from './components/Layout';

/**
 * Main application entry point that manages routing and wraps content in a global Layout.
 */
export default function App() {
    return (
        <BrowserRouter>
            <Layout>
                <Routes>
                    <Route path="/" element={<ArtigoList />} />
                    <Route path="/artigo/:id" element={<ArtigoDetalhes />} />
                </Routes>
            </Layout>
        </BrowserRouter>
    );
}