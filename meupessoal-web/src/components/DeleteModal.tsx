import React from 'react';
import { useTranslation } from 'react-i18next';

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
    const { t } = useTranslation();
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 px-4">
            <div className="bg-white dark:bg-slate-900 rounded-xl shadow-xl max-w-md w-full p-6 animate-in zoom-in duration-200 border border-gray-100 dark:border-slate-800">
                <h3 className="text-xl font-bold text-gray-900 dark:text-slate-100 mb-2">{t('common.confirmDeleteTitle')}</h3>
                <p className="text-gray-600 dark:text-slate-400 mb-6 leading-relaxed">
                    {t('common.confirmDeleteDesc', { title })}
                </p>
                
                <div className="flex justify-end gap-3 font-bold">
                    <button
                        onClick={onCancel}
                        className="px-4 py-2 text-sm text-gray-700 dark:text-slate-300 bg-gray-100 dark:bg-slate-800 hover:bg-gray-200 dark:hover:bg-slate-700 rounded-lg transition-colors cursor-pointer">
                        {t('common.cancel')}
                    </button>
                    <button
                        onClick={onConfirm}
                        className="px-4 py-2 text-sm text-white bg-red-600 hover:bg-red-700 rounded-lg shadow-md transition-colors cursor-pointer">
                        {t('common.delete')}
                    </button>
                </div>
            </div>
        </div>
    );
};