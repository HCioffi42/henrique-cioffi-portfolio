import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import { imageService } from '../services/imageService';
import { MarkdownToolbar } from '../components/MarkdownToolbar';
import { MarkdownRenderer } from '../components/MarkdownRenderer';
import { ArticleCategory, ArticleCategoryOptions } from '../models/ArticleCategory';
import notificationService from '../services/notificationService';

/**
 * Page component for editing an existing article.
 * Fetches current data by ID on mount and provides a full Markdown editor with live preview.
 */
export const EditArticle = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const contentRef = useRef<HTMLTextAreaElement>(null);
    
    // Form State
    const [formData, setFormData] = useState({
        title: '',
        summary: '',
        content: '',
        tags: '',
        category: ArticleCategory.Technology
    });
    
    // UI States
    const [isPreviewMode, setIsPreviewMode] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);

    // Track NEWLY uploaded images to cleanup if the user cancels
    const [newlyUploadedImages, setNewlyUploadedImages] = useState<string[]>([]);
    const isSaved = useRef(false);

    /**
     * Cleanup effect to delete NEWLY uploaded images if the changes are NOT saved.
     */
    useEffect(() => {
        return () => {
            if (!isSaved.current && newlyUploadedImages.length > 0) {
                newlyUploadedImages.forEach(url => {
                    imageService.deleteImage(url).catch(err => 
                        console.error(`Failed to cleanup image ${url}:`, err)
                    );
                });
            }
        };
    }, [newlyUploadedImages]);

    /**
     * Loads the existing article data to populate the form fields.
     */
    useEffect(() => {
        const fetchArticle = async () => {
            try {
                const response = await api.get(`/articles/${id}`);
                const { title, summary, content, tags, category } = response.data;
                
                setFormData({
                    title,
                    summary,
                    content,
                    tags: tags.join(', '),
                    category
                });
            } catch (err) {
                console.error('Failed to fetch article:', err);
                notificationService.error('Could not load the article data.');
            } finally {
                setIsLoading(false);
            }
        };

        if (id) fetchArticle();
    }, [id]);

    /**
     * Updates the form data state when an input value changes.
     */
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        const finalValue = name === 'category' ? Number(value) : value;
        setFormData((prev) => ({ ...prev, [name]: finalValue }));
    };

    /**
     * Handles image selection, uploads it to the backend, and inserts the 
     * markdown syntax at the exact cursor position instead of appending to the end.
     */
    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        const textarea = contentRef.current;
        if (!file || !textarea) return;

        // Captures the cursor position BEFORE the upload starts
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;

        setIsUploading(true);

        const uploadPromise = imageService.uploadImage(file);

        notificationService.promise(uploadPromise, {
            loading: 'Uploading image...',
            success: 'Image uploaded successfully!',
            error: 'Could not upload image.'
        });

        try {
            const url = await uploadPromise;
            // Stores the URL for potential cleanup later
            setNewlyUploadedImages(prev => [...prev, url]);
            
            // Encodes the URL to handle spaces and special characters.
            const encodedUrl = encodeURI(url);
            const markdownImage = `\n![${file.name}](${encodedUrl})\n`;

            // Splicing logic to insert the image where the cursor was located.
            setFormData(prev => {
                const before = prev.content.substring(0, start);
                const after = prev.content.substring(end);
                return { 
                    ...prev, 
                    content: before + markdownImage + after 
                };
            });

            // Restores focus and moves the cursor after the inserted image.
            requestAnimationFrame(() => {
                textarea.focus({ preventScroll: true });
                const newPos = start + markdownImage.length;
                textarea.setSelectionRange(newPos, newPos);
            });
            
        } catch (err: unknown) {
            console.error('Failed to upload image:', err);
        } finally {
            setIsUploading(false);
            e.target.value = '';
        }
    };

    /**
     * Submits the updated data to the PUT endpoint.
     */
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!formData.title.trim() || !formData.summary.trim() || !formData.content.trim()) {
            notificationService.error('Title, Summary, and Content are required fields.');
            return;
        }

        setIsSubmitting(true);
        try {
            const tagsArray = formData.tags
                .split(',')
                .map((tag) => tag.trim())
                .filter((tag) => tag !== '');

            const updatePromise = api.put(`/articles/${id}`, {
                id,
                title: formData.title,
                summary: formData.summary,
                content: formData.content,
                tags: tagsArray,
                category: formData.category
            });

            notificationService.promise(updatePromise, {
                loading: 'Saving changes...',
                success: 'Article updated successfully!',
                error: 'An error occurred while saving changes.'
            });

            await updatePromise;

            isSaved.current = true;
            navigate('/admin/dashboard');
        } catch (err) {
            console.error('Failed to update article:', err);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isLoading) return <div className="text-center py-20 text-gray-500 animate-pulse">Loading article data...</div>;

    return (
        <div className="max-w-4xl mx-auto px-6 py-12 animate-in fade-in duration-500 transition-colors duration-300">
            <header className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900 dark:text-slate-100 tracking-tight">Edit Post</h1>
                <p className="mt-2 text-gray-600 dark:text-slate-400">Refine your content and keep it updated for your audience.</p>
            </header>

            <form onSubmit={handleSubmit} className="space-y-6">
                <div className="bg-white dark:bg-slate-900 p-8 rounded-xl shadow-sm border border-gray-100 dark:border-slate-800 space-y-6">
                    {/* Meta Information Section */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="md:col-span-2">
                            <label htmlFor="title" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Title</label>
                            <input
                                type="text"
                                id="title"
                                name="title"
                                value={formData.title}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                            />
                        </div>

                        <div>
                            <label htmlFor="summary" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Summary</label>
                            <input
                                type="text"
                                id="summary"
                                name="summary"
                                value={formData.summary}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                            />
                        </div>

                        <div>
                            <label htmlFor="category" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Category</label>
                            <select
                                id="category"
                                name="category"
                                value={formData.category}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                            >
                                {ArticleCategoryOptions.map(option => (
                                    <option key={option.value} value={option.value} className="bg-white dark:bg-slate-900">
                                        {option.label}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    {/* Integrated Editor Area */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Content</label>
                        
                        <div className="flex flex-col border border-gray-200 dark:border-slate-700 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-indigo-500 transition-all bg-white dark:bg-slate-900">
                            {/* The toolbar now manages the Write/Preview toggle internally. */}
                            <MarkdownToolbar 
                                textareaRef={contentRef}
                                onContentChange={(newContent) => setFormData(prev => ({ ...prev, content: newContent }))}
                                onImageUpload={handleImageUpload}
                                isUploading={isUploading}
                                isPreviewMode={isPreviewMode}
                                setIsPreviewMode={setIsPreviewMode}
                            />

                            <div className="min-h-[400px] bg-white dark:bg-slate-900">
                                {!isPreviewMode ? (
                                    <textarea
                                        id="content"
                                        name="content"
                                        ref={contentRef}
                                        rows={15}
                                        value={formData.content}
                                        onChange={handleChange}
                                        className="w-full h-full p-6 outline-none resize-none font-mono text-gray-800 dark:text-slate-200 bg-white dark:bg-slate-900 leading-relaxed min-h-[400px]"
                                        placeholder="Edit your story using Markdown..."
                                    />
                                ) : (
                                    <div className="p-8 bg-gray-50/30 dark:bg-slate-950/30">
                                        {formData.content.trim() ? (
                                            <MarkdownRenderer content={formData.content} />
                                        ) : (
                                            <p className="text-gray-400 dark:text-slate-600 italic text-center mt-20">Nothing to preview yet...</p>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Tags Input Section */}
                    <div>
                        <label htmlFor="tags" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Tags</label>
                        <input
                            type="text"
                            id="tags"
                            name="tags"
                            value={formData.tags}
                            onChange={handleChange}
                            className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                            placeholder="dotnet, react, web-dev"
                        />
                    </div>
                </div>

                {/* Form Actions */}
                <div className="flex justify-end items-center gap-4 pt-4 pb-8">
                    <button
                        type="button"
                        onClick={() => navigate('/admin/dashboard')}
                        className="px-6 py-2.5 text-sm font-bold text-gray-600 dark:text-slate-400 hover:text-gray-800 dark:hover:text-slate-200 transition-all cursor-pointer">
                            Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className={`px-10 py-2.5 text-sm font-bold text-white rounded-lg transition-all shadow-md cursor-pointer ${
                            isSubmitting
                                ? 'bg-indigo-400 cursor-not-allowed'
                                : 'bg-indigo-600 hover:bg-indigo-700 hover:shadow-lg transform hover:-translate-y-0.5 active:translate-y-0'
                        }`}
                    >
                        {isSubmitting ? 'Saving...' : 'Update Post'}
                    </button>
                </div>
            </form>
        </div>
    );
};
