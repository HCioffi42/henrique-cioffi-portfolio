import { type ReactNode } from 'react';
import { useDarkMode } from '../hooks/useDarkMode';
import { ThemeContext } from './ThemeContext';

/**
 * Provider component that manages the global theme state.
 * It uses the useDarkMode hook to handle persistence and class toggling.
 */
export const ThemeProvider = ({ children }: { children: ReactNode }) => {
    const { theme, toggleTheme } = useDarkMode();

    return (
        <ThemeContext.Provider value={{ theme, toggleTheme }}>
            {children}
        </ThemeContext.Provider>
    );
};
