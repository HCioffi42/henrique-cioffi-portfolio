import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

/**
 * Higher-order component that restricts access to authenticated users only.
 * It waits for the authentication initialization before making a redirection decision.
 * * @returns {JSX.Element | null} The protected content, a redirect, or null during initialization.
 */
const ProtectedRoute: React.FC = () => {
  const { isAuthenticated, isInitialized } = useAuth();

  // Prevents UI flicker or premature redirection while the session is being recovered.
  if (!isInitialized) {
    return null;
  }

  if (!isAuthenticated) {
    // Navigates unauthenticated users back to the login page.
    return <Navigate to="/login" replace />;
  }

  // Renders the child components defined in the router if the user is verified.
  return <Outlet />;
};

export default ProtectedRoute;