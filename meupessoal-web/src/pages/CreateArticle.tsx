import React, { useState, useRef, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { createArticle } from '../services/articleService';
import { imageService } from '../services/imageService';
import { MarkdownToolbar } from '../components/MarkdownToolbar';
import { MarkdownRenderer } from '../components/MarkdownRenderer';
import { ArticleCategory, ArticleCategoryOptions } from '../models/ArticleCategory';
import notificationService from '../services/notificationService';
import { getCategoryKey } from '../util/categoryMapping';

/**
 * Page component for creating a new article with side-by-side localized inputs (EN/PT).
 * Provides localized forms, validation, and Markdown editors for both languages.
 */
export const CreateArticle = () => {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const contentEnRef = useRef<HTMLTextAreaElement>(null);
    const contentPtRef = useRef<HTMLTextAreaElement>(null);

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
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isUploadingEn, setIsUploadingEn] = useState(false);
    const [isUploadingPt, setIsUploadingPt] = useState(false);

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
            setUploadedImages(prev => [...prev, url]);

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
     * Validates and submits the form data.
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

            const createPromise = createArticle({
                ...formData,
                tags: tagsArray
            });

            notificationService.promise(createPromise, {
                loading: t('common.loading'),
                success: t('article.created'),
                error: t('common.error')
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

    const RequiredBadge = () => (
        <span className="text-red-500 ml-1" title={t('dashboard.required')}>*</span>
    );

    return (
        <div className="max-w-7xl mx-auto px-6 py-12 transition-colors duration-300">
            <header className="mb-8">
                <h1 className="text-3xl font-extrabold text-gray-900 dark:text-slate-100 tracking-tight">
                    {t('dashboard.createArticle')}
                </h1>
                <p className="mt-2 text-gray-600 dark:text-slate-400">
                    {t('dashboard.createSubtitle')}
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
                        {isSubmitting ? t('common.loading') : t('dashboard.publishPost')}
                    </button>
                </div>
            </form>
        </div>
    );
};
