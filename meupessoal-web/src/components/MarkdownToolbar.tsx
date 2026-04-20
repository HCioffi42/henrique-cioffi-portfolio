import React from 'react';
import { useTranslation } from 'react-i18next';
import { 
    Bold, 
    Italic, 
    Heading2, 
    Heading3, 
    Link as LinkIcon, 
    List, 
    Image as ImageIcon,
    Loader2,
    Eye,
    Edit3
} from 'lucide-react';

interface MarkdownToolbarProps {
    textareaRef: React.RefObject<HTMLTextAreaElement | null>;
    onContentChange: (newContent: string) => void;
    onImageUpload: (e: React.ChangeEvent<HTMLInputElement>) => void;
    isUploading: boolean;
    isPreviewMode: boolean;
    setIsPreviewMode: (value: boolean) => void;
}

/**
 * Toolbar component for the Markdown editor.
 * Provides buttons for common Markdown formatting and image upload.
 */
export const MarkdownToolbar: React.FC<MarkdownToolbarProps> = ({ 
    textareaRef, 
    onContentChange, 
    onImageUpload,
    isUploading,
    isPreviewMode,
    setIsPreviewMode
}) => {
    const { t } = useTranslation();
    
    /*
    * Inserts, wraps, or unwraps selection with Markdown markers.
    */
    const insertMarkdown = (prefix: string, suffix: string = '') => {
        const textarea = textareaRef.current;
        if (!textarea) return;

        let start = textarea.selectionStart;
        let end = textarea.selectionEnd;
        const text = textarea.value;
        let selection = text.substring(start, end);

        if (selection.length > 0 && !selection.includes('\n')) {
            while (selection.endsWith(' ')) { selection = selection.substring(0, selection.length - 1); end--; }
            while (selection.startsWith(' ')) { selection = selection.substring(1); start++; }
        }

        let newSelection = '';
        const isMultiLine = selection.includes('\n');
        
        const isWrapped = isMultiLine
            ? selection.split('\n').every(line => 
                line.trim().length === 0 || (line.trim().startsWith(prefix.trim()) && line.trim().endsWith(suffix.trim()))
              )
            : selection.startsWith(prefix) && selection.endsWith(suffix);

        if (isWrapped && selection.length > 0) {
            if (isMultiLine) {
                newSelection = selection.split('\n').map(line => {
                    if (line.trim().length === 0) return line;
                    let newLine = line.trim();
                    if (newLine.startsWith(prefix.trim())) newLine = newLine.substring(prefix.trim().length);
                    if (newLine.endsWith(suffix.trim())) newLine = newLine.substring(0, newLine.length - suffix.trim().length);
                    return newLine.trim();
                }).join('\n');
            } else {
                newSelection = selection.substring(prefix.length, selection.length - suffix.length);
            }
        } else {
            if (isMultiLine) {
                newSelection = selection.split('\n').map(line => (line.trim().length === 0 ? line : `${prefix}${line}${suffix}`)).join('\n');
            } else {
                newSelection = prefix + selection + suffix;
            }
        }

        const before = text.substring(0, start);
        const after = text.substring(end);

        const newContent = before + newSelection + after;
        onContentChange(newContent);

        requestAnimationFrame(() => {
            textarea.focus({ preventScroll: true });
            
            if (selection.length > 0) {
                textarea.setSelectionRange(start, start + newSelection.length);
            } else {
                const newPos = start + (isWrapped ? 0 : prefix.length);
                textarea.setSelectionRange(newPos, newPos);
            }
        });
    };

    const tools = [
        { icon: <Bold size={18} />, label: t('editor.bold'), onClick: () => insertMarkdown('**', '**') },
        { icon: <Italic size={18} />, label: t('editor.italic'), onClick: () => insertMarkdown('_', '_') },
        { icon: <Heading2 size={18} />, label: t('editor.heading2'), onClick: () => insertMarkdown('## ') },
        { icon: <Heading3 size={18} />, label: t('editor.heading3'), onClick: () => insertMarkdown('### ') },
        { icon: <LinkIcon size={18} />, label: t('editor.link'), onClick: () => insertMarkdown('[', '](url)') },
        { icon: <List size={18} />, label: t('editor.list'), onClick: () => insertMarkdown('- ') },
    ];

    return (
        <div className="flex flex-col sm:flex-row items-center justify-between p-2 bg-gray-50 dark:bg-slate-900 border-b border-gray-200 dark:border-slate-800 sticky top-0 z-20 w-full gap-2 sm:gap-0">
            <div className="flex bg-gray-200 dark:bg-slate-800 p-1 rounded-lg w-full sm:w-auto">
                <button
                    type="button"
                    onClick={() => setIsPreviewMode(false)}
                    className={`flex-1 sm:flex-none flex items-center justify-center gap-2 px-3 py-1.5 rounded-md text-xs font-bold transition-all cursor-pointer ${
                        !isPreviewMode 
                            ? 'bg-white dark:bg-slate-700 text-indigo-600 dark:text-indigo-400 shadow-sm' 
                            : 'text-gray-600 dark:text-slate-400 hover:text-gray-900 dark:hover:text-slate-200'
                    }`}>
                    <Edit3 size={14} /> 
					{t('editor.write')}
                </button>
                <button
                    type="button"
                    onClick={() => setIsPreviewMode(true)}
                    className={`flex-1 sm:flex-none flex items-center justify-center gap-2 px-3 py-1.5 rounded-md text-xs font-bold transition-all cursor-pointer ${
                        isPreviewMode 
                            ? 'bg-white dark:bg-slate-700 text-indigo-600 dark:text-indigo-400 shadow-sm' 
                            : 'text-gray-600 dark:text-slate-400 hover:text-gray-900 dark:hover:text-slate-200'
                    }`}>
                    <Eye size={14} /> 
					{t('editor.preview')}
                </button>
            </div>

            {!isPreviewMode && (
                <div className="flex items-center gap-1 overflow-x-auto w-full sm:w-auto justify-center sm:justify-end">
                    {tools.map((tool, index) => (
                        <button
                            key={index}
                            type="button"
                            onClick={tool.onClick}
                            className="p-2 text-gray-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-white dark:hover:bg-slate-800 rounded transition-colors cursor-pointer"
                            title={tool.label}
                        >
                            {tool.icon}
                        </button>
                    ))}
                    
                    <div className="w-px h-6 bg-gray-200 dark:bg-slate-800 mx-1 flex-shrink-0" />
                    
                    <div className="relative flex-shrink-0">
                        <input
                            type="file"
                            id={`toolbar-image-upload-${textareaRef.current?.id || 'default'}`}
                            className="hidden"
                            accept="image/*"
                            onChange={onImageUpload}
                            disabled={isUploading}
                        />
                        <button
                            type="button"
                            onClick={() => document.getElementById(`toolbar-image-upload-${textareaRef.current?.id || 'default'}`)?.click()}
                            disabled={isUploading}
                            className={`p-2 rounded transition-colors cursor-pointer ${
                                isUploading 
                                    ? 'text-gray-400 dark:text-slate-600' 
                                    : 'text-gray-600 dark:text-slate-400 hover:text-indigo-600 dark:hover:text-indigo-400 hover:bg-white dark:hover:bg-slate-800'
                            }`}
                            title={t('editor.uploadImage')}
                        >
                            {isUploading ? <Loader2 size={18} className="animate-spin" /> : <ImageIcon size={18} />}
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};
