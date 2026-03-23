import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { createArticle } from '../services/articleService';
import { imageService } from '../services/imageService';

/**
 * Page component for creating a new article.
 * Provides a form with validation and handles submission to the backend API.
 */
export const CreateArticle = () => {
    const navigate = useNavigate();
    const contentRef = useRef<HTMLTextAreaElement>(null);
    const [formData, setFormData] = useState({
        title: '',
        summary: '',
        content: '',
        tags: '',
    });
    
    // Track uploaded images to cleanup if the user cancels
    const [uploadedImages, setUploadedImages] = useState<string[]>([]);
    const isPublished = useRef(false);
    
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    /**
     * Cleanup effect to delete uploaded images if the article is NOT published.
     */
    useEffect(() => {
        return () => {
            if (!isPublished.current && uploadedImages.length > 0) {
                // Background cleanup of orphaned images
                uploadedImages.forEach(url => {
                    imageService.deleteImage(url).catch(err => 
                        console.error(`Failed to cleanup image ${url}:`, err)
                    );
                });
            }
        };
    }, [uploadedImages]);

    /**
     * Updates the form data state when an input value changes.
     * @param e The change event from the input or textarea.
     */
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
            setUploadedImages(prev => [...prev, url]);
            
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
     * Validates the form and sends the data to the service.
     * Redirects to the home page upon successful creation.
     * @param e The form submit event.
     */
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        // Simple client-side validation
        if (!formData.title.trim() || !formData.summary.trim() || !formData.content.trim()) {
            setError('Title, Summary, and Content are required fields.');
            return;
        }

        setIsSubmitting(true);
        try {
            // Converts comma-separated tags string into an array of trimmed strings.
            const tagsArray = formData.tags
                .split(',')
                .map((tag) => tag.trim())
                .filter((tag) => tag !== '');

            await createArticle({
                title: formData.title,
                summary: formData.summary,
                content: formData.content,
                tags: tagsArray,
            });

            isPublished.current = true;
            // Navigate back to the article list on success.
            navigate('/admin/dashboard');
        } catch (err: unknown) {
            console.error('Failed to create article:', err);
            setError('An error occurred while saving the article. Please check your connection and try again.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="max-w-3xl mx-auto px-6 py-12">
            <div className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900">Create New Post</h1>
                <p className="mt-2 text-gray-600">Share your thoughts and insights with the world.</p>
            </div>

            {error && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6 rounded shadow-sm">
                    <div className="flex">
                        <div className="ml-3">
                            <p className="text-sm text-red-700 font-medium">{error}</p>
                        </div>
                    </div>
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-6 bg-white p-8 rounded-xl shadow-sm border border-gray-100">
                <div>
                    <label htmlFor="title" className="block text-sm font-bold text-gray-700 mb-2">
                        Title
                    </label>
                    <input
                        type="text"
                        id="title"
                        name="title"
                        value={formData.title}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none"
                        placeholder="Enter a compelling title"
                    />
                </div>

                <div>
                    <label htmlFor="summary" className="block text-sm font-bold text-gray-700 mb-2">
                        Summary
                    </label>
                    <input
                        type="text"
                        id="summary"
                        name="summary"
                        value={formData.summary}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none"
                        placeholder="A short summary for the article list"
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
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none resize-none"
                        placeholder="Write the full story here..."
                    />
                </div>

                <div>
                    <label htmlFor="tags" className="block text-sm font-bold text-gray-700 mb-2">
                        Tags
                    </label>
                    <input
                        type="text"
                        id="tags"
                        name="tags"
                        value={formData.tags}
                        onChange={handleChange}
                        className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none"
                        placeholder="e.g., dotnet, react, web-dev (separated by commas)"
                    />
                </div>

                <div className="flex justify-end items-center gap-4 pt-4 border-t border-gray-100">
                    <button
                        type="button"
                        onClick={() => navigate('/admin/dashboard')}
                        className="px-6 py-2.5 text-sm font-semibold text-gray-600 hover:text-gray-800 bg-gray-50 hover:bg-gray-100 rounded-lg transition-all"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className={`px-8 py-2.5 text-sm font-semibold text-white rounded-lg transition-all shadow-md ${
                            isSubmitting
                                ? 'bg-indigo-400 cursor-not-allowed'
                                : 'bg-indigo-600 hover:bg-indigo-700 hover:shadow-lg transform hover:-translate-y-0.5'
                        }`}
                    >
                        {isSubmitting ? 'Publishing...' : 'Publish Post'}
                    </button>
                </div>
            </form>
        </div>
    );
};
