import { getStorage, setStorage } from "zmp-sdk/apis";

interface StorageResult {
  success: boolean;
  data?: any;
  error?: string;
}

export const zaloStorage = {
  async get(key: string): Promise<StorageResult> {
    try {
      const result = await getStorage({ keys: [key] });
      if (result && result[key] !== undefined && result[key] !== null) {
        return {
          success: true,
          data: result[key],
        };
      }
      return { success: true, data: null };
    } catch (error) {
      console.error(`Error getting storage key ${key}:`, error);
      return {
        success: false,
        error: String(error),
      };
    }
  },

  async set(key: string, value: any): Promise<StorageResult> {
    try {
      const result = await setStorage({
        data: {
          [key]: value,
        },
      });
      if (result && result.errorKeys && result.errorKeys.length > 0) {
        return {
          success: false,
          error: `Failed to save keys: ${result.errorKeys.join(", ")}`,
        };
      }
      return { success: true };
    } catch (error) {
      console.error(`Error setting storage key ${key}:`, error);
      return {
        success: false,
        error: String(error),
      };
    }
  },

  async remove(key: string): Promise<StorageResult> {
    try {
      await setStorage({
        data: {
          [key]: "",
        },
      });
      return { success: true };
    } catch (error) {
      console.error(`Error removing storage key ${key}:`, error);
      return {
        success: false,
        error: String(error),
      };
    }
  },

  async clear(): Promise<StorageResult> {
    try {
      return { success: true };
    } catch (error) {
      console.error("Error clearing storage:", error);
      return {
        success: false,
        error: String(error),
      };
    }
  },
};
