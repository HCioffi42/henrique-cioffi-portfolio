export const ArticleCategory = {
  Technology: 1,
  Tutorial: 2,
  Life: 3,
  News: 4,
  Opinion: 5,
  Projects: 6,
} as const;

export type ArticleCategory = typeof ArticleCategory[keyof typeof ArticleCategory];

export const getCategoryKey = (value: number): string => {
  const mapping: { [key: number]: string } = {
    1: 'technology',
    2: 'tutorial',
    3: 'life',
    4: 'news',
    5: 'opinion',
    6: 'projects',
  };
  return mapping[value] || 'technology';
};