import React from "react";
import { useNavigate } from "react-router-dom";
import { Info } from "lucide-react";
import BKASIMLogo from "../assets/BKASIM.jpg";

interface MemberHelloCardProps {
  isLoadingProfile?: boolean;
  term?: string | null;
  appPosition?: string | null;
}

const MemberHelloCard: React.FC<MemberHelloCardProps> = ({
  isLoadingProfile = false,
  term,
  appPosition,
}) => {
  const navigate = useNavigate();

  return (
    <div
      style={{
        background: "#fff",
        borderRadius: "8px",
        padding: "16px",
        display: "flex",
        flexDirection: "column",
        gap: "16px",
        boxShadow: "none",
        border: "1px solid #0066cc",
        cursor: "pointer",
        transition: "all 0.3s ease",
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <div
        style={{
          display: "flex",
          alignItems: "flex-start",
          gap: "16px",
          width: "100%",
          justifyContent: "center",
        }}
        onClick={() => navigate("/giba/profile-intro")}
      >
        {/* Logo */}
        <div
          style={{
            borderRadius: "12px",
            background: "#f3f4f6",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexShrink: 0,
          }}
        >
          <img
            src={BKASIMLogo}
            alt="BKASIM"
            style={{
              width: "56px",
              height: "56px",
              objectFit: "contain",
            }}
          />
        </div>

        {/* Content Column */}
        <div
          style={{
            flex: 1,
            minWidth: 0,
            display: "flex",
            flexDirection: "column",
            gap: "8px",
            justifyContent: "center",
          }}
        >
          <div
            style={{
              fontSize: "16px",
              fontWeight: "700",
              color: "#1f2937",
              marginTop: "10px",
            }}
          >
            BKASIM
          </div>
        </div>

        {/* Right Button */}
        <div
          style={{
            background: "transparent",
            borderRadius: "8px",
            padding: "6px 16px",
            fontSize: "13px",
            fontWeight: "600",
            color: "#1e40af",
            flexShrink: 0,
            whiteSpace: "nowrap",
            border: "1px solid #bfdbfe",
          }}
        >
          Truy cập
        </div>
      </div>
      <div
        style={{
          fontSize: "13px",
          color: "#6b7280",
          lineHeight: "1.4",
          display: "flex",
          alignItems: "flex-start",
          gap: "8px",
        }}
      >
        <Info
          size={16}
          style={{
            color: "#0066cc",
            flexShrink: 0,
            marginTop: "2px",
          }}
        />
        <span>Truy cập Mini App BKASIM để cập nhật thông tin mới nhất.</span>
      </div>
    </div>
  );
};

export default MemberHelloCard;
