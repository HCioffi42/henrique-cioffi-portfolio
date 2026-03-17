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
            className="px-4 py-2 bg-white border border-gray-300 rounded-lg 
                        text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-40 
                        disabled:cursor-not-allowed transition-all shadow-sm">
            Previous
        </button>

        <div className="text-sm font-medium text-gray-600">
        Page <span className="text-black">{currentPage}</span> of <span className="text-black">{totalPages}</span>
        </div>

        <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage >= totalPages}
            className="px-4 py-2 bg-white border border-gray-300 rounded-lg 
                        text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-40 
                        disabled:cursor-not-allowed transition-all shadow-sm">
                Next
        </button>
        </div>
    );
};

export default Pagination;