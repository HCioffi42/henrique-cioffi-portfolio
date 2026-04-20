import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import api from '../services/api';
import { imageService } from '../services/imageService';
import { MarkdownToolbar } from '../components/MarkdownToolbar';
import { MarkdownRenderer } from '../components/MarkdownRenderer';
import { ArticleCategory, ArticleCategoryOptions } from '../models/ArticleCategory';
import notificationService from '../services/notificationService';
import { updateArticle } from '../services/articleService';
import { getCategoryKey } from '../util/categoryMapping';

/**
 * Page component for editing an existing article with side-by-side localized inputs (EN/PT).
 * Fetches current data and allows editing both languages simultaneously.
 */
export const EditArticle = () => {
    const { t } = useTranslation();
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const contentEnRef = useRef<HTMLTextAreaElement>(null);
    const contentPtRef = useRef<HTMLTextAreaElement>(null);
    
    // Form State
    const [formData, setFormData] = useState({
        titleEn: '',
        titlePt: '',
        summaryEn: '',
        summaryPt: '',
        contentEn: '',
        contentPt: '',
        tags: '',
        category: ArticleCategory.Technology
    });
    
    // UI States
    const [isPreviewModeEn, setIsPreviewModeEn] = useState(false);
    const [isPreviewModePt, setIsPreviewModePt] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploadingEn, setIsUploadingEn] = useState(false);
    const [isUploadingPt, setIsUploadingPt] = useState(false);

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
     * HC: Refactored to explicitly use titleEn/titlePt from the backend to support side-by-side editing.
     */
    useEffect(() => {
        const fetchArticle = async () => {
            try {
                const response = await api.get(`/articles/${id}`);
                const data = response.data;
                
                // HC: Priority is given to specific localized columns. 
                // Legacy 'title' acts as a fallback for both if specific ones are missing (old articles).
                setFormData({
                    titleEn: data.titleEn || data.title || '',
                    titlePt: data.titlePt || data.title || '',
                    summaryEn: data.summaryEn || data.summary || '',
                    summaryPt: data.summaryPt || data.summary || '',
                    contentEn: data.contentEn || data.content || '',
                    contentPt: data.contentPt || data.content || '',
                    tags: data.tags.join(', '),
                    category: data.category
                });
            } catch (err) {
                console.error('Failed to fetch article:', err);
                notificationService.error(t('common.error'));
            } finally {
                setIsLoading(false);
            }
        };

        if (id) fetchArticle();
    }, [id, t]);

    /**
     * Updates the form data state when an input value changes.
     */
    const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>) => {
        const { name, value } = e.target;
        const finalValue = name === 'category' ? Number(value) : value;
        setFormData((prev) => ({ ...prev, [name]: finalValue }));
    };

    /**
     * Handles image upload and inserts Markdown syntax into the specific content field.
     */
    const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>, lang: 'En' | 'Pt') => {
        const file = e.target.files?.[0];
        const textarea = lang === 'En' ? contentEnRef.current : contentPtRef.current;
        if (!file || !textarea) return;

        const start = textarea.selectionStart;
        const end = textarea.selectionEnd;

        if (lang === 'En') setIsUploadingEn(true);
        else setIsUploadingPt(true);

        const uploadPromise = imageService.uploadImage(file);

        notificationService.promise(uploadPromise, {
            loading: t('common.loading'),
            success: t('common.success'),
            error: t('common.error')
        });

        try {
            const url = await uploadPromise;
            setNewlyUploadedImages(prev => [...prev, url]);
            
            const encodedUrl = encodeURI(url);
            const markdownImage = `\n![${file.name}](${encodedUrl})\n`;

            const fieldName = `content${lang}` as 'contentEn' | 'contentPt';

            setFormData(prev => {
                const before = prev[fieldName].substring(0, start);
                const after = prev[fieldName].substring(end);
                return { 
                    ...prev, 
                    [fieldName]: before + markdownImage + after 
                };
            });

            requestAnimationFrame(() => {
                textarea.focus({ preventScroll: true });
                const newPos = start + markdownImage.length;
                textarea.setSelectionRange(newPos, newPos);
            });
            
        } catch (err: unknown) {
            console.error('Failed to upload image:', err);
        } finally {
            if (lang === 'En') setIsUploadingEn(false);
            else setIsUploadingPt(false);
            e.target.value = '';
        }
    };

    /**
     * Submits the updated data to the PUT endpoint.
     */
    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const { titleEn, titlePt, summaryEn, summaryPt, contentEn, contentPt } = formData;
        if (!titleEn.trim() || !titlePt.trim() || !summaryEn.trim() || !summaryPt.trim() || !contentEn.trim() || !contentPt.trim()) {
            notificationService.error(t('dashboard.validationError'));
            return;
        }

        setIsSubmitting(true);
        try {
            const tagsArray = formData.tags
                .split(',')
                .map((tag) => tag.trim())
                .filter((tag) => tag !== '');

            if (!id) return;

            const updatePromise = updateArticle(id, {
                id,
                ...formData,
                tags: tagsArray
            });

            notificationService.promise(updatePromise, {
                loading: t('common.loading'),
                success: t('article.updated'),
                error: t('common.error')
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

    const RequiredBadge = () => (
        <span className="text-red-500 ml-1" title={t('dashboard.required')}>*</span>
    );

    if (isLoading) return <div className="text-center py-20 text-gray-500 animate-pulse">{t('common.loading')}</div>;

    return (
        <div className="max-w-7xl mx-auto px-6 py-12 animate-in fade-in duration-500 transition-colors duration-300">
            <header className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900 dark:text-slate-100 tracking-tight">
                    {t('dashboard.editArticle')}
                </h1>
                <p className="mt-2 text-gray-600 dark:text-slate-400">
                    {t('nav.dashboard')} - Side-by-side localization
                </p>
            </header>

            <form onSubmit={handleSubmit} className="space-y-6">
                <div className="bg-white dark:bg-slate-900 p-8 rounded-xl shadow-sm border border-gray-100 dark:border-slate-800 space-y-8">
                    
                    {/* Titles Section */}
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <div>
                            <label htmlFor="titleEn" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">
                                {t('dashboard.titleEn')} <RequiredBadge />
                            </label>
                            <input
                                type="text"
                                id="titleEn"
                                name="titleEn"
                                value={formData.titleEn}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                                placeholder={t('dashboard.placeholderTitleEn')}
                            />
                        </div>
                        <div>
                            <label htmlFor="titlePt" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">
                                {t('dashboard.titlePt')} <RequiredBadge />
                            </label>
                            <input
                                type="text"
                                id="titlePt"
                                name="titlePt"
                                value={formData.titlePt}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                                placeholder={t('dashboard.placeholderTitlePt')}
                            />
                        </div>
                    </div>

                    {/* Summaries Section */}
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                        <div>
                            <label htmlFor="summaryEn" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">
                                {t('dashboard.summaryEn')} <RequiredBadge />
                            </label>
                            <input
                                type="text"
                                id="summaryEn"
                                name="summaryEn"
                                value={formData.summaryEn}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                                placeholder={t('dashboard.placeholderSummaryEn')}
                            />
                        </div>
                        <div>
                            <label htmlFor="summaryPt" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">
                                {t('dashboard.summaryPt')} <RequiredBadge />
                            </label>
                            <input
                                type="text"
                                id="summaryPt"
                                name="summaryPt"
                                value={formData.summaryPt}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                                placeholder={t('dashboard.placeholderSummaryPt')}
                            />
                        </div>
                    </div>

                    {/* Metadata Section (Category & Tags) */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div>
                            <label htmlFor="category" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">
                                {t('dashboard.category')}
                            </label>
                            <select
                                id="category"
                                name="category"
                                value={formData.category}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                            >
                                {ArticleCategoryOptions.map(option => (
                                    <option key={option.value} value={option.value} className="bg-white dark:bg-slate-900">
                                        {t(`categories.${getCategoryKey(option.value)}`)}
                                    </option>
                                ))}
                            </select>
                        </div>
                        <div>
                            <label htmlFor="tags" className="block text-sm font-bold text-gray-700 dark:text-slate-300 mb-2">
                                {t('dashboard.tags')}
                            </label>
                            <input
                                type="text"
                                id="tags"
                                name="tags"
                                value={formData.tags}
                                onChange={handleChange}
                                className="w-full px-4 py-3 border border-gray-200 dark:border-slate-700 rounded-lg focus:ring-2 focus:ring-indigo-500 outline-none transition-all bg-white dark:bg-slate-800 text-gray-900 dark:text-slate-100"
                                placeholder={t('dashboard.placeholderTags')}
                            />
                        </div>
                    </div>

                    {/* Content Section (Editors) */}
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                        {/* English Content */}
                        <div className="space-y-2">
                            <label className="block text-sm font-bold text-gray-700 dark:text-slate-300">
                                {t('dashboard.contentEn')} <RequiredBadge />
                            </label>
                            <div className="flex flex-col border border-gray-200 dark:border-slate-700 rounded-lg overflow-hidden transition-all bg-white dark:bg-slate-900">
                                <MarkdownToolbar 
                                    textareaRef={contentEnRef}
                                    onContentChange={(newContent) => setFormData(prev => ({ ...prev, contentEn: newContent }))}
                                    onImageUpload={(e) => handleImageUpload(e, 'En')}
                                    isUploading={isUploadingEn}
                                    isPreviewMode={isPreviewModeEn}
                                    setIsPreviewMode={setIsPreviewModeEn}
                                />
                                <div className="min-h-[400px] bg-white dark:bg-slate-900">
                                    {!isPreviewModeEn ? (
                                        <textarea
                                            id="contentEn"
                                            name="contentEn"
                                            ref={contentEnRef}
                                            rows={15}
                                            value={formData.contentEn}
                                            onChange={handleChange}
                                            className="w-full h-full p-6 outline-none resize-none font-mono text-gray-800 dark:text-slate-200 bg-white dark:bg-slate-900 leading-relaxed min-h-[400px]"
                                            placeholder={t('editor.placeholderEn')}
                                        />
                                    ) : (
                                        <div className="p-8 bg-gray-50/30 dark:bg-slate-950/30 overflow-y-auto max-h-[500px]">
                                            {formData.contentEn.trim() ? (
                                                <MarkdownRenderer content={formData.contentEn} />
                                            ) : (
                                                <p className="text-gray-400 dark:text-slate-600 italic text-center mt-20">Nothing to preview...</p>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>

                        {/* Portuguese Content */}
                        <div className="space-y-2">
                            <label className="block text-sm font-bold text-gray-700 dark:text-slate-300">
                                {t('dashboard.contentPt')} <RequiredBadge />
                            </label>
                            <div className="flex flex-col border border-gray-200 dark:border-slate-700 rounded-lg overflow-hidden transition-all bg-white dark:bg-slate-900">
                                <MarkdownToolbar 
                                    textareaRef={contentPtRef}
                                    onContentChange={(newContent) => setFormData(prev => ({ ...prev, contentPt: newContent }))}
                                    onImageUpload={(e) => handleImageUpload(e, 'Pt')}
                                    isUploading={isUploadingPt}
                                    isPreviewMode={isPreviewModePt}
                                    setIsPreviewMode={setIsPreviewModePt}
                                />
                                <div className="min-h-[400px] bg-white dark:bg-slate-900">
                                    {!isPreviewModePt ? (
                                        <textarea
                                            id="contentPt"
                                            name="contentPt"
                                            ref={contentPtRef}
                                            rows={15}
                                            value={formData.contentPt}
                                            onChange={handleChange}
                                            className="w-full h-full p-6 outline-none resize-none font-mono text-gray-800 dark:text-slate-200 bg-white dark:bg-slate-900 leading-relaxed min-h-[400px]"
                                            placeholder={t('editor.placeholderPt')}
                                        />
                                    ) : (
                                        <div className="p-8 bg-gray-50/30 dark:bg-slate-950/30 overflow-y-auto max-h-[500px]">
                                            {formData.contentPt.trim() ? (
                                                <MarkdownRenderer content={formData.contentPt} />
                                            ) : (
                                                <p className="text-gray-400 dark:text-slate-600 italic text-center mt-20">Nada para visualizar...</p>
                                            )}
                                        </div>
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Form Actions */}
                <div className="flex justify-end items-center gap-4 pt-4 pb-8">
                    <button
                        type="button"
                        onClick={() => navigate('/admin/dashboard')}
                        className="px-6 py-2.5 text-sm font-bold text-gray-600 dark:text-slate-400 hover:text-gray-800 dark:hover:text-slate-200 transition-all cursor-pointer"
                    >
                        {t('common.cancel')}
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
                        {isSubmitting ? t('common.loading') : t('common.save')}
                    </button>
                </div>
            </form>
        </div>
    );
};
