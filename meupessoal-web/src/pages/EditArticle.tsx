import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import { imageService } from '../services/imageService';
import { MarkdownToolbar } from '../components/MarkdownToolbar';
import { MarkdownRenderer } from '../components/MarkdownRenderer';
import { Eye, Edit3 } from 'lucide-react';

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
    });
    
    // UI States
    const [isPreviewMode, setIsPreviewMode] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploading, setIsUploading] = useState(false);
    const [error, setError] = useState<string | null>(null);

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
     * Fetches the article data from the API when the component mounts.
     */
    useEffect(() => {
        const fetchArticle = async () => {
            try {
                const response = await api.get(`/articles/${id}`);
                const { title, summary, content, tags } = response.data;
                
                setFormData({
                    title,
                    summary,
                    content,
                    tags: tags.join(', '),
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

    /**
     * Updates the form data state when an input value changes.
     */
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
        const { name, value } = e.target;
        setFormData((prev) => ({ ...prev, [name]: value }));
    };

    /**
     * Helper function to insert Markdown syntax at the cursor position.
     * Handles text selection and cursor persistence.
     */
    const insertMarkdown = (prefix: string, suffix: string = '') => {
        const textarea = contentRef.current;
        if (!textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;
        const text = formData.content;
        const selection = text.substring(start, end);

        const before = text.substring(0, start);
        const after = text.substring(end);

        const newContent = before + prefix + selection + suffix + after;
        
        setFormData(prev => ({ ...prev, content: newContent }));

        // Focus back and set selection in the next tick
        setTimeout(() => {
            textarea.focus();
            if (selection.length > 0) {
                textarea.setSelectionRange(start, start + prefix.length + selection.length + suffix.length);
            } else {
                const newPos = start + prefix.length;
                textarea.setSelectionRange(newPos, newPos);
            }
        }, 0);
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
            insertMarkdown(markdownImage);
        } catch (err: unknown) {
            console.error('Failed to upload image:', err);
            setError('Could not upload image. Please try again.');
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

            await api.put(`/articles/${id}`, {
                id,
                title: formData.title,
                summary: formData.summary,
                content: formData.content,
                tags: tagsArray,
            });

            isSaved.current = true;
            navigate('/admin/dashboard');
        } catch (err) {
            console.error('Failed to update article:', err);
            setError('An error occurred while saving changes.');
        } finally {
            setIsSubmitting(false);
        }
    };

    if (isLoading) return <div className="text-center py-20 text-gray-500 animate-pulse">Loading article data...</div>;

    return (
        <div className="max-w-4xl mx-auto px-6 py-12">
            <div className="mb-8 flex justify-between items-end">
                <div>
                    <h1 className="text-3xl font-extrabold text-gray-900">Edit Post</h1>
                    <p className="mt-2 text-gray-600">Refine your content and keep it updated.</p>
                </div>
                
                {/* Write/Preview Toggle */}
                <div className="flex bg-gray-100 p-1 rounded-lg border border-gray-200">
                    <button
                        type="button"
                        onClick={() => setIsPreviewMode(false)}
                        className={`flex items-center gap-2 px-4 py-1.5 text-sm font-bold rounded-md transition-all ${
                            !isPreviewMode 
                                ? 'bg-white text-indigo-600 shadow-sm' 
                                : 'text-gray-500 hover:text-gray-700'
                        }`}
                    >
                        <Edit3 size={16} />
                        Write
                    </button>
                    <button
                        type="button"
                        onClick={() => setIsPreviewMode(true)}
                        className={`flex items-center gap-2 px-4 py-1.5 text-sm font-bold rounded-md transition-all ${
                            isPreviewMode 
                                ? 'bg-white text-indigo-600 shadow-sm' 
                                : 'text-gray-500 hover:text-gray-700'
                        }`}
                    >
                        <Eye size={16} />
                        Preview
                    </button>
                </div>
            </div>

            {error && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-6 rounded shadow-sm">
                    <p className="text-sm text-red-700 font-medium">{error}</p>
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-6">
                <div className="bg-white p-8 rounded-xl shadow-sm border border-gray-100 space-y-6">
                    {/* Title & Summary */}
                    <div className="grid grid-cols-1 gap-6">
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
                                className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none"
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
                                className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none"
                            />
                        </div>
                    </div>

                    {/* Editor / Preview Area */}
                    <div>
                        <label className="block text-sm font-bold text-gray-700 mb-2">
                            Content
                        </label>
                        
                        {!isPreviewMode ? (
                            <div className="flex flex-col border border-gray-200 rounded-lg overflow-hidden focus-within:ring-2 focus-within:ring-indigo-500 focus-within:border-indigo-500 transition-all">
                                <MarkdownToolbar 
                                    textareaRef={contentRef}
                                    onContentChange={(newContent) => setFormData(prev => ({ ...prev, content: newContent }))}
                                    onImageUpload={handleImageUpload}
                                    isUploading={isUploading}
                                />
                                <textarea
                                    id="content"
                                    name="content"
                                    ref={contentRef}
                                    rows={15}
                                    value={formData.content}
                                    onChange={handleChange}
                                    className="w-full px-4 py-4 border-0 outline-none resize-none font-mono text-gray-800 leading-relaxed min-h-[400px]"
                                    placeholder="Write your story using Markdown..."
                                />
                            </div>
                        ) : (
                            <div className="w-full min-h-[464px] px-8 py-8 border border-gray-200 rounded-lg bg-gray-50/50 overflow-y-auto prose-indigo">
                                {formData.content.trim() ? (
                                    <MarkdownRenderer content={formData.content} />
                                ) : (
                                    <p className="text-gray-400 italic text-center mt-20">Nothing to preview yet...</p>
                                )}
                            </div>
                        )}
                    </div>

                    {/* Tags */}
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
                            className="w-full px-4 py-3 border border-gray-200 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-all outline-none"
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
                        {isSubmitting ? 'Saving...' : 'Update Post'}
                    </button>
                </div>
            </form>
        </div>
    );
};
