import React from "react";

const ComingSoonSkeleton: React.FC = () => {
  return (
    <div
      className="bg-white rounded-lg overflow-hidden animate-pulse"
      style={{
        minWidth: "280px",
        width: "280px",
        flexShrink: 0,
      }}
    >
      {/* Banner skeleton - 16:9 aspect ratio */}
      <div
        className="bg-gray-200"
        style={{
          width: "100%",
          aspectRatio: "16 / 9",
        }}
      />
      
      {/* Content skeleton */}
      <div className="p-4 space-y-3">
        {/* Title skeleton */}
        <div className="space-y-2">
          <div className="h-4 bg-gray-200 rounded w-full" />
          <div className="h-4 bg-gray-200 rounded w-3/4" />
        </div>
        
        {/* Info items skeleton */}
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <div className="w-5 h-5 bg-gray-200 rounded" />
            <div className="h-3 bg-gray-200 rounded flex-1" />
          </div>
          <div className="flex items-center gap-2">
            <div className="w-5 h-5 bg-gray-200 rounded" />
            <div className="h-3 bg-gray-200 rounded flex-1" />
          </div>
        </div>
        
        {/* Buttons skeleton - minHeight 96px to match EventItem */}
        <div className="space-y-2" style={{ minHeight: "96px" }}>
          <div className="h-10 bg-gray-200 rounded" />
          <div className="h-10 bg-gray-200 rounded" />
        </div>
      </div>
    </div>
  );
};

export default ComingSoonSkeleton;
