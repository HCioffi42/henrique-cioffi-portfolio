import React, { useState } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vscDarkPlus } from 'react-syntax-highlighter/dist/esm/styles/prism';
import { BACKEND_URL } from '../services/api';

/**
 * Component responsible for rendering code blocks.
 * Bypasses Tailwind's prose background interference and uses a dark IDE theme.
 */
const CodeBlock = ({ inline, className, children, ...props }: any) => {
    const match = /language-(\w+)/.exec(className || '');
    const language = match ? match[1] : '';
    // State that tracks whether the code was recently copied to provide visual feedback.
    const [isCopied, setIsCopied] = useState(false);

    /**
     * Function that extracts the raw text from the code block and
     * uses the modern Clipboard API to copy it to the user's clipboard.
     */
    const handleCopy = async () => {
        const codeText = String(children).replace(/\n$/, '');
        try {
            await navigator.clipboard.writeText(codeText);
            setIsCopied(true);
            setTimeout(() => setIsCopied(false), 2000);
        } catch (err) {
            console.error("Failed to copy text:", err);
        }
    };

    if (!inline && match) {
        return (
            <div className="my-6 rounded-lg overflow-hidden bg-[#24292e] shadow-md">
                <div className="bg-[#1b1f23] px-4 py-2 text-xs font-mono text-gray-200 capitalize tracking-wider border-b border-gray-700/50 flex justify-between items-center">
                    <span>{language}</span>
                    <button
                        type="button"
                        onClick={handleCopy}
                        className="text-gray-200 hover:text-white transition-colors focus:outline-none cursor-pointer"
                        aria-label="Copy code to clipboard"
                        title="Copy code">
                        {isCopied ? "Copied!" : "Copy"}
                    </button>
                </div>

                <SyntaxHighlighter
                    style={vscDarkPlus}
                    language={language}
                    PreTag="div"
                    customStyle={{
                        margin: 0,
                        padding: '1rem',
                        background: 'transparent',
                        fontSize: '1rem',
                        fontWeight: 'bold',
                        lineHeight: '1.4',
                        overflowX: 'auto',
                    }}
                    codeTagProps={{
                        style: {
                            backgroundColor: 'transparent',
                            borderWidth: 0,
                            padding: 0
                        }
                    }}
                    {...props}
                >
                    {String(children).replace(/\n$/, '')}
                </SyntaxHighlighter>
            </div>
        );
    }

    return (
        <code className="text-indigo-600 bg-indigo-50 px-1.5 py-0.5 rounded font-mono text-sm" {...props}>
            {children}
        </code>
    );
};

/**
 * Component responsible for rendering the Markdown content.
 * Centralizes markdown configuration, injecting the custom CodeBlock and image handling.
 */
export const MarkdownRenderer = ({ content }: { content: string }) => (
    <section className="markdown-content prose max-w-none text-gray-800 leading-relaxed text-lg">
        <ReactMarkdown
            remarkPlugins={[remarkGfm]}
            components={{ 
                code: CodeBlock, 
                pre: ({ children }) => <>{children}</>,
                img: ({ src, alt, ...props }) => {
                    // If the src is relative (starts with /), prepend the backend URL
                    const resolvedSrc = src?.startsWith('/') 
                        ? `${BACKEND_URL}${src}` 
                        : (src?.startsWith('http') 
                            ? src 
                            : `${BACKEND_URL}/${src}`);
                    return (
                        <img 
                            src={resolvedSrc} 
                            alt={alt} 
                            className="rounded-xl shadow-lg my-8 mx-auto border border-gray-100 max-h-[600px] object-contain" 
                            {...props} 
                        />
                    );
                }
            }} >
            {content}
        </ReactMarkdown>
    </section>
);
