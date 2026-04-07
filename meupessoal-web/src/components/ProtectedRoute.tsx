import React from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
  requiredRole?: 'Admin' | 'Reader';
}

/**
 * Higher-order component that restricts access to authenticated users only.
 * It also supports role-based redirection for administrative areas.
 * 
 * @param {ProtectedRouteProps} props Props defining role requirements.
 * @returns {JSX.Element | null} The protected content, a redirect, or null during initialization.
 */
const ProtectedRoute: React.FC<ProtectedRouteProps> = ({ requiredRole }) => {
  const { isAuthenticated, isInitialized, user } = useAuth();

  // Prevents UI flicker or premature redirection while the session is being recovered.
  if (!isInitialized) {
    return null;
  }

  if (!isAuthenticated || !user) {
    // Navigates unauthenticated users back to the login page.
    return <Navigate to="/login" replace />;
  }

  // Role check: If a role is required (e.g., Admin) and the user doesn't have it.
  if (requiredRole && user.role !== requiredRole && user.role !== 'Admin') {
    // Unauthorized access attempts are sent back to the home page or a 403 page.
    return <Navigate to="/" replace />;
  }

  // Renders the child components defined in the router if the user is verified.
  return <Outlet />;
};

export default ProtectedRoute;
