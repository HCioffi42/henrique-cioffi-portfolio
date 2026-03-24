import React, { useState, useEffect, useCallback } from 'react';
import { Link } from 'react-router-dom';
import api from '../services/api';
import { DeleteModal } from '../components/DeleteModal';
import { deleteArticle } from '../services/articleService';
import notificationService from '../services/notificationService';

// HC: Keeping interfaces inside the file is fine as long as they are not exported, preventing Fast Refresh issues.
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

const PAGE_SIZE = 10;

const Dashboard: React.FC = () => {
  const [articles, setArticles] = useState<ArticleSummary[]>([]);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedArticle, setSelectedArticle] = useState<{id: string, title: string} | null>(null);

  /**
   * Memoized function to fetch articles.
   * Wrapping this in useCallback prevents the 'cascading render' lint error
   * and allows it to be safely used as a dependency in useEffect.
   */
  const fetchArticles = useCallback(async (page: number) => {
    try {
      const response = await api.get<PagedResult>(`/articles`, {
        params: {
          pageNumber: page,
          pageSize: PAGE_SIZE
        }
      });

      setArticles(response.data.items);
      setTotalCount(response.data.totalCount);
    } catch (error) {
      console.error("Error loading articles from backend:", error);
      notificationService.error("Failed to load articles. Please refresh the page.");
    }
  }, []); // Empty dependencies as it only relies on the stable 'api' service and constants.

  // Effect now correctly lists fetchArticles as a dependency.
  useEffect(() => {
    fetchArticles(currentPage);
  }, [currentPage, fetchArticles]);

  const openDeleteModal = (id: string, title: string) => {
    setSelectedArticle({ id, title });
    setIsModalOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!selectedArticle) return;
    
    try {
      const deletePromise = deleteArticle(selectedArticle.id);

      notificationService.promise(deletePromise, {
        loading: 'Deleting article...',
        success: 'Article deleted successfully!',
        error: 'Failed to delete article. Please try again.'
      });

      await deletePromise;
      setIsModalOpen(false);
      
      // Refresh the current page to reflect changes.
      fetchArticles(currentPage);
    } catch (error) {
      console.error("Failed to delete article:", error);
    }
  };

  return (
    <div className="container mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-800">Administrative Dashboard</h1>
        <Link 
          to="/admin/new-post" 
          className="bg-black text-white px-4 py-2 rounded hover:bg-gray-800 transition shadow-sm font-medium">
          New Article
        </Link>
      </div>

      <div className="bg-white shadow-md rounded-lg overflow-hidden border border-gray-100">
        <table className="min-w-full leading-normal">
          <thead>
            <tr>
              <th className="px-5 py-3 border-b-2 border-gray-100 bg-gray-50 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                Title
              </th>
              <th className="px-5 py-3 border-b-2 border-gray-100 bg-gray-50 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                Date
              </th>
              <th className="px-5 py-3 border-b-2 border-gray-100 bg-gray-50 text-left text-xs font-bold text-gray-600 uppercase tracking-wider">
                Tags
              </th>
              <th className="px-5 py-3 border-b-2 border-gray-100 bg-gray-50 text-center text-xs font-bold text-gray-600 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody>
            {articles.length > 0 ? articles.map((article) => (
              <tr key={article.id} className="hover:bg-gray-50 transition-colors">
                <td className="px-5 py-5 border-b border-gray-100 bg-white text-sm">
                  <p className="text-gray-900 whitespace-no-wrap font-medium">{article.title}</p>
                </td>
                <td className="px-5 py-5 border-b border-gray-100 bg-white text-sm">
                  <p className="text-gray-900 whitespace-no-wrap">
                    {new Date(article.createdAt).toLocaleDateString()}
                  </p>
                </td>
                <td className="px-5 py-5 border-b border-gray-100 bg-white text-sm">
                  <div className="flex flex-wrap gap-1">
                    {article.tags.map(tag => (
                      <span key={tag} className="bg-gray-100 text-gray-600 text-xs px-2 py-1 rounded border border-gray-200">
                        {tag}
                      </span>
                    ))}
                  </div>
                </td>
                <td className="px-5 py-5 border-b border-gray-100 bg-white text-sm text-center">
                  <div className="flex justify-center space-x-4">
                    <Link to={`/article/${article.id}`} className="text-blue-600 hover:text-blue-800 font-medium">
                      View
                    </Link>
                    <Link to={`/admin/articles/edit/${article.id}`} className="text-indigo-600 hover:text-indigo-800 font-medium">
                      Edit
                    </Link>
                    <button onClick={() => openDeleteModal(article.id, article.title)} className="text-red-600 hover:text-red-800 font-medium cursor-pointer">
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            )) : (
              <tr>
                <td colSpan={4} className="px-5 py-10 border-b border-gray-100 bg-white text-sm text-center text-gray-500 italic">
                  No articles found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Controls */}
      {totalCount > PAGE_SIZE && (
        <div className="px-5 py-5 bg-white border-t border-gray-100 flex flex-col xs:flex-row items-center xs:justify-between">
          <span className="text-xs xs:text-sm text-gray-600">
            Showing page {currentPage} of {Math.ceil(totalCount / PAGE_SIZE)}
          </span>
          <div className="inline-flex mt-2 xs:mt-0 space-x-2">
            <button 
              onClick={() => setCurrentPage(prev => Math.max(prev - 1, 1))}
              disabled={currentPage === 1}
              className="text-sm bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded disabled:opacity-50 transition shadow-sm cursor-pointer"
            >
              Previous
            </button>
            <button 
              onClick={() => setCurrentPage(prev => prev + 1)}
              disabled={currentPage * PAGE_SIZE >= totalCount}
              className="text-sm bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2 px-4 rounded disabled:opacity-50 transition shadow-sm cursor-pointer"
            >
              Next
            </button>
          </div>
        </div>
      )}

      <DeleteModal 
          isOpen={isModalOpen}
          title={selectedArticle?.title || ""}
          onConfirm={handleDeleteConfirm}
          onCancel={() => setIsModalOpen(false)}/>
    </div>
  );
};

export default Dashboard;