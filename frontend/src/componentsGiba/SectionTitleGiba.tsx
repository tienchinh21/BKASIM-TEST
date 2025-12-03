import React from "react";

interface SectionTitleGibaProps {
  title: string;
  className?: string;
}

const SectionTitleGiba: React.FC<SectionTitleGibaProps> = ({
  title,
  className = "",
}) => {
  return (
    <div className={`bg-black py-2 px-4 ${className}`}>
      <h2 className="text-white font-bold text-start text-base">{title}</h2>
    </div>
  );
};

export default SectionTitleGiba;
