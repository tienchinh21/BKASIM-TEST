/**
 * Input Validation Utility
 * Contains functions to validate different types of input
 */

const InputValidator = {
	/**
	 * Number Validations
	 */
	number: {
		/**
		 * Checks if input is a valid number
		 * @param {string|number} value - The value to check
		 * @returns {boolean} - True if valid number, false otherwise
		 */
		isValid: function (value) {
			if (value === "" || value === null || value === undefined) return false;
			return !isNaN(parseFloat(value)) && isFinite(value);
		},

		/**
		 * Checks if input is an integer
		 * @param {string|number} value - The value to check
		 * @returns {boolean} - True if valid integer, false otherwise
		 */
		isInteger: function (value) {
			if (!this.isValid(value)) return false;
			return Number.isInteger(Number(value));
		},

		/**
		 * Checks if input is a positive number
		 * @param {string|number} value - The value to check
		 * @returns {boolean} - True if positive number, false otherwise
		 */
		isPositive: function (value) {
			if (!this.isValid(value)) return false;
			return parseFloat(value) > 0;
		},

		/**
		 * Checks if input is a negative number
		 * @param {string|number} value - The value to check
		 * @returns {boolean} - True if negative number, false otherwise
		 */
		isNegative: function (value) {
			if (!this.isValid(value)) return false;
			return parseFloat(value) < 0;
		},

		/**
		 * Checks if input is within range
		 * @param {string|number} value - The value to check
		 * @param {number} min - Minimum value (inclusive)
		 * @param {number} max - Maximum value (inclusive)
		 * @returns {boolean} - True if within range, false otherwise
		 */
		inRange: function (value, min, max) {
			if (!this.isValid(value)) return false;
			const num = parseFloat(value);
			return num >= min && num <= max;
		},

		/**
		 * Format a number as currency (VND)
		 * @param {string|number} value - The number to format
		 * @returns {string} - Formatted currency string
		 */
		formatCurrency: function (value) {
			if (!this.isValid(value)) return "";
			return parseFloat(value).toLocaleString("vi-VN", {
				style: "currency",
				currency: "VND",
			});
		},

		/**
		 * Strip non-numeric characters from input
		 * @param {string} value - The input string
		 * @returns {string} - String with only numbers
		 */
		stripNonNumeric: function (value) {
			if (typeof value !== "string") return value;
			return value.replace(/[^0-9.-]/g, "");
		},
	},

	/**
	 * Text Validations
	 */
	text: {
		/**
		 * Checks if input is not empty
		 * @param {string} value - The string to check
		 * @returns {boolean} - True if not empty, false otherwise
		 */
		isNotEmpty: function (value) {
			return typeof value === "string" && value.trim() !== "";
		},

		/**
		 * Checks if input has a minimum length
		 * @param {string} value - The string to check
		 * @param {number} minLength - Minimum required length
		 * @returns {boolean} - True if meets minimum length, false otherwise
		 */
		minLength: function (value, minLength) {
			if (!this.isNotEmpty(value)) return false;
			return value.trim().length >= minLength;
		},

		/**
		 * Checks if input doesn't exceed maximum length
		 * @param {string} value - The string to check
		 * @param {number} maxLength - Maximum allowed length
		 * @returns {boolean} - True if doesn't exceed max length, false otherwise
		 */
		maxLength: function (value, maxLength) {
			if (typeof value !== "string") return false;
			return value.length <= maxLength;
		},

		/**
		 * Checks if input length is within range
		 * @param {string} value - The string to check
		 * @param {number} minLength - Minimum required length
		 * @param {number} maxLength - Maximum allowed length
		 * @returns {boolean} - True if length is within range, false otherwise
		 */
		lengthInRange: function (value, minLength, maxLength) {
			return this.minLength(value, minLength) && this.maxLength(value, maxLength);
		},

		/**
		 * Checks if input only contains alphabetic characters
		 * @param {string} value - The string to check
		 * @returns {boolean} - True if only alphabetic, false otherwise
		 */
		isAlphabetic: function (value) {
			if (!this.isNotEmpty(value)) return false;
			return /^[A-Za-zÀ-ỹ\s]+$/.test(value); // Includes Vietnamese characters
		},

		/**
		 * Checks if input only contains alphanumeric characters
		 * @param {string} value - The string to check
		 * @returns {boolean} - True if only alphanumeric, false otherwise
		 */
		isAlphanumeric: function (value) {
			if (!this.isNotEmpty(value)) return false;
			return /^[A-Za-zÀ-ỹ0-9\s]+$/.test(value); // Includes Vietnamese characters
		},
	},

	/**
	 * Email Validations
	 */
	email: {
		/**
		 * Checks if input is a valid email address
		 * @param {string} value - The email to check
		 * @returns {boolean} - True if valid email, false otherwise
		 */
		isValid: function (value) {
			if (typeof value !== "string" || value.trim() === "") return false;
			// Regular expression for email validation
			const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
			return emailRegex.test(value);
		},
	},

	/**
	 * Phone Number Validations
	 */
	phone: {
		/**
		 * Checks if input is a valid Vietnamese phone number
		 * @param {string} value - The phone number to check
		 * @returns {boolean} - True if valid Vietnamese phone, false otherwise
		 */
		isValidVN: function (value) {
			if (typeof value !== "string") return false;
			// Remove spaces, dashes, etc.
			const cleanValue = value.replace(/\s+|-|\(|\)/g, "");
			// Vietnamese phone number pattern
			return /^(0[3|5|7|8|9]|84[3|5|7|8|9])[0-9]{8}$/.test(cleanValue);
		},

		/**
		 * Format phone number with spaces for better readability
		 * @param {string} value - The phone number to format
		 * @returns {string} - Formatted phone number
		 */
		format: function (value) {
			if (typeof value !== "string") return value;
			// Remove non-numeric characters
			const cleaned = value.replace(/\D/g, "");

			// Format as Vietnamese phone number
			if (cleaned.length === 10) {
				return cleaned.replace(/(\d{3})(\d{3})(\d{4})/, "$1 $2 $3");
			} else if (cleaned.length === 11 && cleaned.startsWith("84")) {
				return "0" + cleaned.substring(2).replace(/(\d{3})(\d{3})(\d{4})/, "$1 $2 $3");
			}

			return value;
		},
	},

	/**
	 * Date Validations
	 */
	date: {
		/**
		 * Checks if input is a valid date
		 * @param {string} value - The date string to check
		 * @returns {boolean} - True if valid date, false otherwise
		 */
		isValid: function (value) {
			if (!value) return false;
			const date = new Date(value);
			return !isNaN(date.getTime());
		},

		/**
		 * Checks if date is in the past
		 * @param {string} value - The date string to check
		 * @returns {boolean} - True if date is in the past, false otherwise
		 */
		isPast: function (value) {
			if (!this.isValid(value)) return false;
			const date = new Date(value);
			const now = new Date();
			return date < now;
		},

		/**
		 * Checks if date is in the future
		 * @param {string} value - The date string to check
		 * @returns {boolean} - True if date is in the future, false otherwise
		 */
		isFuture: function (value) {
			if (!this.isValid(value)) return false;
			const date = new Date(value);
			const now = new Date();
			return date > now;
		},

		/**
		 * Checks if date is within range
		 * @param {string} value - The date string to check
		 * @param {string} startDate - Start date
		 * @param {string} endDate - End date
		 * @returns {boolean} - True if date is within range, false otherwise
		 */
		inRange: function (value, startDate, endDate) {
			if (!this.isValid(value) || !this.isValid(startDate) || !this.isValid(endDate)) return false;
			const date = new Date(value);
			const start = new Date(startDate);
			const end = new Date(endDate);
			return date >= start && date <= end;
		},

		/**
		 * Format date as DD/MM/YYYY
		 * @param {string} value - The date string to format
		 * @returns {string} - Formatted date string
		 */
		formatDMY: function (value) {
			if (!this.isValid(value)) return "";
			const date = new Date(value);
			return `${date.getDate().toString().padStart(2, "0")}/${(date.getMonth() + 1).toString().padStart(2, "0")}/${date.getFullYear()}`;
		},
	},

	/**
	 * URL Validations
	 */
	url: {
		/**
		 * Checks if input is a valid URL
		 * @param {string} value - The URL to check
		 * @returns {boolean} - True if valid URL, false otherwise
		 */
		isValid: function (value) {
			if (typeof value !== "string" || value.trim() === "") return false;
			try {
				new URL(value);
				return true;
			} catch (e) {
				return false;
			}
		},
	},

	/**
	 * Custom Pattern Validations
	 */
	pattern: {
		/**
		 * Checks if input matches a regex pattern
		 * @param {string} value - The string to check
		 * @param {RegExp} regex - The regular expression to test against
		 * @returns {boolean} - True if matches pattern, false otherwise
		 */
		matches: function (value, regex) {
			if (typeof value !== "string") return false;
			return regex.test(value);
		},
	},

	/**
	 * Input sanitization methods
	 */
	sanitize: {
		/**
		 * Sanitize text to prevent XSS attacks
		 * @param {string} value - The string to sanitize
		 * @returns {string} - Sanitized string
		 */
		text: function (value) {
			if (typeof value !== "string") return "";
			const map = {
				"&": "&amp;",
				"<": "&lt;",
				">": "&gt;",
				'"': "&quot;",
				"'": "&#x27;",
				"/": "&#x2F;",
			};
			const reg = /[&<>"'/]/gi;
			return value.replace(reg, (match) => map[match]);
		},
	},

	/**
	 * Restricts input in real-time based on validation rules
	 * @param {HTMLElement} inputElement - The input element being typed in
	 * @param {string} validationType - Type of validation to apply
	 * @param {Object} options - Additional validation options
	 */
	validateRealTime: function (inputElement, validationType, options = {}) {
		const value = inputElement.value;
		let newValue = value;
		let isValid = true;
		let cursorPosition = inputElement.selectionStart;

		// Apply validation based on type
		switch (validationType) {
			case "number":
				isValid = this.validateNumberInput(inputElement, options);
				break;
			case "decimal":
				isValid = this.validateDecimalInput(inputElement, options);
				break;
			case "currency":
				isValid = this.validateCurrencyInput(inputElement, options);
				break;
			case "phone":
				isValid = this.validatePhoneInput(inputElement, options);
				break;
			case "letters":
				isValid = this.validateLettersInput(inputElement, options);
				break;
			case "alphanumeric":
				isValid = this.validateAlphanumericInput(inputElement, options);
				break;
			case "email":
				isValid = this.validateEmailInput(inputElement, options);
				break;
			default:
				// No validation applied
				break;
		}

		return isValid;
	},

	/**
	 * Validates number input, allowing only numbers
	 * @param {HTMLInputElement} input - The input element
	 * @param {Object} options - Validation options (min, max)
	 * @returns {boolean} - Whether input is valid
	 */
	number: function (input, options = {allowZeroFirst: false}) {
		const oldValue = input.value;
		const cursorPosition = input.selectionStart;

		// Strip non-numeric characters
		let newValue = oldValue.replace(/[^0-9]/g, "");

		// Remove leading zeros (if not just "0")
		if (options.allowZeroFirst) {
			// Nếu chuỗi mới chỉ toàn số 0 (ví dụ "0", "00", "000"), giữ lại 1 số 0 thôi
			if (/^0+$/.test(newValue)) {
				newValue = "0";
			} 
		}
		else
		{
			if (newValue.length > 1 && newValue.startsWith("0"))
			{
				newValue = newValue.replace(/^0+/, "");
			}
		}

		// Apply min/max constraints if provided and value isn't empty
		if (newValue && options.min !== undefined && parseInt(newValue) < options.min) {
			newValue = options.min.toString();
		}
		if (newValue && options.max !== undefined && parseInt(newValue) > options.max) {
			newValue = options.max.toString();
		}

		// Only update if value changed
		if (oldValue !== newValue) {
			input.value = newValue;
			// Restore cursor position
			input.setSelectionRange(Math.min(cursorPosition, newValue.length), Math.min(cursorPosition, newValue.length));
			return false;
		}
		return true;
	},

	/**
	 * Validates decimal input, allowing numbers and decimal point
	 * @param {HTMLInputElement} input - The input element
	 * @param {Object} options - Validation options (decimals, min, max)
	 * @returns {boolean} - Whether input is valid
	 */
	decimal: function (input, options = {}) {
		const oldValue = input.value;
		const cursorPosition = input.selectionStart;

		// Allow only digits and one decimal point
		let newValue = oldValue.replace(/[^0-9.]/g, "");

		// Ensure only one decimal point
		const parts = newValue.split(".");
		if (parts.length > 2) {
			newValue = parts[0] + "." + parts.slice(1).join("");
		}

		// Limit decimal places
		if (parts.length > 1 && options.decimals !== undefined) {
			if (parts[1].length > options.decimals) {
				newValue = parts[0] + "." + parts[1].substring(0, options.decimals);
			}
		}

		// Apply min/max constraints if provided and value isn't empty
		if (newValue && options.min !== undefined && parseFloat(newValue) < options.min) {
			newValue = options.min.toString();
		}
		if (newValue && options.max !== undefined && parseFloat(newValue) > options.max) {
			newValue = options.max.toString();
		}

		// Only update if value changed
		if (oldValue !== newValue) {
			input.value = newValue;
			// Restore cursor position
			input.setSelectionRange(Math.min(cursorPosition, newValue.length), Math.min(cursorPosition, newValue.length));
			return false;
		}
		return true;
	},

	/**
	 * Format input thành tiền tệ Việt Nam, giới hạn theo min/max nếu có
	 * @param {HTMLInputElement} input - Trường nhập liệu
	 * @param {Object} options - Cấu hình min/max
	 * @param {number} [options.min] - Giá trị tối thiểu
	 * @param {number} [options.max] - Giá trị tối đa
	 * @returns {boolean}
	 */
	currency: function (input, options = {}) {
		const oldValue = input.value?.trim() ?? "";

		// Lưu lại giá trị hợp lệ cuối cùng
		if (!input.dataset.lastValidValue) {
			input.dataset.lastValidValue = oldValue;
		}

		const hasMin = typeof options.min === "number";
		const minValue = hasMin ? options.min : null;

		const hasMax = typeof options.max === "number";
		const maxValue = hasMax ? options.max : null;

		// Nếu rỗng và có min => gán min
		if (hasMin && oldValue === "") {
			const formattedMin = new Intl.NumberFormat("vi-VN").format(minValue);
			input.value = formattedMin;
			input.dataset.lastValidValue = formattedMin;
			return false;
		}

		// Nếu chứa ký tự không hợp lệ thì khôi phục lại giá trị cũ
		if (/[^\d\s.,]/.test(oldValue)) {
			input.value = input.dataset.lastValidValue;
			return false;
		}

		// Loại bỏ tất cả ký tự không phải số (giữ số thập phân nếu cần thì regex khác)
		let numericValue = oldValue.replace(/[^\d]/g, "");
		if (numericValue === "") return true;

		let parsedValue = parseInt(numericValue, 10);

		// Áp min/max nếu có
		if (hasMin && parsedValue < minValue) {
			parsedValue = minValue;
		}
		if (hasMax && parsedValue > maxValue) {
			parsedValue = maxValue;
		}

		// Format lại để hiển thị đẹp
		const formattedValue = new Intl.NumberFormat("vi-VN").format(parsedValue);

		// Cập nhật nếu có thay đổi
		if (oldValue !== formattedValue) {
			input.value = formattedValue;
			input.dataset.lastValidValue = formattedValue;
			return false;
		}

		return true;
	},

	/**
	 * Restricts input to numbers only (integers)
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Options like min, max
	 * @returns {boolean} - True if valid
	 */
	validateNumberInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Strip non-numeric characters
		let newValue = oldValue.replace(/[^0-9]/g, "");

		// Apply min/max constraints if provided
		if (newValue !== "") {
			const num = parseInt(newValue);
			if (options.min !== undefined && num < options.min) {
				newValue = options.min.toString();
			}
			if (options.max !== undefined && num > options.max) {
				newValue = options.max.toString();
			}
		}

		// Only update if the value changed
		if (oldValue !== newValue) {
			inputElement.value = newValue;

			// Restore cursor position adjusted for removed characters
			const posDiff = oldValue.length - newValue.length;
			inputElement.setSelectionRange(cursorPosition - posDiff, cursorPosition - posDiff);

			return false;
		}
		return true;
	},

	/**
	 * Restricts input to decimal numbers
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Options like decimals (number of decimal places)
	 * @returns {boolean} - True if valid
	 */
	validateDecimalInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Allow only digits and one decimal point
		let newValue = oldValue.replace(/[^0-9.]/g, "");

		// Ensure only one decimal point
		const parts = newValue.split(".");
		if (parts.length > 2) {
			newValue = parts[0] + "." + parts.slice(1).join("");
		}

		// Limit decimal places if specified
		if (parts.length > 1 && options.decimals !== undefined) {
			newValue = parts[0] + "." + parts[1].substring(0, options.decimals);
		}

		// Apply min/max constraints if provided
		if (newValue !== "" && newValue !== ".") {
			const num = parseFloat(newValue);
			if (!isNaN(num)) {
				if (options.min !== undefined && num < options.min) {
					newValue = options.min.toString();
				}
				if (options.max !== undefined && num > options.max) {
					newValue = options.max.toString();
				}
			}
		}

		// Only update if the value changed
		if (oldValue !== newValue) {
			inputElement.value = newValue;

			// Restore cursor position adjusted for removed characters
			const posDiff = oldValue.length - newValue.length;
			inputElement.setSelectionRange(cursorPosition - posDiff, cursorPosition - posDiff);

			return false;
		}
		return true;
	},

	/**
	 * Restricts input to currency format (with thousand separators)
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Options for currency format
	 * @returns {boolean} - True if valid
	 */
	validateCurrencyInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Remove all non-digits except decimal point
		let cleanValue = oldValue.replace(/[^0-9.]/g, "");

		// Ensure only one decimal point
		const parts = cleanValue.split(".");
		if (parts.length > 2) {
			cleanValue = parts[0] + "." + parts.slice(1).join("");
		}

		// Limit decimal places
		const decimals = options.decimals || 0;
		if (parts.length > 1 && decimals > 0) {
			cleanValue = parts[0] + "." + parts[1].substring(0, decimals);
		} else if (decimals === 0) {
			cleanValue = parts[0];
		}

		// Format with thousand separators
		let formattedValue = cleanValue;
		if (cleanValue !== "" && cleanValue !== ".") {
			const numValue = parseFloat(cleanValue);
			if (!isNaN(numValue)) {
				// Format number with thousand separators
				const wholePartStr = parts[0];
				let formattedWhole = "";

				// Add thousand separators
				for (let i = wholePartStr.length; i > 0; i -= 3) {
					const start = Math.max(0, i - 3);
					const segment = wholePartStr.substring(start, i);
					formattedWhole = segment + (formattedWhole ? "," + formattedWhole : "");
				}

				formattedValue = formattedWhole;
				if (parts.length > 1 && decimals > 0) {
					formattedValue += "." + parts[1].substring(0, decimals);
				}
			}
		}

		// Only update if the value changed
		if (oldValue !== formattedValue) {
			inputElement.value = formattedValue;

			// Try to adjust cursor position for the formatting changes
			// This is complex due to thousand separators
			let newCursorPos = cursorPosition;
			// Add logic here to adjust cursor position

			inputElement.setSelectionRange(newCursorPos, newCursorPos);
			return false;
		}
		return true;
	},

	/**
	 * Restricts input to valid phone number format
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Validation options
	 * @returns {boolean} - True if valid
	 */
	validatePhoneInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Allow only digits and some special characters used in phone numbers
		let newValue = oldValue.replace(/[^0-9+\-\s()]/g, "");

		// Format for Vietnamese phone if specified
		if (options.country === "VN") {
			// Basic VN phone formatting logic
			if (newValue.startsWith("0") && newValue.length > 3) {
				// Format as: 0xx xxxx xxxx
				const cleaned = newValue.replace(/\D/g, "");
				if (cleaned.length <= 4) {
					newValue = cleaned;
				} else if (cleaned.length <= 7) {
					newValue = cleaned.substring(0, 3) + " " + cleaned.substring(3);
				} else {
					newValue = cleaned.substring(0, 3) + " " + cleaned.substring(3, 7) + " " + cleaned.substring(7, 11);
				}
			}
		}

		// Enforce maximum length if specified
		if (options.maxLength && newValue.length > options.maxLength) {
			newValue = newValue.substring(0, options.maxLength);
		}

		// Only update if the value changed
		if (oldValue !== newValue) {
			inputElement.value = newValue;

			// Adjust cursor position for formatting changes
			let newCursorPos = cursorPosition;
			// Complex cursor positioning logic

			inputElement.setSelectionRange(newCursorPos, newCursorPos);
			return false;
		}
		return true;
	},

	/**
	 * Restricts input to letters only (with space)
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Validation options
	 * @returns {boolean} - True if valid
	 */
	validateLettersInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Default pattern: letters, spaces, Vietnamese diacritics
		let pattern = /[^A-Za-zÀ-ỹ\s]/g;

		// If special chars are allowed, modify the pattern
		if (options.allowSpecial) {
			pattern = /[^A-Za-zÀ-ỹ\s.,\-_()]/g;
		}

		// Strip invalid characters
		let newValue = oldValue.replace(pattern, "");

		// Only update if the value changed
		if (oldValue !== newValue) {
			inputElement.value = newValue;

			// Restore cursor position adjusted for removed characters
			const posDiff = oldValue.length - newValue.length;
			inputElement.setSelectionRange(cursorPosition - posDiff, cursorPosition - posDiff);

			return false;
		}
		return true;
	},

	/**
	 * Restricts input to alphanumeric characters
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Validation options
	 * @returns {boolean} - True if valid
	 */
	validateAlphanumericInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Default pattern: letters, numbers, spaces, Vietnamese diacritics
		let pattern = /[^A-Za-z0-9À-ỹ\s]/g;

		// If special chars are allowed, modify the pattern
		if (options.allowSpecial) {
			pattern = /[^A-Za-z0-9À-ỹ\s.,\-_()]/g;
		}

		// Strip invalid characters
		let newValue = oldValue.replace(pattern, "");

		// Only update if the value changed
		if (oldValue !== newValue) {
			inputElement.value = newValue;

			// Restore cursor position adjusted for removed characters
			const posDiff = oldValue.length - newValue.length;
			inputElement.setSelectionRange(cursorPosition - posDiff, cursorPosition - posDiff);

			return false;
		}
		return true;
	},

	/**
	 * Validates input for email format
	 * @param {HTMLElement} inputElement - The input element
	 * @param {Object} options - Validation options
	 * @returns {boolean} - True if valid
	 */
	validateEmailInput: function (inputElement, options = {}) {
		const oldValue = inputElement.value;
		const cursorPosition = inputElement.selectionStart;

		// Allow characters valid in emails
		let newValue = oldValue.replace(/[^a-zA-Z0-9@._\-+]/g, "");

		// Only update if the value changed
		if (oldValue !== newValue) {
			inputElement.value = newValue;

			// Restore cursor position adjusted for removed characters
			const posDiff = oldValue.length - newValue.length;
			inputElement.setSelectionRange(cursorPosition - posDiff, cursorPosition - posDiff);

			return false;
		}

		return true;
	},

	/**
	 * Chuyển chuỗi tiền định dạng '1.000.000' thành số 1000000
	 * @param {string} value - Chuỗi tiền tệ có dấu chấm ngăn cách
	 * @returns {number} - Số nguyên đã parse
	 */
	parseCurrency(value) {
			if (!value) return 0;
	const raw = value.replace(/\./g, "").trim(); // Bỏ dấu chấm
	const number = parseInt(raw, 10);
	return isNaN(number) ? 0 : number;
	}

};

// Example usage:
// InputValidator.number.isValid('123'); // true
// InputValidator.email.isValid('test@example.com'); // true
// InputValidator.phone.isValidVN('0912345678'); // true

// Example usage in HTML:
// <input type="text" oninput="InputValidator.validateRealTime(this, 'number', {min: 0, max: 100})" />
// <input type="text" oninput="InputValidator.validateRealTime(this, 'decimal', {decimals: 2})" />
// <input type="text" oninput="InputValidator.validateRealTime(this, 'currency')" />
// <input type="text" oninput="InputValidator.validateRealTime(this, 'phone', {country: 'VN'})" />
// <input type="text" oninput="InputValidator.validateRealTime(this, 'letters')" />
// <input type="text" oninput="InputValidator.validateRealTime(this, 'alphanumeric', {allowSpecial: true})" />
// <input type="email" oninput="InputValidator.validateRealTime(this, 'email')" />
