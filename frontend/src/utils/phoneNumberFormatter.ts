
export const formatPhoneNumber = (phoneNumber: string): string | null => {
  if (!phoneNumber) {
    return null;
  }

  let cleaned = phoneNumber.replace(/[\s\-\(\)\.]/g, "");

  if (!cleaned) {
    return null;
  }

  if (cleaned.startsWith("84")) {
    cleaned = "0" + cleaned.slice(2);
  }

  return cleaned;
};


export const isValidPhoneNumber = (phoneNumber: string): boolean => {
  if (!phoneNumber) {
    return false;
  }

  const cleaned = phoneNumber.replace(/[\s\-\(\)\.]/g, "");

  if (!/^\d+$/.test(cleaned)) {
    return false;
  }

  if (cleaned.startsWith("0")) {
    return cleaned.length === 10;
  }

  if (cleaned.startsWith("84")) {
    return cleaned.length === 11;
  }

  return false;
};


export const validatePhoneNumber = (phoneNumber: string): string | null => {
  if (!phoneNumber) {
    return "Số điện thoại là bắt buộc";
  }

  if (!isValidPhoneNumber(phoneNumber)) {
    return "Số điện thoại không hợp lệ";
  }

  return null;
};


export const normalizePhoneNumber = (phoneNumber: string): string | null => {
  const formatted = formatPhoneNumber(phoneNumber);
  if (!formatted) {
    return null;
  }

  // Ensure it's in 0xxx format
  if (formatted.startsWith("84")) {
    return "0" + formatted.slice(2);
  }

  return formatted;
};
