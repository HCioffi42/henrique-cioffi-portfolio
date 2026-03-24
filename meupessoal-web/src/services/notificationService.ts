import toast, { ToastOptions } from 'react-hot-toast';

/**
 * A wrapper service for global notifications using react-hot-toast.
 * This ensures consistency across the application and keeps components clean.
 */
const notificationService = {
  /**
   * Displays a success toast.
   * @param message The message to display.
   * @param options Optional toast configuration.
   */
  success: (message: string, options?: ToastOptions) => {
    return toast.success(message, options);
  },

  /**
   * Displays an error toast.
   * @param message The message to display.
   * @param options Optional toast configuration.
   */
  error: (message: string, options?: ToastOptions) => {
    return toast.error(message, options);
  },

  /**
   * Displays a loading toast that must be manually dismissed.
   * @param message The message to display.
   * @param options Optional toast configuration.
   */
  loading: (message: string, options?: ToastOptions) => {
    return toast.loading(message, options);
  },

  /**
   * Tracks a promise and updates the toast based on its state.
   * @param promise The promise to track.
   * @param messages The messages for loading, success, and error states.
   * @param options Optional toast configuration.
   */
  promise: <T>(
    promise: Promise<T>,
    messages: { loading: string; success: string; error: string },
    options?: ToastOptions
  ) => {
    return toast.promise(promise, messages, options);
  },

  /**
   * Dismisses one or all toasts.
   * @param toastId Optional ID of the toast to dismiss. If omitted, all toasts are dismissed.
   */
  dismiss: (toastId?: string) => {
    toast.dismiss(toastId);
  }
};

export default notificationService;
