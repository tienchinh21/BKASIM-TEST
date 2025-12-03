/**
 * Badge Component
 * Reusable badge component with different variants and sizes
 */

import React from "react";

export type BadgeVariant =
  | "blue"
  | "green"
  | "purple"
  | "orange"
  | "red"
  | "gray"
  | "yellow"
  | "indigo"
  | "pink";

export type BadgeSize = "xs" | "sm" | "md" | "lg";

export interface BadgeProps {
  text: string;
  variant?: BadgeVariant;
  size?: BadgeSize;
  className?: string;
  style?: React.CSSProperties;
  onClick?: () => void;
}

const Badge: React.FC<BadgeProps> = ({
  text,
  variant = "blue",
  size = "md",
  className = "",
  style = {},
  onClick,
}) => {
  const variantClasses: Record<BadgeVariant, string> = {
    blue: "bg-blue-600 text-white",
    green: "bg-green-600 text-white",
    purple: "bg-purple-600 text-white",
    orange: "bg-orange-600 text-white",
    red: "bg-red-600 text-white",
    gray: "bg-gray-600 text-white",
    yellow: "bg-yellow-600 text-white",
    indigo: "bg-indigo-600 text-white",
    pink: "bg-pink-600 text-white",
  };

  const sizeClasses: Record<BadgeSize, string> = {
    xs: "px-1.5 py-0.5 text-xs rounded",
    sm: "px-2 py-0.5 text-xs rounded",
    md: "px-3 py-1 text-sm rounded-md",
    lg: "px-4 py-2 text-base rounded-lg",
  };

  return (
    <div
      className={`inline-flex items-center justify-center font-semibold ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      style={style}
      onClick={onClick}
    >
      {text}
    </div>
  );
};

export default Badge;

