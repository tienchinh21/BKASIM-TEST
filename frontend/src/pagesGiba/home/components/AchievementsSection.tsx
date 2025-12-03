import React, { useState, useEffect } from "react";
import { useRecoilValue } from "recoil";
import { token } from "../../../recoil/RecoilState";
import {
  ChevronRight,
  Plus,
  Calendar,
  Users,
  Gift,
  Clock,
  CheckCircle,
  UserPlus,
  Send,
  Download,
} from "lucide-react";
import axios from "axios";
import dfData from "../../../common/DefaultConfig.json";
import { useNavigate } from "react-router-dom";

interface UserStats {
  events: {
    checkInCount: number;
  };
  guestInvites: {
    count: number;
    totalGuestNumber: number;
  };
  refs: {
    sent: {
      count: number;
      value: number;
    };
    received: {
      count: number;
      value: number;
    };
  };
  appointments: {
    confirmedCount: number;
  };
}

interface AchievementsSectionProps {
  onTitleClick?: (section: string) => void;
}

const AchievementsSection: React.FC<AchievementsSectionProps> = ({
  onTitleClick,
}) => {
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const [stats, setStats] = useState<UserStats | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchStats = async () => {
      if (!userToken) {
        return;
      }

      try {
        setLoading(true);
        const response = await axios.get(
          `${dfData.domain}/api/statistics/user-stats`,
          {
            headers: {
              Authorization: `Bearer ${userToken}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (response.data.success && response.data.data) {
          setStats(response.data.data);
        }
      } catch (error) {
        console.error("Error fetching user stats:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchStats();
  }, [userToken]);

  const handleTitleClick = (section: string) => {
    if (onTitleClick) {
      onTitleClick(section);
    } else {
      // Default: navigate to achievements page
      navigate("/giba/achievements");
    }
  };

  const handleIconClick = (section: string, e: React.MouseEvent) => {
    e.stopPropagation(); // Prevent triggering title click

    switch (section) {
      case "events":
        navigate("/giba/event");
        break;
      case "guests":
        navigate("/giba/guest-list-history");
        break;
      case "refs":
        navigate("/giba/ref-list");
        break;
      case "appointments":
        navigate("/giba/appointment-list");
        break;
      default:
        if (onTitleClick) {
          onTitleClick(section);
        } else {
          navigate("/giba/achievements");
        }
    }
  };

  if (!userToken) {
    return null;
  }

  if (loading) {
    return (
      <div className="mb-6">
        <div className="flex items-center justify-between mb-5">
          <h2 className="text-xl font-bold text-gray-900">Thành tích</h2>
        </div>
        <div className="flex items-center justify-center py-8">
          <div className="text-gray-500 text-sm">Đang tải...</div>
        </div>
      </div>
    );
  }

  if (!stats) {
    return null;
  }

  const StatCard: React.FC<{
    title: string;
    value: string | number;
    subtitle?: string;
    compact?: boolean;
    icon?: React.ReactNode;
    iconColor?: string;
    bgColor?: string;
    onClick?: () => void;
  }> = ({
    title,
    value,
    subtitle,
    compact = false,
    icon,
    iconColor = "text-blue-600",
    bgColor = "bg-blue-50",
    onClick,
  }) => (
    <div
      className={`bg-white rounded-xl shadow-md border border-gray-100 transition-all duration-200 hover:shadow-lg hover:scale-[1.02] ${
        compact ? "p-3" : "p-4"
      } ${onClick ? "cursor-pointer" : ""}`}
      onClick={onClick}
    >
      <div className="flex items-start justify-between mb-2">
        <div className="flex-1">
          <div
            className={`text-gray-500 mb-1.5 font-medium ${
              compact ? "text-xs" : "text-sm"
            }`}
          >
            {title}
          </div>
          <div
            className={`font-bold text-gray-900 leading-tight ${
              compact ? "text-xl" : "text-2xl"
            }`}
          >
            {value}
          </div>
        </div>
        {icon && (
          <div
            className={`${bgColor} ${iconColor} p-2 rounded-lg flex-shrink-0`}
          >
            {icon}
          </div>
        )}
      </div>
      {subtitle && (
        <div
          className={`text-gray-400 mt-2 pt-2 border-t border-gray-100 ${
            compact ? "text-xs" : "text-xs"
          }`}
        >
          {subtitle}
        </div>
      )}
    </div>
  );

  const SectionTitle: React.FC<{
    title: string;
    section: string;
    icon?: React.ReactNode;
    iconColor?: string;
  }> = ({ title, section, icon, iconColor = "text-blue-600" }) => (
    <div className="flex items-center justify-between mb-3 group">
      <div className="flex items-center gap-2">
        {icon && <div className={iconColor}>{icon}</div>}
        <h3
          className="text-base font-bold text-gray-900 cursor-pointer hover:text-gray-700 transition-colors"
          onClick={() => handleTitleClick(section)}
        >
          {title}
        </h3>
      </div>
      <button
        className="flex items-center justify-center w-7 h-7 rounded-full border-2 border-gray-200 text-gray-400 hover:border-blue-500 hover:text-blue-600 hover:bg-blue-50 transition-all duration-200 bg-white cursor-pointer active:scale-95"
        onClick={(e) => handleIconClick(section, e)}
      >
        <Plus size={16} />
      </button>
    </div>
  );

  return (
    <div className="mb-6">
      <div className="flex items-center justify-between mb-5">
        <h2 className="text-xl font-bold text-gray-900">Thành tích</h2>
      </div>

      {/* Sự kiện */}
      <div className="mb-5">
        <SectionTitle
          title="Sự kiện"
          section="events"
          icon={<Calendar size={18} />}
          iconColor="text-blue-600"
        />
        <StatCard
          title="Hiện diện"
          value={stats.events.checkInCount}
          compact
          icon={<CheckCircle size={20} />}
          iconColor="text-blue-600"
          bgColor="bg-blue-50"
          onClick={() => navigate("/giba/event-registration-history")}
        />
      </div>

      <div className="mb-5">
        <SectionTitle
          title="Khách mời"
          section="guests"
          icon={<UserPlus size={18} />}
          iconColor="text-purple-600"
        />
        <div className="grid grid-cols-2 gap-3">
          <StatCard
            title="Số lần mời"
            value={stats.guestInvites.count}
            compact
            icon={<UserPlus size={18} />}
            iconColor="text-purple-600"
            bgColor="bg-purple-50"
            onClick={() => navigate("/giba/guest-list-history")}
          />
          <StatCard
            title="Tổng số khách"
            value={stats.guestInvites.totalGuestNumber}
            compact
            icon={<Users size={18} />}
            iconColor="text-purple-600"
            bgColor="bg-purple-50"
            onClick={() => navigate("/giba/guest-list-history")}
          />
        </div>
      </div>

      {/* REFERRAL */}
      <div className="mb-5">
        <SectionTitle
          title="REFERRAL"
          section="refs"
          icon={<Gift size={18} />}
          iconColor="text-orange-600"
        />
        <div className="grid grid-cols-2 gap-3">
          <StatCard
            title="Đã gửi"
            value={stats.refs.sent.count}
            subtitle={`Giá trị: ${stats.refs.sent.value.toLocaleString()}`}
            compact
            icon={<Send size={18} />}
            iconColor="text-orange-600"
            bgColor="bg-orange-50"
            onClick={() =>
              navigate("/giba/ref-list", { state: { filter: "sent" } })
            }
          />
          <StatCard
            title="Đã nhận"
            value={stats.refs.received.count}
            subtitle={`Giá trị: ${stats.refs.received.value.toLocaleString()}`}
            compact
            icon={<Download size={18} />}
            iconColor="text-orange-600"
            bgColor="bg-orange-50"
            onClick={() =>
              navigate("/giba/ref-list", { state: { filter: "received" } })
            }
          />
        </div>
      </div>

      {/* Cuộc hẹn */}
      <div className="mb-5">
        <SectionTitle
          title="Cuộc hẹn"
          section="appointments"
          icon={<Clock size={18} />}
          iconColor="text-green-600"
        />
        <StatCard
          title="Đã xác nhận"
          value={stats.appointments.confirmedCount}
          compact
          icon={<CheckCircle size={20} />}
          iconColor="text-green-600"
          bgColor="bg-green-50"
          onClick={() => navigate("/giba/appointment-list")}
        />
      </div>
    </div>
  );
};

export default AchievementsSection;
