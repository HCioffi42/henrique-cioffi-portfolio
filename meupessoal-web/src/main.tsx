import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App'
import './index.css'
import './i18n/config'

/**
 * Renders the root React application into the DOM.
 * StrictMode is used to identify potential problems in the application.
 * React.Suspense is used to handle the loading state for i18next lazy-loaded resources.
 */
ReactDOM.createRoot(document.getElementById('root')!).render(
    <React.StrictMode>
        <React.Suspense fallback={<div className="flex items-center justify-center min-h-screen">Loading...</div>}>
            <App />
        </React.Suspense>
    </React.StrictMode>,
)
