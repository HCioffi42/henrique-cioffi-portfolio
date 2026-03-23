import React from 'react';
import { 
    Bold, 
    Italic, 
    Heading2, 
    Heading3, 
    Link as LinkIcon, 
    List, 
    Image as ImageIcon,
    Loader2
} from 'lucide-react';

interface MarkdownToolbarProps {
    textareaRef: React.RefObject<HTMLTextAreaElement | null>;
    onContentChange: (newContent: string) => void;
    onImageUpload: (e: React.ChangeEvent<HTMLInputElement>) => void;
    isUploading: boolean;
}

/**
 * Toolbar component for the Markdown editor.
 * Provides buttons for common Markdown formatting and image upload.
 */
export const MarkdownToolbar: React.FC<MarkdownToolbarProps> = ({ 
    textareaRef, 
    onContentChange, 
    onImageUpload,
    isUploading 
}) => {
    
    /*
    * Inserts or wraps selection with Markdown markers.
    * Automatically detects multi-line selections and applies 
    * formatting to each line individually to maintain valid syntax.
    */
    const insertMarkdown = (prefix: string, suffix: string = '') => {
        const textarea = textareaRef.current;
        if (!textarea) return;

        let start = textarea.selectionStart;
        let end = textarea.selectionEnd;
        const text = textarea.value;
        let selection = text.substring(start, end);

        // Extra spaces selection handling (common with double-click selection)
        if (selection.length > 0 && !selection.includes('\n')) {
            // Remove spaces on the right and left of the selection for cleaner formatting
            while (selection.endsWith(' ')) { selection = selection.substring(0, selection.length - 1); end--; }
            while (selection.startsWith(' ')) { selection = selection.substring(1); start++; }
        }

        let newSelection = '';
    
        // Multi-line detection logic: if the selection contains line breaks, it processes each line independently.
        if (selection.includes('\n')) {
            const lines = selection.split('\n');
            newSelection = lines
                .map(line => {
                    // Skips empty lines to preserve vertical spacing
                    if (line.trim().length === 0) return line;
                    
                    // Applies prefix and suffix to each line (handles Bold, Italic, Link, etc.)
                    return `${prefix}${line}${suffix}`;
                })
                .join('\n');
        } else {
            // Fallback for single line or empty cursor insertion
            newSelection = prefix + selection + suffix;
        }

        const before = text.substring(0, start);
        const after = text.substring(end);

        // Apply formatting only to the "clean" text (without leading/trailing spaces)
        const newContent = before + newSelection + after;
        onContentChange(newContent);

        // Uses requestAnimationFrame to ensure the focus returns after the state update
        requestAnimationFrame(() => {
            textarea.focus();
            
            // If text was selected, keeps the entire newly formatted block highlighted
            if (selection.length > 0) {
                textarea.setSelectionRange(start, start + newSelection.length);
            } else {
                // If cursor was empty, positions it between markers
                const newPos = start + prefix.length;
                textarea.setSelectionRange(newPos, newPos);
            }
        });
    };

    /*
    * Configuration for all toolbar items.
    * All tools now leverage the enhanced multi-line logic automatically.
    */
    const tools = [
        { 
            icon: <Bold size={18} />, 
            label: 'Bold', 
            onClick: () => insertMarkdown('**', '**') 
        },
        { 
            icon: <Italic size={18} />, 
            label: 'Italic', 
            onClick: () => insertMarkdown('_', '_') 
        },
        { 
            icon: <Heading2 size={18} />, 
            label: 'Heading 2', 
            onClick: () => insertMarkdown('## ') 
        },
        { 
            icon: <Heading3 size={18} />, 
            label: 'Heading 3', 
            onClick: () => insertMarkdown('### ') 
        },
        { 
            icon: <LinkIcon size={18} />, 
            label: 'Link', 
            onClick: () => insertMarkdown('[', '](url)') 
        },
        { 
            icon: <List size={18} />, 
            label: 'Bullet List', 
            onClick: () => insertMarkdown('- ') 
        },
    ];

    return (
        <div className="flex items-center gap-1 p-1.5 bg-gray-50 border border-gray-200 rounded-t-lg border-b-0 sticky top-0 z-10">
            {tools.map((tool, index) => (
                <button
                    key={index}
                    type="button"
                    onClick={tool.onClick}
                    className="p-2 text-gray-600 hover:text-indigo-600 hover:bg-white rounded transition-colors"
                    title={tool.label}
                >
                    {tool.icon}
                </button>
            ))}
            
            <div className="w-px h-6 bg-gray-200 mx-1" />
            
            <div className="relative">
                <input
                    type="file"
                    id="toolbar-image-upload"
                    className="hidden"
                    accept="image/*"
                    onChange={onImageUpload}
                    disabled={isUploading}
                />
                <button
                    type="button"
                    onClick={() => document.getElementById('toolbar-image-upload')?.click()}
                    disabled={isUploading}
                    className={`p-2 rounded transition-colors ${
                        isUploading 
                            ? 'text-gray-400 cursor-not-allowed' 
                            : 'text-gray-600 hover:text-indigo-600 hover:bg-white'
                    }`}
                    title="Upload Image"
                >
                    {isUploading ? <Loader2 size={18} className="animate-spin" /> : <ImageIcon size={18} />}
                </button>
            </div>
        </div>
    );
};
