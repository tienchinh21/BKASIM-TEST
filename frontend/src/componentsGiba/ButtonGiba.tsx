import React from "react";

interface ButtonGibaProps {
  label: string;
  onClick?: () => void;
  variant?: "primary" | "secondary" | "outline" | "ghost";
  size?: "sm" | "md" | "lg";
  disabled?: boolean;
  loading?: boolean;
  fullWidth?: boolean;
  icon?: React.ReactNode;
  className?: string;
}

const ButtonGiba: React.FC<ButtonGibaProps> = ({
  label,
  onClick,
  variant = "primary",
  size = "md",
  disabled = false,
  loading = false,
  fullWidth = false,
  icon,
  className = "",
}) => {
  // Base styles - ultra modern and premium
  const baseStyles = `
    relative inline-flex items-center justify-center gap-3
    font-bold text-sm tracking-wide
    rounded-2xl
    transition-all duration-300 ease-out
    focus:outline-none focus:ring-4 focus:ring-offset-2
    disabled:opacity-40 disabled:cursor-not-allowed
    transform active:scale-95
    overflow-hidden
    group
  `;

  // Variant styles - sophisticated and premium
  const variantStyles = {
    primary: `
      bg-gradient-to-r from-slate-900 via-slate-800 to-slate-900
      text-white shadow-2xl shadow-slate-900/40
      hover:shadow-3xl hover:shadow-slate-900/60
      hover:from-slate-800 hover:via-slate-700 hover:to-slate-800
      focus:ring-slate-500
      before:absolute before:inset-0 before:bg-gradient-to-r before:from-white/10 before:to-transparent before:opacity-0 hover:before:opacity-100 before:transition-opacity before:duration-300
    `,
    secondary: `
      bg-gradient-to-r from-slate-100 to-slate-200
      text-slate-800 shadow-lg shadow-slate-200/50
      hover:from-slate-200 hover:to-slate-300
      hover:shadow-xl hover:shadow-slate-300/60
      focus:ring-slate-400
    `,
    outline: `
      bg-transparent border-2 border-slate-300
      text-slate-700
      hover:bg-slate-50 hover:border-slate-400
      hover:shadow-lg hover:shadow-slate-200/50
      focus:ring-slate-400
    `,
    ghost: `
      bg-transparent text-slate-600
      hover:bg-slate-100 hover:text-slate-800
      focus:ring-slate-400
    `,
  };

  // Size styles - generous and comfortable
  const sizeStyles = {
    sm: "px-5 py-3 text-xs min-h-[40px]",
    md: "px-7 py-4 text-sm min-h-[48px]",
    lg: "px-9 py-5 text-base min-h-[56px]",
  };

  // Width styles
  const widthStyles = fullWidth ? "w-full" : "";

  return (
    <button
      onClick={onClick}
      disabled={disabled || loading}
      className={`
        ${baseStyles}
        ${variantStyles[variant]}
        ${sizeStyles[size]}
        ${widthStyles}
        ${className}
      `}
    >
      {loading ? (
        <>
          <svg
            className="animate-spin h-5 w-5"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          <span>Loading...</span>
        </>
      ) : (
        <>
          {icon && <span>{icon}</span>}
          <span>{label}</span>
        </>
      )}
    </button>
  );
};

export default ButtonGiba;
