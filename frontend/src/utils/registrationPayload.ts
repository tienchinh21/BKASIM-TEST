/**
 * Registration Payload Utilities
 * Handles construction and validation of registration payloads
 * Requirements: 2.1
 */

/**
 * Registration payload structure
 * Contains all required fields for member registration
 */
export interface RegistrationPayload {
  UserZaloId: string;
  UserZaloName: string;
  Fullname: string;
  PhoneNumber: string;
  UserZaloIdByOA: string;
  ZaloAvatar: string;
}

/**
 * Zalo user information from SDK
 */
export interface ZaloUserInfo {
  id: string;
  name: string;
  avatar: string;
  idByOA: string;
}

/**
 * Constructs a registration payload with all required fields
 * Requirement 2.1
 *
 * @param userInfo - Zalo user information from SDK
 * @param phoneNumber - Phone number retrieved from backend API
 * @returns Registration payload object
 */
export function constructRegistrationPayload(
  userInfo: any,
  phoneNumber: string | null | undefined
): RegistrationPayload {
  console.log("constructRegistrationPayload input:", { userInfo, phoneNumber });
  
  const userZaloId = userInfo?.id || userInfo?.userId || "";
  
  // idByOA is the user ID in the Official Account (OA)
  // This is REQUIRED and must come from Zalo SDK
  const idByOA = userInfo?.idByOA || "";
  
  const payload = {
    UserZaloId: userZaloId,
    UserZaloName: userInfo?.name || "",
    Fullname: userInfo?.name || "",
    PhoneNumber: phoneNumber || "",
    UserZaloIdByOA: idByOA,
    ZaloAvatar: userInfo?.avatar || "",
  };
  
  console.log("constructRegistrationPayload output:", payload);
  return payload;
}

/**
 * Validates that a registration payload contains all required non-empty fields
 * Requirement 2.1
 *
 * @param payload - Registration payload to validate
 * @returns Object with isValid flag and error message if invalid
 */
export function validateRegistrationPayload(
  payload: RegistrationPayload
): { isValid: boolean; error?: string } {
  // Helper function to check if a field is valid (non-empty string)
  const isValidField = (value: any): boolean => {
    return typeof value === "string" && value.trim() !== "";
  };

  // Check required fields: UserZaloId, UserZaloName, Fullname, PhoneNumber, UserZaloIdByOA
  if (!isValidField(payload.UserZaloId)) {
    console.error("Validation failed: UserZaloId is empty", payload.UserZaloId);
    return { isValid: false, error: "UserZaloId is required" };
  }

  if (!isValidField(payload.UserZaloName)) {
    console.error("Validation failed: UserZaloName is empty", payload.UserZaloName);
    return { isValid: false, error: "UserZaloName is required" };
  }

  if (!isValidField(payload.Fullname)) {
    console.error("Validation failed: Fullname is empty", payload.Fullname);
    return { isValid: false, error: "Fullname is required" };
  }

  if (!isValidField(payload.PhoneNumber)) {
    console.error("Validation failed: PhoneNumber is empty", payload.PhoneNumber);
    return { isValid: false, error: "PhoneNumber is required" };
  }

  // UserZaloIdByOA is REQUIRED - must come from Zalo SDK
  if (!isValidField(payload.UserZaloIdByOA)) {
    console.error("Validation failed: UserZaloIdByOA is empty", payload.UserZaloIdByOA);
    return { isValid: false, error: "UserZaloIdByOA is required - user must follow OA" };
  }

  console.log("Validation passed: all required fields are valid");
  console.log("Payload details:", {
    UserZaloId: payload.UserZaloId,
    UserZaloName: payload.UserZaloName,
    Fullname: payload.Fullname,
    PhoneNumber: payload.PhoneNumber,
    UserZaloIdByOA: payload.UserZaloIdByOA,
    ZaloAvatar: payload.ZaloAvatar || "(empty)",
  });
  return { isValid: true };
}

/**
 * Checks if a payload has the correct structure
 * Requirement 2.1
 *
 * @param payload - Object to check
 * @returns true if payload has all required fields, false otherwise
 */
export function hasValidPayloadStructure(payload: any): boolean {
  if (!payload || typeof payload !== "object") {
    return false;
  }

  const requiredFields = [
    "UserZaloId",
    "UserZaloName",
    "Fullname",
    "PhoneNumber",
    "UserZaloIdByOA",
    "ZaloAvatar",
  ];

  return requiredFields.every((field) => field in payload);
}
