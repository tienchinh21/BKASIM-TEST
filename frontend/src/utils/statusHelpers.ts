/**
 * Status Helper Utilities
 * Centralized status badge/color/gradient logic to avoid code duplication
 */

import {
  AppointmentStatus,
  AppointmentStatusColor,
  AppointmentStatusGradient,
  AppointmentStatusLabel,
} from "./enum/appointment.enum";

/**
 * Get appointment status information (color, gradient, text)
 * @param status - Appointment status number
 * @returns Object with color, gradient, and text
 * @example
 * getAppointmentStatus(1) 
 * // Returns: { 
 * //   color: "bg-yellow-100 text-yellow-700",
 * //   gradient: "linear-gradient(90deg, #fbbf24, #f59e0b)",
 * //   text: "Chờ xác nhận"
 * // }
 */
export const getAppointmentStatus = (status: number) => ({
  color:
    AppointmentStatusColor[status as AppointmentStatus] ||
    "bg-gray-100 text-gray-700",
  gradient:
    AppointmentStatusGradient[status as AppointmentStatus] ||
    "linear-gradient(90deg, #6b7280, #4b5563)",
  text:
    AppointmentStatusLabel[status as AppointmentStatus] || "Không xác định",
});

/**
 * Get showcase status badge information
 * @param status - Showcase status number (0-3)
 * @returns Object with text and color classes
 * @example
 * getShowcaseStatusBadge(0) 
 * // Returns: { text: "Sắp diễn ra", color: "bg-blue-100 text-blue-700" }
 */
export const getShowcaseStatusBadge = (status: number) => {
  const statusMap: Record<number, { text: string; color: string }> = {
    0: { text: "Sắp diễn ra", color: "bg-blue-100 text-blue-700" },
    1: { text: "Đang diễn ra", color: "bg-green-100 text-green-700" },
    2: { text: "Đã hoàn thành", color: "bg-gray-100 text-gray-700" },
    3: { text: "Đã hủy", color: "bg-red-100 text-red-700" },
  };
  return (
    statusMap[status] || { text: "Không xác định", color: "bg-gray-100 text-gray-700" }
  );
};

/**
 * Get appointment status color classes only
 * @param status - Appointment status number
 * @returns Tailwind color classes
 */
export const getAppointmentStatusColor = (status: number): string => {
  return (
    AppointmentStatusColor[status as AppointmentStatus] ||
    "bg-gray-100 text-gray-700"
  );
};

/**
 * Get appointment status gradient only
 * @param status - Appointment status number
 * @returns CSS gradient string
 */
export const getAppointmentStatusGradient = (status: number): string => {
  return (
    AppointmentStatusGradient[status as AppointmentStatus] ||
    "linear-gradient(90deg, #6b7280, #4b5563)"
  );
};

/**
 * Get appointment status text only
 * @param status - Appointment status number
 * @returns Status text in Vietnamese
 */
export const getAppointmentStatusText = (status: number): string => {
  return (
    AppointmentStatusLabel[status as AppointmentStatus] || "Không xác định"
  );
};

