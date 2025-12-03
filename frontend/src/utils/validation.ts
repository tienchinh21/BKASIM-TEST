

export type ValidationError = string | null;
export type Validator<T = any> = (value: T, fieldName?: string) => ValidationError;

export const validators = {

  required: (fieldName: string = 'Trường này'): Validator => {
    return (value: any): ValidationError => {
      if (value === null || value === undefined) {
        return `${fieldName} là bắt buộc`;
      }
      if (typeof value === 'string' && !value.trim()) {
        return `${fieldName} là bắt buộc`;
      }
      if (Array.isArray(value) && value.length === 0) {
        return `${fieldName} là bắt buộc`;
      }
      return null;
    };
  },

  
  phone: (value: string): ValidationError => {
    if (!value) return null; // Skip if empty (use required validator separately)
    
    const cleaned = value.replace(/\s/g, '');
    if (!/^[0-9]{10,11}$/.test(cleaned)) {
      return 'Số điện thoại không hợp lệ (10-11 chữ số)';
    }
    return null;
  },


  email: (value: string): ValidationError => {
    if (!value) return null; // Skip if empty (use required validator separately)
    
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
      return 'Email không hợp lệ';
    }
    return null;
  },

  minLength: (min: number, fieldName: string = 'Trường này'): Validator<string> => {
    return (value: string): ValidationError => {
      if (!value) return null; // Skip if empty
      
      if (value.length < min) {
        return `${fieldName} phải có ít nhất ${min} ký tự`;
      }
      return null;
    };
  },


  maxLength: (max: number, fieldName: string = 'Trường này'): Validator<string> => {
    return (value: string): ValidationError => {
      if (!value) return null; // Skip if empty
      
      if (value.length > max) {
        return `${fieldName} không được vượt quá ${max} ký tự`;
      }
      return null;
    };
  },


  range: (min: number, max: number, fieldName: string = 'Giá trị'): Validator<number> => {
    return (value: number): ValidationError => {
      if (value === null || value === undefined) return null; // Skip if empty
      
      if (value < min || value > max) {
        return `${fieldName} phải nằm trong khoảng ${min} - ${max}`;
      }
      return null;
    };
  },

  url: (value: string): ValidationError => {
    if (!value) return null; // Skip if empty
    
    try {
      new URL(value);
      return null;
    } catch {
      return 'URL không hợp lệ';
    }
  },

  taxCode: (value: string): ValidationError => {
    if (!value) return null; // Skip if empty
    
    const cleaned = value.replace(/\s/g, '');
    if (!/^[0-9]{10,13}$/.test(cleaned)) {
      return 'Mã số thuế không hợp lệ (10-13 chữ số)';
    }
    return null;
  },

  date: (value: string): ValidationError => {
    if (!value) return null; // Skip if empty
    
    if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) {
      return 'Định dạng ngày không hợp lệ (YYYY-MM-DD)';
    }
    
    const date = new Date(value);
    if (isNaN(date.getTime())) {
      return 'Ngày không hợp lệ';
    }
    
    return null;
  },

  pattern: (regex: RegExp, errorMessage: string): Validator<string> => {
    return (value: string): ValidationError => {
      if (!value) return null; // Skip if empty
      
      if (!regex.test(value)) {
        return errorMessage;
      }
      return null;
    };
  },

  matches: (otherValue: any, fieldName: string = 'Trường này'): Validator => {
    return (value: any): ValidationError => {
      if (value !== otherValue) {
        return `${fieldName} không khớp`;
      }
      return null;
    };
  },
};

/**
 * Validate multiple fields with rules
 * 
 * @example
 * const rules = {
 *   fullname: [validators.required('Họ tên'), validators.minLength(2, 'Họ tên')],
 *   phone: [validators.required('Số điện thoại'), validators.phone],
 *   email: [validators.email],
 * };
 * 
 * const errors = validateFields(formData, rules);
 * if (Object.keys(errors).length > 0) {
 *   setErrors(errors);
 *   return false;
 * }
 */
export const validateFields = <T extends Record<string, any>>(
  data: T,
  rules: Partial<Record<keyof T, Validator[]>>
): Partial<Record<keyof T, string>> => {
  const errors: Partial<Record<keyof T, string>> = {};

  for (const [field, fieldRules] of Object.entries(rules) as [keyof T, Validator[]][]) {
    if (!fieldRules || fieldRules.length === 0) continue;

    const value = data[field];

    // Run all validators for this field
    for (const validator of fieldRules) {
      const error = validator(value);
      if (error) {
        errors[field] = error;
        break; // Stop at first error for this field
      }
    }
  }

  return errors;
};

/**
 * Check if validation errors exist
 */
export const hasErrors = (errors: Record<string, any>): boolean => {
  return Object.keys(errors).length > 0;
};

/**
 * Get first error message from errors object
 */
export const getFirstError = (errors: Record<string, string>): string | null => {
  const keys = Object.keys(errors);
  return keys.length > 0 ? errors[keys[0]] : null;
};

export const validateField = (value: any, validatorList: Validator[]): ValidationError => {
  for (const validator of validatorList) {
    const error = validator(value);
    if (error) {
      return error;
    }
  }
  return null;
};

export const createValidator = <T = any>(
  predicate: (value: T) => boolean,
  errorMessage: string
): Validator<T> => {
  return (value: T): ValidationError => {
    if (!predicate(value)) {
      return errorMessage;
    }
    return null;
  };
};

