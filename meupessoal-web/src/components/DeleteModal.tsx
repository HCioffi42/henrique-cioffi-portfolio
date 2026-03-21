import React from 'react';

interface DeleteModalProps {
    isOpen: boolean;
    title: string;
    onConfirm: () => void;
    onCancel: () => void;
}

/**
 * Reusable modal component for confirming resource deletion.
 * Provides a warning message and action buttons for the user.
 */
export const DeleteModal: React.FC<DeleteModalProps> = ({ isOpen, title, onConfirm, onCancel }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 px-4">
            <div className="bg-white rounded-xl shadow-xl max-w-md w-full p-6 animate-in zoom-in duration-200">
                <h3 className="text-xl font-bold text-gray-900 mb-2">Confirm Deletion</h3>
                <p className="text-gray-600 mb-6">
                    Are you sure you want to delete <span className="font-semibold text-gray-800">"{title}"</span>? 
                    This action cannot be undone.
                </p>
                
                <div className="flex justify-end gap-3">
                    <button
                        onClick={onCancel}
                        className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 hover:bg-gray-200 rounded-lg transition-colors">
                        Cancel
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg shadow-md transition-colors">
                        Delete Article
                    </button>
                </div>
            </div>
        </div>
    );
};