import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import notificationService from '../services/notificationService';

/**
 * Intermediate page that handles the redirect from the backend after a successful
 * external OAuth2 login (Google or GitHub).
 *
 * The backend redirects here with `?token=<jwt>&username=<name>` as query parameters.
 * This component reads those values, writes them into the auth context, and
 * redirects the user to the home page.
 *
 * @returns {null} Renders nothing — purely a redirect handler.
 */
const OAuthCallback: React.FC = () => {
  const { login } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const token = params.get('token');
    const username = params.get('username');

    if (token && username) {
      login(token, username);
      notificationService.success(`Welcome, ${username}!`);
      navigate('/', { replace: true });
    } else {
      notificationService.error('External login failed. Please try again.');
      navigate('/login', { replace: true });
    }
  // The effect runs once on mount — dependencies intentionally omitted.
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return null;
};

export default OAuthCallback;
