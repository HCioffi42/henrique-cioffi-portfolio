/**
 * Shared enumeration for article categories.
 * Must match the Backend ArticleCategory enum values.
 */
export enum ArticleCategory {
    Technology = 1,
    Tutorial = 2,
    Life = 3,
    News = 4,
    Opinion = 5,
    Projects = 6
}

/**
 * Human-readable labels for each category.
 */
export const ArticleCategoryLabels: Record<ArticleCategory, string> = {
    [ArticleCategory.Technology]: 'Technology',
    [ArticleCategory.Tutorial]: 'Tutorial',
    [ArticleCategory.Life]: 'Life',
    [ArticleCategory.News]: 'News',
    [ArticleCategory.Opinion]: 'Opinion',
    [ArticleCategory.Projects]: 'Projects'
};

/**
 * Helper to get a list of options for a select dropdown.
 */
export const ArticleCategoryOptions = Object.entries(ArticleCategoryLabels)
    .filter(([key]) => !isNaN(Number(key)))
    .map(([key, label]) => ({
        value: Number(key),
        label
    }));
