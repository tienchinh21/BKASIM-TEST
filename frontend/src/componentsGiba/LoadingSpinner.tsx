/**
 * LoadingSpinner Component
 * Reusable loading spinner with different sizes and colors
 */

import React from "react";

export type SpinnerSize = "xs" | "sm" | "md" | "lg" | "xl";
export type SpinnerColor = "black" | "white" | "blue" | "green" | "gray";

export interface LoadingSpinnerProps {
  size?: SpinnerSize;
  color?: SpinnerColor;
  className?: string;
  fullScreen?: boolean;
  text?: string;
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = "md",
  color = "black",
  className = "",
  fullScreen = false,
  text,
}) => {
  const sizeClasses: Record<SpinnerSize, string> = {
    xs: "h-3 w-3 border",
    sm: "h-4 w-4 border",
    md: "h-8 w-8 border-2",
    lg: "h-12 w-12 border-2",
    xl: "h-16 w-16 border-4",
  };

  const colorClasses: Record<SpinnerColor, string> = {
    black: "border-black",
    white: "border-white",
    blue: "border-blue-600",
    green: "border-green-600",
    gray: "border-gray-600",
  };

  const spinner = (
    <div
      className={`animate-spin rounded-full border-t-transparent ${sizeClasses[size]} ${colorClasses[color]} ${className}`}
    ></div>
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 flex flex-col items-center justify-center bg-white bg-opacity-80 z-50">
        {spinner}
        {text && <p className="mt-4 text-gray-700 text-sm">{text}</p>}
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center">
      {spinner}
      {text && <p className="mt-2 text-gray-700 text-sm">{text}</p>}
    </div>
  );
};

export default LoadingSpinner;

