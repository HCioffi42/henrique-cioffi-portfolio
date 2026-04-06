import api from './api';

export interface NewsletterSubscriptionRequest {
    email: string;
}

/**
 * Handles operations related to the newsletter subscription.
 */
export const newsletterService = {
    /**
     * Subscribes a user to the newsletter using their email address.
     * @param request The subscription request containing the email.
     */
    async subscribe(request: NewsletterSubscriptionRequest): Promise<void> {
        await api.post('/newsletter/subscribe', request);
    }
};
