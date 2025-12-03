/**
 * Giba Color Constants
 * Modern, minimal color palette for clean UI design
 */

export const colorsGiba = {
  // Primary colors - BKASIM Blue gradient
  primary: "#003d82", // Navy Blue
  primaryDark: "#001f47", // Darker Navy
  primaryLight: "#0066cc", // Sky Blue

  // Secondary colors - Elegant gray scale
  secondary: "#64748b", // Slate-500
  secondaryDark: "#475569", // Slate-600
  secondaryLight: "#94a3b8", // Slate-400

  // Status colors - Clean and clear
  success: "#10b981", // Emerald-500
  warning: "#f59e0b", // Amber-500
  error: "#ef4444", // Red-500
  info: "#0066cc", // Blue-500 (updated to match primary light)

  // Neutral colors - Modern backgrounds
  white: "#ffffff",
  background: "#f8fafc", // Slate-50
  surface: "#ffffff",
  border: "#e2e8f0", // Slate-200

  // Text colors - Clear hierarchy
  textPrimary: "#0f172a", // Slate-900
  textSecondary: "#475569", // Slate-600
  textTertiary: "#94a3b8", // Slate-400

  // Accent colors - Subtle highlights
  accent: "#0066cc", // Sky Blue (updated to match primary light)
  accentLight: "#4d94d9", // Lighter Sky Blue
} as const;

export type ColorKey = keyof typeof colorsGiba;

export default colorsGiba;
