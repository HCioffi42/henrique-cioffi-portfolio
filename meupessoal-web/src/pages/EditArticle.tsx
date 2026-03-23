import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import { imageService } from '../services/imageService';

/**
 * Page component for editing an existing article.
 * Fetches current data by ID on mount and handles updates via the API.
 */
export const EditArticle = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const contentRef = useRef<HTMLTextAreaElement>(null);
    
    // Using English keys to match the refactored backend standards.
    const [formData, setFormData] = useState({
        title: '',
        summary: '',
        content: '',
        tags: '',
    });
    
    // Track NEWLY uploaded images to cleanup if the user cancels
    const [newlyUploadedImages, setNewlyUploadedImages] = useState<string[]>([]);
    const isSaved = useRef(false);
    
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    /**
     * Cleanup effect to delete NEWLY uploaded images if the changes are NOT saved.
     */
    useEffect(() => {
        return () => {
            if (!isSaved.current && newlyUploadedImages.length > 0) {
                // Background cleanup of orphaned images
                newlyUploadedImages.forEach(url => {
                    imageService.deleteImage(url).catch(err => 
                        console.error(`Failed to cleanup image ${url}:`, err)
                    );
                });
            }
        };
    }, [newlyUploadedImages]);

    /**
     * Fetches the article data from the API when the component mounts.
     */
    useEffect(() => {
        const fetchArticle = async () => {
            try {
                // Retrieves the full article data including content for the form.
                const response = await api.get(`/articles/${id}`);
                const { title, summary, content, tags } = response.data;
                
                setFormData({
                    title,
                    summary,
                    content,
                    tags: tags.join(', '), // Converts the array back to a comma-separated string for editing.
                });
            } catch (err) {
                console.error('Failed to fetch article:', err);
                setError('Could not load the article data. Please return to the dashboard.');
            } finally {
                setIsLoading(false);
            }
        };

        if (id) fetchArticle();
    }, [id]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    /**
     * Handles image upload and inserts Markdown syntax into the content.
     */
    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        setIsUploading(true);
        setError(null);

        try {
            const url = await imageService.uploadImage(file);
            setNewlyUploadedImages(prev => [...prev, url]);
            
            const markdownImage = `\n![${file.name}](${url})\n`;

            // Insert at cursor position if possible
            const textarea = contentRef.current;
            if (textarea) {
                const start = textarea.selectionStart;
                const end = textarea.selectionEnd;
                const newContent = 
                    formData.content.substring(0, start) + 
                    markdownImage + 
                    formData.content.substring(end);
                
                setFormData((prev) => ({ ...prev, content: newContent }));
                
                // Focus back and set cursor after the inserted text (in the next tick)
                setTimeout(() => {
                    textarea.focus();
                    const newPos = start + markdownImage.length;
                    textarea.setSelectionRange(newPos, newPos);
                }, 0);
            } else {
                setFormData((prev) => ({ ...prev, content: prev.content + markdownImage }));
            }
        } catch (err: unknown) {
            console.error('Failed to upload image:', err);
            
            let message = 'Failed to upload image. Please try again.';
            
            // Check if it's an axios error and extract the server message if available
            const axios = await import('axios');
            if (axios.isAxiosError(err) && err.response?.data) {
                const serverError = typeof err.response.data === 'string' 
                    ? err.response.data 
                    : (err.response.data as { message?: string }).message || JSON.stringify(err.response.data);
                message = `Could not upload image. Error: ${serverError}`;
            }

            setError(message);
        } finally {
            setIsUploading(false);
            // Reset input
            e.target.value = '';
        }
    };

    /**
     * Submits the updated data to the PUT endpoint.
     */
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!formData.title.trim() || !formData.summary.trim() || !formData.content.trim()) {
            setError('Title, Summary, and Content are required fields.');
            return;
        }

        setIsSubmitting(true);
        try {
            const tagsArray = formData.tags
                .split(',')
                .map((tag) => tag.trim())
                .filter((tag) => tag !== '');

            // Calls the refactored PUT endpoint on the backend.
            await api.put(`/articles/${id}`, {
                id, // The backend UpdateArticleCommand requires the ID in the body.
                title: formData.title,
                summary: formData.summary,
                content: formData.content,
                tags: tagsArray,
            });

            isSaved.current = true;
            navigate('/admin/dashboard'); // Redirects back to the management panel.
        } catch (err) {
            console.error('Failed to update article:', err);
            setError('An error occurred while saving changes.');
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isLoading) return <div className="text-center py-20">Loading article data...</div>;

    return (
        <div className="max-w-3xl mx-auto px-6 py-12">
            <div className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900">Edit Post</h1>
                <p className="mt-2 text-gray-600">Refine your content and keep it updated.</p>
            </div>

            {error && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6 rounded shadow-sm">
                    <p className="text-sm text-red-700 font-medium">{error}</p>
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-6 bg-white p-8 rounded-xl shadow-sm border border-gray-100">
                <div>
                    <label htmlFor="title" className="block text-sm font-bold text-gray-700 mb-2">Title</label>
                    <input
                        type="text"
                        id="title"
                        name="title"
                        value={formData.title}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none"
                    />
                </div>

                <div>
                    <label htmlFor="summary" className="block text-sm font-bold text-gray-700 mb-2">Summary</label>
                    <input
                        type="text"
                        id="summary"
                        name="summary"
                        value={formData.summary}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none"
                    />
                </div>

                <div>
                    <div className="flex justify-between items-center mb-2">
                        <label htmlFor="content" className="block text-sm font-bold text-gray-700">
                            Content (Markdown supported)
                        </label>
                        <div className="relative">
                            <input
                                type="file"
                                id="image-upload"
                                className="hidden"
                                accept="image/*"
                                onChange={handleImageUpload}
                                disabled={isUploading}
                            />
                            <button
                                type="button"
                                onClick={() => document.getElementById('image-upload')?.click()}
                                disabled={isUploading}
                                className={`flex items-center gap-2 px-3 py-1.5 text-xs font-semibold rounded-md border transition-all ${
                                    isUploading
                                        ? 'bg-gray-100 text-gray-400 border-gray-200 cursor-not-allowed'
                                        : 'bg-white text-indigo-600 border-indigo-200 hover:bg-indigo-50 hover:border-indigo-300'
                                }`}
                            >
                                {isUploading ? (
                                    <>
                                        <span className="animate-spin h-3 w-3 border-2 border-indigo-600 border-t-transparent rounded-full"></span>
                                        Uploading...
                                    </>
                                ) : (
                                    <>
                                        <span className="text-sm">📷</span>
                                        Upload Image
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                    <textarea
                        id="content"
                        name="content"
                        ref={contentRef}
                        rows={10}
                        value={formData.content}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none resize-none"
                    />
                </div>

                <div>
                    <label htmlFor="tags" className="block text-sm font-bold text-gray-700 mb-2">Tags</label>
                    <input
                        type="text"
                        id="tags"
                        name="tags"
                        value={formData.tags}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none"
                        placeholder="e.g., dotnet, react (separated by commas)"
                    />
                </div>

                <div className="flex justify-end items-center gap-4 pt-4 border-t border-gray-100">
                    <button
                        type="button"
                        onClick={() => navigate('/admin/dashboard')}
                        className="px-6 py-2.5 text-sm font-semibold text-gray-600 bg-gray-50 rounded-lg"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className={`px-8 py-2.5 text-sm font-semibold text-white rounded-lg shadow-md ${
                            isSubmitting ? 'bg-indigo-400' : 'bg-indigo-600 hover:bg-indigo-700'
                        }`}
                    >
                        {isSubmitting ? 'Saving...' : 'Update Post'}
                    </button>
                </div>
            </form>
        </div>
    );
};
