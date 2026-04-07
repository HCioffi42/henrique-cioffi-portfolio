import React from 'react';
import { useAuth } from '../context/AuthContext';

interface PermissionGateProps {
  children: React.ReactNode;
  requiredRole?: 'Admin' | 'Reader';
  ownerId?: string; // Optional: If provided, the gate will also check if the current user is the owner.
  fallback?: React.ReactNode;
}

/**
 * A component that conditionally renders its children based on the user's role and/or ownership.
 * 
 * @param {PermissionGateProps} props Component props.
 * @returns {JSX.Element | null} The children if permissions are met, otherwise the fallback or null.
 */
export const PermissionGate: React.FC<PermissionGateProps> = ({ 
  children, 
  requiredRole, 
  ownerId, 
  fallback = null 
}) => {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated || !user) {
    return <>{fallback}</>;
  }

  // 1. Admin bypass: Admins can see everything.
  if (user.role === 'Admin') {
    return <>{children}</>;
  }

  // 2. Role Check: If a specific role is required (other than Admin, handled above).
  if (requiredRole && user.role !== requiredRole) {
    return <>{fallback}</>;
  }

  // 3. Ownership Check: If an ownerId is provided, the user must match it.
  if (ownerId && user.id !== ownerId) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
};
