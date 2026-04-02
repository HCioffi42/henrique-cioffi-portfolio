import { createContext, useContext, type ReactNode } from 'react';
import { useDarkMode } from '../hooks/useDarkMode';

interface ThemeContextType {
    theme: 'light' | 'dark';
    toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

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

/**
 * Custom hook to access the theme context.
 */
export const useTheme = () => {
    const context = useContext(ThemeContext);
    if (context === undefined) {
        throw new Error('useTheme must be used within a ThemeProvider');
    }
    return context;
};
