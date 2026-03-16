import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ArtigoList } from './pages/ArtigoList';
import { ArtigoDetalhes } from './pages/ArtigoDetalhes';

/**
 * Root component that defines the application's routing structure.
 * @returns The BrowserRouter with defined routes.
 */
function App() {
  return (
      <BrowserRouter>
        <Routes>
          {/* Home Route: Shows the list of articles */}
          <Route path="/" element={<ArtigoList />} />

          {/* Detail Route: Shows a single article based on ID */}
          <Route path="/artigo/:id" element={<ArtigoDetalhes />} />
        </Routes>
      </BrowserRouter>
  );
}

export default App;