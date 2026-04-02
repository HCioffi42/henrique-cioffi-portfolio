import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { createArticle } from '../services/articleService';
import { imageService } from '../services/imageService';
import { MarkdownToolbar } from '../components/MarkdownToolbar';
import { MarkdownRenderer } from '../components/MarkdownRenderer';
import { ArticleCategory, ArticleCategoryOptions } from '../models/ArticleCategory';
import notificationService from '../services/notificationService';

/**
 * Page component for creating a new article.
 * Provides a form with validation, a Markdown toolbar, and live preview.
 */
export const CreateArticle = () => {
    const navigate = useNavigate();
    const contentRef = useRef<HTMLTextAreaElement>(null);
    const [formData, setFormData] = useState({
        title: '',
        summary: '',
        content: '',
        tags: '',
        category: ArticleCategory.Technology
    });
    
    // UI States
    const [isPreviewMode, setIsPreviewMode] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);

    // Track uploaded images to cleanup if the user cancels
    const [uploadedImages, setUploadedImages] = useState<string[]>([]);
    const isPublished = useRef(false);

    /**
     * Cleanup effect to delete uploaded images if the article is NOT published.
     */
    useEffect(() => {
        return () => {
            if (!isPublished.current && uploadedImages.length > 0) {
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
     */
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        // Convert to number if it's the category field
        const finalValue = name === 'category' ? Number(value) : value;
        setFormData((prev) => ({ ...prev, [name]: finalValue }));
    };

    /**
     * Handles image upload and inserts Markdown syntax into the content.
     */
    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        const textarea = contentRef.current;
        if (!file || !textarea) return;

        // Captures the cursor position BEFORE the upload process starts.
        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;

        setIsUploading(true);

        const uploadPromise = imageService.uploadImage(file);

        notificationService.promise(uploadPromise, {
            loading: 'Uploading image...',
            success: 'Image uploaded successfully!',
            error: 'Could not upload image. Please try again.'
        });

        try {
            const url = await uploadPromise;
            setUploadedImages(prev => [...prev, url]);

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
            e.target.value = ''; // Resets input to allow re-uploading the same file.
        }
    };

    /**
     * Validates and submits the form data.
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

            const createPromise = createArticle({
                title: formData.title,
                summary: formData.summary,
                content: formData.content,
                tags: tagsArray,
                category: formData.category
            });

            notificationService.promise(createPromise, {
                loading: 'Publishing your post...',
                success: 'Post published successfully!',
                error: 'An error occurred while saving the article.'
            });

            await createPromise;

            isPublished.current = true;
            navigate('/admin/dashboard');
        } catch (err: unknown) {
            console.error('Failed to create article:', err);
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="max-w-4xl mx-auto px-6 py-12 transition-colors duration-300">
            <header className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900 dark:text-slate-100 tracking-tight">Create New Post</h1>
                <p className="mt-2 text-gray-600 dark:text-slate-400">Share your thoughts and insights with the world.</p>
            </header>

            <form onSubmit={handleSubmit} className="space-y-6">
                <div className="bg-white dark:bg-slate-900 p-8 rounded-xl shadow-sm border border-gray-100 dark:border-slate-800 space-y-6">
                    {/* Meta Section */}
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
                                placeholder="Enter a compelling title"
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
                                placeholder="A short summary for the readers"
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

                    {/* Integrated Editor Section */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Content</label>
                        
                        <div className="flex flex-col border border-gray-200 dark:border-slate-700 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-indigo-500 transition-all bg-white dark:bg-slate-900">
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
                                        placeholder="Write your story using Markdown..."
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

                    {/* Tags Section */}
                    <div>
                        <label htmlFor="tags" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">Tags</label>
                        <input
                            type="text"
                            id="tags"
                            name="tags"
                            value={formData.tags}
                            onChange={handleChange}
                            className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                            placeholder="e.g., dotnet, react, web-dev"
                        />
                    </div>
                </div>

                {/* Form Actions */}
                <div className="flex justify-end items-center gap-4 pt-4 pb-8">
                    <button
                        type="button"
                        onClick={() => navigate('/admin/dashboard')}
                        className="px-6 py-2.5 text-sm font-bold text-gray-600 dark:text-slate-400 hover:text-gray-800 dark:hover:text-slate-200 transition-all cursor-pointer"
                    >
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
                        {isSubmitting ? 'Publishing...' : 'Publish Post'}
                    </button>
                </div>
            </form>
        </div>
    );
};
