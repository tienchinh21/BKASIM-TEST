/**
 * Giba Color Constants
 * Modern, minimal color palette for clean UI design
 */

export const colorsGiba = {
  // Primary colors - Modern blue/purple gradient
  primary: "#6366f1", // Indigo-500
  primaryDark: "#4f46e5", // Indigo-600
  primaryLight: "#818cf8", // Indigo-400

  // Secondary colors - Elegant gray scale
  secondary: "#64748b", // Slate-500
  secondaryDark: "#475569", // Slate-600
  secondaryLight: "#94a3b8", // Slate-400

  // Status colors - Clean and clear
  success: "#10b981", // Emerald-500
  warning: "#f59e0b", // Amber-500
  error: "#ef4444", // Red-500
  info: "#3b82f6", // Blue-500

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
  accent: "#8b5cf6", // Violet-500
  accentLight: "#a78bfa", // Violet-400
} as const;

export type ColorKey = keyof typeof colorsGiba;

export default colorsGiba;
