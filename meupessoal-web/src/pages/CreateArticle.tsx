import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { createArticle } from '../services/articleService';
import { imageService } from '../services/imageService';
import { MarkdownToolbar } from '../components/MarkdownToolbar';
import { MarkdownRenderer } from '../components/MarkdownRenderer';

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
    });
    
    // UI States
    const [isPreviewMode, setIsPreviewMode] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);

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
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
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
        setError(null);

        try {
            const url = await imageService.uploadImage(file);
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
            setError('Could not upload image. Please try again.');
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

            await createArticle({
                title: formData.title,
                summary: formData.summary,
                content: formData.content,
                tags: tagsArray,
            });

            isPublished.current = true;
            navigate('/admin/dashboard');
        } catch (err: unknown) {
            console.error('Failed to create article:', err);
            setError('An error occurred while saving the article.');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="max-w-4xl mx-auto px-6 py-12">
            <header className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">Create New Post</h1>
                <p className="mt-2 text-gray-600">Share your thoughts and insights with the world.</p>
            </header>

            {error && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6 rounded shadow-sm animate-in fade-in slide-in-from-top-4">
                    <p className="text-sm text-red-700 font-medium">{error}</p>
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-6">
                <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-100 space-y-6">
                    {/* Meta Section */}
                    <div className="grid grid-cols-1 gap-6">
                        <div>
                            <label htmlFor="title" className="block text-sm font-bold text-gray-700 mb-2">Title</label>
                            <input
                                type="text"
                                id="title"
                                name="title"
                                value={formData.title}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all"
                                placeholder="Enter a compelling title"
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
                                className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all"
                                placeholder="A short summary for the readers"
                            />
                        </div>
                    </div>

                    {/* Integrated Editor Section */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-2">Content</label>
                        
                        <div className="flex flex-col border border-gray-200 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-indigo-500 transition-all bg-white">
                            <MarkdownToolbar 
                                textareaRef={contentRef}
                                onContentChange={(newContent) => setFormData(prev => ({ ...prev, content: newContent }))}
                                onImageUpload={handleImageUpload}
                                isUploading={isUploading}
                                isPreviewMode={isPreviewMode}
                                setIsPreviewMode={setIsPreviewMode}
                            />

                            <div className="min-h-[400px] bg-white">
                                {!isPreviewMode ? (
                                    <textarea
                                        id="content"
                                        name="content"
                                        ref={contentRef}
                                        rows={15}
                                        value={formData.content}
                                        onChange={handleChange}
                                        className="w-full h-full p-6 outline-none resize-none font-mono text-gray-800 leading-relaxed min-h-[400px]"
                                        placeholder="Write your story using Markdown..."
                                    />
                                ) : (
                                    <div className="p-8 bg-gray-50/30">
                                        {formData.content.trim() ? (
                                            <MarkdownRenderer content={formData.content} />
                                        ) : (
                                            <p className="text-gray-400 italic text-center mt-20">Nothing to preview yet...</p>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Tags Section */}
                    <div>
                        <label htmlFor="tags" className="block text-sm font-bold text-gray-700 mb-2">Tags</label>
                        <input
                            type="text"
                            id="tags"
                            name="tags"
                            value={formData.tags}
                            onChange={handleChange}
                            className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all"
                            placeholder="e.g., dotnet, react, web-dev"
                        />
                    </div>
                </div>

                {/* Form Actions */}
                <div className="flex justify-end items-center gap-4 pt-4">
                    <button
                        type="button"
                        onClick={() => navigate('/admin/dashboard')}
                        className="px-6 py-2.5 text-sm font-bold text-gray-600 hover:text-gray-800 transition-all"
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className={`px-10 py-2.5 text-sm font-bold text-white rounded-lg transition-all shadow-md ${
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
