import React from "react";

interface FloatingActionButtonGibaProps {
  icon: React.ReactNode;
  onClick: () => void;
  position?: "bottom-right" | "bottom-left" | "top-right" | "top-left";
  color?: "yellow" | "blue" | "green" | "red" | "purple";
  size?: "sm" | "md" | "lg";
  className?: string;
  tooltip?: string;
}

const FloatingActionButtonGiba: React.FC<FloatingActionButtonGibaProps> = ({
  icon,
  onClick,
  position = "bottom-right",
  color = "yellow",
  size = "md",
  className = "",
  tooltip,
}) => {
  const getPositionClasses = () => {
    switch (position) {
      case "bottom-left":
        return "bottom-6 left-6";
      case "top-right":
        return "top-6 right-6";
      case "top-left":
        return "top-6 left-6";
      case "bottom-right":
      default:
        return "bottom-6 right-2";
    }
  };

  const getColorClasses = () => {
    switch (color) {
      case "blue":
        return "bg-blue-500 hover:bg-blue-600 text-white";
      case "green":
        return "bg-green-500 hover:bg-green-600 text-white";
      case "red":
        return "bg-red-500 hover:bg-red-600 text-white";
      case "purple":
        return "bg-purple-500 hover:bg-purple-600 text-white";
      case "yellow":
      default:
        return "bg-yellow-400 hover:bg-yellow-500 text-black";
    }
  };

  const getSizeClasses = () => {
    switch (size) {
      case "sm":
        return "w-12 h-12";
      case "lg":
        return "w-16 h-16";
      case "md":
      default:
        return "w-14 h-14";
    }
  };

  const getIconSize = () => {
    switch (size) {
      case "sm":
        return 20;
      case "lg":
        return 28;
      case "md":
      default:
        return 24;
    }
  };

  return (
    <div className="relative">
      <button
        onClick={onClick}
        className={`fixed ${getPositionClasses()} ${getColorClasses()} ${getSizeClasses()} rounded-full shadow-lg hover:shadow-xl transition-all duration-200 flex items-center justify-center z-50 transform hover:scale-105 active:scale-95 ${className}`}
        style={{
          boxShadow: "0 4px 12px rgba(0, 0, 0, 0.15)",
        }}
        title={tooltip}
      >
        <div className="relative">
          {React.cloneElement(icon as React.ReactElement, {
            size: getIconSize(),
            strokeWidth: 2.5,
          })}
        </div>
      </button>
      {tooltip && (
        <div className="fixed invisible group-hover:visible opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none z-50">
          <div className="bg-gray-800 text-white text-xs px-2 py-1 rounded whitespace-nowrap">
            {tooltip}
          </div>
        </div>
      )}
    </div>
  );
};

export default FloatingActionButtonGiba;
