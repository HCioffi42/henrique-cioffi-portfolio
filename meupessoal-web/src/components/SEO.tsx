import { Helmet } from 'react-helmet-async';
import { useTranslation } from 'react-i18next';

interface SEOProps {
    title?: string;
    description?: string;
    keywords?: string;
    image?: string;
    url?: string;
    type?: 'website' | 'article';
    articleData?: {
        publishedTime?: string;
        modifiedTime?: string;
        author?: string;
        section?: string;
        tags?: string[];
    };
}

/**
 * Reusable SEO component to manage document head metadata.
 * Handles standard meta tags, Open Graph, and Twitter Cards.
 */
export const SEO = ({ 
    title, 
    description, 
    keywords, 
    image, 
    url, 
    type = 'website',
    articleData
}: SEOProps) => {
    const { t } = useTranslation();
    const siteTitle = 'Henrique Cioffi';
    const defaultTitle = `Henrique Cioffi - ${t('home.title')}`;
    const defaultDescription = t('home.seoDescription');
    const defaultKeywords = 'software engineering, react, dotnet, technology, design, programming';
    const siteUrl = window.location.origin;
    const currentUrl = url || window.location.href;
    const defaultImage = `${siteUrl}/favicon.svg`; // Fallback image

    const seoTitle = title ? `${title} | ${siteTitle}` : defaultTitle;
    const seoDescription = description || defaultDescription;
    const seoKeywords = keywords || defaultKeywords;
    const seoImage = image || defaultImage;

    return (
        <Helmet>
            {/* Standard Meta Tags */}
            <title>{seoTitle}</title>
            <meta name="description" content={seoDescription} />
            <meta name="keywords" content={seoKeywords} />
            <link rel="canonical" href={currentUrl} />

            {/* Open Graph / Facebook */}
            <meta property="og:type" content={type} />
            <meta property="og:url" content={currentUrl} />
            <meta property="og:title" content={seoTitle} />
            <meta property="og:description" content={seoDescription} />
            <meta property="og:image" content={seoImage} />

            {/* Twitter */}
            <meta name="twitter:card" content="summary_large_image" />
            <meta name="twitter:url" content={currentUrl} />
            <meta name="twitter:title" content={seoTitle} />
            <meta name="twitter:description" content={seoDescription} />
            <meta name="twitter:image" content={seoImage} />

            {/* Article Specific Metadata */}
            {type === 'article' && articleData && (
                <>
                    {articleData.publishedTime && (
                        <meta property="article:published_time" content={articleData.publishedTime} />
                    )}
                    {articleData.modifiedTime && (
                        <meta property="article:modified_time" content={articleData.modifiedTime} />
                    )}
                    {articleData.author && (
                        <meta property="article:author" content={articleData.author} />
                    )}
                    {articleData.section && (
                        <meta property="article:section" content={articleData.section} />
                    )}
                    {articleData.tags?.map(tag => (
                        <meta key={tag} property="article:tag" content={tag} />
                    ))}
                </>
            )}
        </Helmet>
    );
};
