import React from "react";

interface FeatureInDevelopmentProps {
  title?: string;
  message?: string;
  icon?: React.ReactNode;
}

const FeatureInDevelopment: React.FC<FeatureInDevelopmentProps> = ({
  title = "Tính năng đang phát triển",
  message = "Tính năng này đang được phát triển và sẽ sớm ra mắt. Vui lòng quay lại sau!",
  icon = null,
}) => {
  return (
    <div className="flex flex-col items-center justify-center min-h-[100vh] px-6 py-12 bg-black">
      <div className="w-full max-w-md mx-auto text-center">
        <h2 className="text-3xl font-bold text-white mb-4">{title}</h2>
        <p className="text-gray-300 leading-relaxed">{message}</p>
      </div>
    </div>
  );
};

export default FeatureInDevelopment;
