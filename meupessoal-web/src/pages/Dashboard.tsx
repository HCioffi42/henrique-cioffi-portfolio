import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../services/api';
import { DeleteModal } from '../components/DeleteModal';
import { deleteArticle } from '../services/articleService';

interface ArticleSummary {
  id: string;
  title: string;
  summary: string;
  createdAt: string;
  tags: string[];
}

interface PagedResult {
  items: ArticleSummary[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

const Dashboard: React.FC = () => {
  const [articles, setArticles] = useState<ArticleSummary[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedArticle, setSelectedArticle] = useState<{id: string, title: string} | null>(null);
  const pageSize = 10;

  const fetchArticles = async (page: number) => {
    try {
      // Performs a GET request to the articles endpoint with pagination parameters.
      const response = await api.get<PagedResult>(`/articles`, {
        params: {
          pageNumber: page,
          pageSize: pageSize
        }
      });

      // Updates the component state with the actual data returned from the backend.
      setArticles(response.data.items);
      setTotalCount(response.data.totalCount);
      
      console.log(`Página ${page} carregada do servidor.`);
    } catch (error) {
      console.error("Erro ao carregar articles do backend:", error);
    }
      // In a production application, you would want to display a user-friendly error message here.
  };

  useEffect(() => {
    fetchArticles(currentPage);
  }, [currentPage]);

  const openDeleteModal = (id: string, title: string) => {
    setSelectedArticle({ id, title });
    setIsModalOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!selectedArticle) return;
    
    try {
      await deleteArticle(selectedArticle.id);
      setIsModalOpen(false);
      
      // Refresh the current page to reflect changes.
      fetchArticles(currentPage);
    } catch (error) {
      console.error("Failed to delete article:", error);
      // In a production app, consider adding a notification here.
    }
  };

  return (
    <div className="container mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Administrative Dashboard</h1>
        <Link 
          to="/admin/articles/new" 
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 transition">
          New Article
        </Link>
      </div>

      <div className="bg-white shadow-md rounded-lg overflow-hidden">
        <table className="min-w-full leading-normal">
          <thead>
            <tr>
              <th className="px-5 py-3 border-b-2 border-gray-200 bg-gray-100 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                Title
              </th>
              <th className="px-5 py-3 border-b-2 border-gray-200 bg-gray-100 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                Date
              </th>
              <th className="px-5 py-3 border-b-2 border-gray-200 bg-gray-100 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider">
                Tags
              </th>
              <th className="px-5 py-3 border-b-2 border-gray-200 bg-gray-100 text-center text-xs font-semibold text-gray-600 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {articles.map((article) => (
              <tr key={article.id}>
                <td className="px-5 py-5 border-b border-gray-200 bg-white text-sm">
                  <p className="text-gray-900 whitespace-no-wrap font-medium">{article.title}</p>
                </td>
                <td className="px-5 py-5 border-b border-gray-200 bg-white text-sm">
                  <p className="text-gray-900 whitespace-no-wrap">
                    {new Date(article.createdAt).toLocaleDateString()}
                  </p>
                </td>
                <td className="px-5 py-5 border-b border-gray-200 bg-white text-sm">
                  <div className="flex flex-wrap gap-1">
                    {article.tags.map(tag => (
                      <span key={tag} className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded">
                        {tag}
                      </span>
                    ))}
                  </div>
                </td>
                <td className="px-5 py-5 border-b border-gray-200 bg-white text-sm text-center">
                  <div className="flex justify-center space-x-3">
                    <Link to={`/admin/articles/edit/${article.id}`} className="text-indigo-600 hover:text-indigo-900">
                      Edit Article
                    </Link>
                    <button onClick={() => openDeleteModal(article.id, article.title)} className="text-red-600 hover:text-red-900">
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Pagination Controls */}
      <div className="px-5 py-5 bg-white border-t flex flex-col xs:flex-row items-center xs:justify-between">
        <span className="text-xs xs:text-sm text-gray-900">
          Showing page {currentPage} of {Math.ceil(totalCount / pageSize)}
        </span>
        <div className="inline-flex mt-2 xs:mt-0">
          <button 
            onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
            disabled={currentPage === 1}
            className="text-sm bg-gray-300 hover:bg-gray-400 text-gray-800 font-semibold py-2 px-4 rounded-l disabled:opacity-50"
          >
            Previows
          </button>
          <button 
            onClick={() => setCurrentPage(prev => prev + 1)}
            disabled={currentPage * pageSize >= totalCount}
            className="text-sm bg-gray-300 hover:bg-gray-400 text-gray-800 font-semibold py-2 px-4 rounded-r disabled:opacity-50"
          >
            Next
          </button>
        </div>
      </div>

      {/* HC: Added the DeleteModal component. It only renders when isModalOpen is true. */}
      <DeleteModal 
          isOpen={isModalOpen}
          title={selectedArticle?.title || ""}
          onConfirm={handleDeleteConfirm}
          onCancel={() => setIsModalOpen(false)}/>
    </div>
  );
};

export default Dashboard;