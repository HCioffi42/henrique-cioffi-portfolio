import api from './api';

/**
 * Service for handling image-related operations.
 */
export const imageService = {
  /**
   * Uploads an image file to the server.
   * @param file The file to upload.
   * @returns The URL of the uploaded image.
   */
  uploadImage: async (file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);

    const response = await api.post<{ url: string }>('/Images/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return response.data.url;
  },

  /**
   * Deletes an uploaded image from the server.
   * @param url The relative URL of the image.
   */
  deleteImage: async (url: string): Promise<void> => {
    await api.delete('/Images', {
      params: { url },
    });
  },
};
