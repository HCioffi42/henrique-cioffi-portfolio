import { Sun, Moon } from 'lucide-react';
import { useTheme } from '../context/ThemeContext';

/**
 * Component that provides a toggle switch for switching between light and dark themes.
 * Precisely matches the neumorphic design with a sliding thumb and stacked text labels.
 */
export const ThemeToggle = () => {
    const { theme, toggleTheme } = useTheme();
    const isDark = theme === 'dark';

    return (
        <button
            type="button"
            role="switch"
            aria-checked={isDark}
            aria-label={isDark ? 'Switch to light mode' : 'Switch to dark mode'}
            onClick={toggleTheme}
            className={`
                relative flex items-center h-9 w-24 rounded-full transition-all duration-500 ease-in-out focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 dark:focus:ring-offset-slate-950 cursor-pointer px-1 border shadow-inner
                ${isDark ? 'bg-slate-900 border-slate-800' : 'bg-slate-100 border-slate-200'}
            `}>
            {/* Labels permanecem com as mesmas classes de opacidade */}
            <div className={`
                absolute left-3 flex flex-col items-start leading-[0.9] transition-all duration-500
                ${isDark ? 'opacity-100 translate-x-0' : 'opacity-0 -translate-x-2 pointer-events-none'}
            `}>
                <span className="text-[9px] font-black text-slate-500 dark:text-slate-400 tracking-tighter uppercase">Dark</span>
                <span className="text-[9px] font-black text-slate-500 dark:text-slate-400 tracking-tighter uppercase">Mode</span>
            </div>

            <div className={`
                absolute right-3 flex flex-col items-end leading-[0.9] transition-all duration-500
                ${isDark ? 'opacity-0 translate-x-2 pointer-events-none' : 'opacity-100 translate-x-0'}
            `}>
                <span className="text-[9px] font-black text-slate-400 dark:text-slate-500 tracking-tighter uppercase">Light</span>
                <span className="text-[9px] font-black text-slate-400 dark:text-slate-500 tracking-tighter uppercase">Mode</span>
            </div>

            <div
                className={`
                    h-7 w-7 rounded-full flex items-center justify-center transition-all duration-500 shadow-md transform -translate-y-[0.5px]
                    ${isDark 
                        ? 'translate-x-[60px] bg-white' 
                        : 'translate-x-0 bg-slate-700'}
                `}>
                {isDark ? (
                    <Moon className="w-4 h-4 text-indigo-600 fill-indigo-600/20" />
                ) : (
                    <Sun className="w-4 h-4 text-amber-400" />
                )}
            </div>
        </button>
    );
};
