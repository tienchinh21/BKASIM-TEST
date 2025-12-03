import React from "react";

interface LoadingGibaProps {
  size?: "sm" | "md" | "lg";
  fullScreen?: boolean;
  text?: string;
}

const LoadingGiba: React.FC<LoadingGibaProps> = ({
  size = "md",
  fullScreen = false,
  text = "",
}) => {
  // Size configurations
  const sizeConfig = {
    sm: { spinner: "w-8 h-8", dots: "w-2 h-2" },
    md: { spinner: "w-12 h-12", dots: "w-3 h-3" },
    lg: { spinner: "w-16 h-16", dots: "w-4 h-4" },
  };

  const { spinner, dots } = sizeConfig[size];

  const LoadingContent = () => (
    <div className="flex flex-col items-center justify-center gap-4 mt-[100px]  ">
      {/* Simple modern spinner */}
      <div className="relative">
        <div className={`${spinner} relative`}>
          {/* Outer ring */}
          <div
            className={`
              absolute inset-0 rounded-full
              border-2 border-gray-200
            `}
          />
          {/* Animated ring */}
          <div
            className={`
              absolute inset-0 rounded-full
              border-2 border-transparent
              border-t-black
              animate-spin
            `}
            style={{ animationDuration: "1s" }}
          />
        </div>
      </div>

      {/* Loading text */}
      {text && <p className="text-gray-600 text-sm font-medium">{text}</p>}
    </div>
  );

  if (fullScreen) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-white/80 backdrop-blur-sm">
        <LoadingContent />
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center p-8">
      <LoadingContent />
    </div>
  );
};

export default LoadingGiba;
