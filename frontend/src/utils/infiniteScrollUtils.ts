
export function shouldLoadMore(
  scrollTop: number,
  clientHeight: number,
  scrollHeight: number,
  threshold: number = 10
): boolean {
  return scrollTop + clientHeight >= scrollHeight - threshold;
}

export function isCacheValid(
  timestamp: number | null,
  maxAge: number = 5 * 60 * 1000
): boolean {
  if (timestamp === null) {
    return false;
  }
  const now = Date.now();
  return now - timestamp < maxAge;
}


export function canLoadMore(
  currentPage: number,
  totalPages: number,
  isLoading: boolean
): boolean {
  if (isLoading) {
    return false;
  }
  return currentPage < totalPages;
}

export const CACHE_MAX_AGE = 5 * 60 * 1000;

export const SCROLL_THRESHOLD = 100;


export const SCROLL_DEBOUNCE_DELAY = 300;
