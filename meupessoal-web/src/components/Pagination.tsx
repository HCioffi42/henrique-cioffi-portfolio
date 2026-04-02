interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

/**
 * Renders navigation controls for paginated lists.
 * It disables navigation buttons when boundaries (first or last page) are reached.
 */
const Pagination = ({ currentPage, totalPages, onPageChange }: PaginationProps) => {
    return (
        <div className="flex items-center justify-center space-x-6 py-8">
        <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage <= 1}
            className="px-4 py-2 bg-white dark:bg-slate-900 border border-gray-300 dark:border-slate-800 rounded-lg 
                        text-sm font-medium text-gray-700 dark:text-slate-300 hover:bg-gray-50 dark:hover:bg-slate-800 disabled:opacity-40 
                        disabled:cursor-not-allowed transition-all shadow-sm">
            Previous
        </button>

        <div className="text-sm font-medium text-gray-600 dark:text-slate-400">
        Page <span className="text-black dark:text-slate-100">{currentPage}</span> of <span className="text-black dark:text-slate-100">{totalPages}</span>
        </div>

        <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage >= totalPages}
            className="px-4 py-2 bg-white dark:bg-slate-900 border border-gray-300 dark:border-slate-800 rounded-lg 
                        text-sm font-medium text-gray-700 dark:text-slate-300 hover:bg-gray-50 dark:hover:bg-slate-800 disabled:opacity-40 
                        disabled:cursor-not-allowed transition-all shadow-sm">
                Next
        </button>
        </div>
    );
};

export default Pagination;