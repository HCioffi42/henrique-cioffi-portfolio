import { useState, useEffect, useRef } from 'react';

interface SearchBarProps {
  onSearch: (searchTerm: string) => void;
  initialValue?: string;
}

export const SearchBar = ({ onSearch, initialValue = '' }: SearchBarProps) => {
  const [isExpanded, setIsExpanded] = useState(!!initialValue);
  const [inputValue, setInputValue] = useState(initialValue);
  // HC: Tracks the previous prop value to sync state without useEffect
  const [prevInitialValue, setPrevInitialValue] = useState(initialValue);
  const inputRef = useRef<HTMLInputElement>(null);

  /**
   * HC: Adjusts state while rendering. 
   * This is the recommended pattern to sync state with props without cascading renders.
   */
  if (initialValue !== prevInitialValue) {
    setPrevInitialValue(initialValue);
    setInputValue(initialValue);
    if (initialValue) setIsExpanded(true);
  }

  const triggerSearch = () => {
    onSearch(inputValue);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      triggerSearch();
    }
  };

  return (
    <div 
      className="relative flex items-center h-10 transition-all duration-300 ease-in-out"
      onMouseEnter={() => setIsExpanded(true)}
      onMouseLeave={() => {
        if (!inputValue && document.activeElement !== inputRef.current) {
          setIsExpanded(false);
        }
      }}
    >
      <div className={`
        flex items-center bg-gray-100 rounded-full transition-all duration-300
        ${isExpanded ? 'w-64 px-4 shadow-inner' : 'w-10 px-0 justify-center cursor-pointer'}
      `}>
        <button 
          onClick={triggerSearch}
          className="text-gray-500 hover:text-indigo-600 transition-colors"
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </button>

        <input
          ref={inputRef}
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          onKeyDown={handleKeyDown}
          onFocus={() => setIsExpanded(true)}
          placeholder="Search articles..."
          className={`
            bg-transparent border-none focus:ring-0 text-sm transition-all duration-300 placeholder-gray-400
            ${isExpanded ? 'w-full ml-2 opacity-100' : 'w-0 opacity-0'}
          `}
        />
      </div>
    </div>
  );
};