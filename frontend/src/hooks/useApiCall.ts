import { useState, useCallback } from 'react';
import { toast } from 'react-toastify';

interface UseApiCallOptions<T = any> {
  onSuccess?: (data: T) => void;
  onError?: (error: any) => void;
  successMessage?: string;
  errorMessage?: string;
  showSuccessToast?: boolean;
  showErrorToast?: boolean;
}

interface UseApiCallReturn<T = any> {
  loading: boolean;
  error: Error | null;
  data: T | null;
  execute: (apiFunction: () => Promise<T>) => Promise<T | undefined>;
  reset: () => void;
}


export const useApiCall = <T = any>(
  options: UseApiCallOptions<T> = {}
): UseApiCallReturn<T> => {
  const {
    onSuccess,
    onError,
    successMessage,
    errorMessage,
    showSuccessToast = true,
    showErrorToast = true,
  } = options;

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const [data, setData] = useState<T | null>(null);

  const execute = useCallback(
    async (apiFunction: () => Promise<T>): Promise<T | undefined> => {
      try {
        setLoading(true);
        setError(null);
        
        const result = await apiFunction();
        setData(result);

        // Show success toast if enabled and message provided
        if (showSuccessToast && successMessage) {
          toast.success(successMessage);
        }

        // Call success callback if provided
        onSuccess?.(result);

        return result;
      } catch (err: any) {
        const errorObj = err instanceof Error ? err : new Error(String(err));
        setError(errorObj);

        // Extract error message from response
        const extractedErrorMessage =
          err?.response?.data?.message ||
          err?.response?.data?.error ||
          err?.message ||
          errorMessage ||
          'Có lỗi xảy ra';

        // Show error toast if enabled
        if (showErrorToast) {
          toast.error(extractedErrorMessage);
        }

        // Call error callback if provided
        onError?.(err);

        // Re-throw error for component to handle if needed
        throw err;
      } finally {
        setLoading(false);
      }
    },
    [onSuccess, onError, successMessage, errorMessage, showSuccessToast, showErrorToast]
  );

  const reset = useCallback(() => {
    setLoading(false);
    setError(null);
    setData(null);
  }, []);

  return {
    loading,
    error,
    data,
    execute,
    reset,
  };
};

export const useLoading = () => {
  const [loading, setLoading] = useState(false);

  const withLoading = useCallback(
    async <T,>(fn: () => Promise<T>): Promise<T | undefined> => {
      try {
        setLoading(true);
        return await fn();
      } catch (error) {
        throw error;
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return { loading, withLoading, setLoading };
};

