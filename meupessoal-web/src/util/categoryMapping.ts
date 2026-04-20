export enum ArticleCategory {
  Technology = 1,
  Tutorial = 2,
  Life = 3,
  News = 4,
  Opinion = 5,
  Projects = 6,
}

export const getCategoryKey = (value: number): string => {
  const mapping: Record<number, string> = {
    [ArticleCategory.Technology]: 'technology',
    [ArticleCategory.Tutorial]: 'tutorial',
    [ArticleCategory.Life]: 'life',
    [ArticleCategory.News]: 'news',
    [ArticleCategory.Opinion]: 'opinion',
    [ArticleCategory.Projects]: 'projects',
  };
  return mapping[value] || 'technology';
};
