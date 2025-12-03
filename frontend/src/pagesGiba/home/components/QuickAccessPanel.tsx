import React from "react";
import { useRecoilValue } from "recoil";
import {
  isRegister,
  phoneNumberUser,
  userMembershipInfo,
} from "../../../recoil/RecoilState";
import { useNavigate, useSnackbar } from "zmp-ui";
interface QuickAccessItem {
  id: string;
  label: string;
  icon: React.ReactNode;
  requiresAuth?: boolean;
  link?: string;
  action?: () => void;
}

interface QuickAccessPanelProps {
  className?: string;
  userSlug?: string | null;
}

const QuickAccessPanel: React.FC<QuickAccessPanelProps> = ({
  className = "",
  userSlug = null,
}) => {
  const membershipInfo = useRecoilValue(userMembershipInfo);
  const isLoggedIn = useRecoilValue(isRegister);
  const navigate = useNavigate();
  const snackbar = useSnackbar();

  const row1Items: QuickAccessItem[] = [
    {
      id: "groups",
      label: "Nhóm",
      link: "/giba/groups",
      icon: (
        <div className="relative">
          {/* Group icon */}
          <div className="w-6 h-6 bg-black rounded-sm flex items-center justify-center">
            <div className="w-3 h-3 bg-white rounded-sm"></div>
          </div>
          {/* Small dots around */}
          <div className="absolute -top-1 -right-1 w-2 h-2 bg-yellow-400 rounded-full"></div>
          <div className="absolute -bottom-1 -left-1 w-2 h-2 bg-yellow-400 rounded-full"></div>
        </div>
      ),
    },
    {
      id: "events",
      label: "Sự kiện",
      requiresAuth: true,
      link: "/giba/event",
      icon: (
        <div className="relative">
          {/* Calendar icon */}
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1/2 transform -translate-x-1/2 w-4 h-3 border border-white rounded-sm"></div>
            <div className="absolute -top-1 left-1/2 transform -translate-x-1/2 w-1 h-2 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
    {
      id: "behavior-rules",
      label: "Quy tắc",
      link: "/giba/behavior-rules?readonly=true",
      icon: (
        <div className="relative">
          {/* ARISTINO text with lines */}
          <div className="text-black font-bold text-xs">GIBA</div>
          <div className="absolute -top-1 left-0 right-0 h-0.5 bg-yellow-400"></div>
          <div className="absolute -bottom-1 left-0 right-0 h-0.5 bg-yellow-400"></div>
        </div>
      ),
    },
    {
      id: "contact",
      label: "Liên hệ",
      link: "/giba/contact",
      icon: (
        <div className="relative">
          {/* Contact/Phone icon */}
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1/2 transform -translate-x-1/2 w-3 h-4 border border-white rounded-sm"></div>
            <div className="absolute bottom-1 left-1/2 transform -translate-x-1/2 w-1 h-1 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
  ];

  const row2Items: QuickAccessItem[] = [
    {
      id: "business-opportunity",
      label: "Cơ hội kinh doanh",
      link: "/giba/ref-list",
      requiresAuth: true,
      icon: (
        <div className="relative">
          {/* Business opportunity - REF text */}
          <div className="text-black font-bold text-xs">REF</div>
          <div className="absolute -top-1 left-0 right-0 h-0.5 bg-yellow-400"></div>
          <div className="absolute -bottom-1 left-0 right-0 h-0.5 bg-yellow-400"></div>
        </div>
      ),
    },
    {
      id: "newsletter",
      label: "Bản tin",
      link: "/giba/news",
      icon: (
        <div className="relative">
          {/* Newsletter - Paper/Document */}
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1 w-4 h-3 border border-white rounded-sm"></div>
            <div className="absolute top-2 left-2 w-2 h-0.5 bg-yellow-400 rounded-full"></div>
            <div className="absolute top-3 left-2 w-1 h-0.5 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
    {
      id: "showcase-calendar",
      label: "Lịch showcase",
      link: "/giba/showcase-list",
      requiresAuth: true,
      icon: (
        <div className="relative">
          {/* Showcase calendar */}
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1 w-4 h-3 border-2 border-white rounded-sm"></div>
            <div className="absolute top-0 left-2 w-0.5 h-1 bg-yellow-400 rounded-full"></div>
            <div className="absolute top-0 left-4 w-0.5 h-1 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
    {
      id: "meeting-calendar",
      label: "Lịch họp",
      requiresAuth: true,
      link: "/giba/meeting-list",
      icon: (
        <div className="relative">
          {/* Meeting calendar */}
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1 w-4 h-3 border border-white rounded-sm"></div>
            <div className="absolute top-2 left-1/2 transform -translate-x-1/2 w-2 h-0.5 bg-yellow-400 rounded-full"></div>
            <div className="absolute top-3 left-1/2 transform -translate-x-1/2 w-1 h-0.5 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
  ];

  const row3Items: QuickAccessItem[] = [
    {
      id: "appointment",
      label: "Đặt hẹn",
      link: "/giba/appointment-list",
      requiresAuth: true,
      icon: (
        <div className="relative">
          {/* Appointment - Clock */}
          <div className="w-6 h-6 bg-black rounded-full relative">
            <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-0.5 h-2 bg-white rounded-full"></div>
            <div className="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 w-1 h-0.5 bg-white rounded-full"></div>
            <div className="absolute -top-0.5 left-1/2 transform -translate-x-1/2 w-1 h-1 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
    {
      id: "training",
      label: "Đào tạo",
      link: "/giba/train",
      requiresAuth: true,
      icon: (
        <div className="relative">
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1 w-4 h-3 border border-white rounded-sm"></div>
            <div className="absolute top-0 left-1/2 transform -translate-x-1/2 w-3 h-0.5 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
    // {
    //   id: "list-guest",
    //   label: "Đơn danh sách khách mời",
    //   link: "/giba/guest-list-history",
    //   requiresAuth: true,
    //   icon: (
    //     <div className="relative">
    //       {/* Gift box */}
    //       <div className="w-8 h-6 bg-black rounded-sm relative">
    //         {/* Ribbon */}
    //         <div className="absolute -top-1 left-1/2 transform -translate-x-1/2 w-4 h-2 bg-yellow-400 rounded-full"></div>
    //         <div className="absolute -top-1 left-1/2 transform -translate-x-1/2 w-2 h-4 bg-yellow-400 rounded-full"></div>
    //       </div>
    //     </div>
    //   ),
    // },
    // {
    //   id: "ref",
    //   label: "Đơn Ref",
    //   link: "/giba/ref-list",
    //   requiresAuth: true,
    //   icon: (
    //     <div className="relative">
    //       {/* Phone with sound waves */}
    //       <div className="absolute -top-2 left-1/2 transform -translate-x-1/2">
    //         <div className="w-1 h-1 bg-gray-400 rounded-full"></div>
    //         <div className="absolute top-0 left-2 w-1 h-1 bg-gray-400 rounded-full"></div>
    //         <div className="absolute top-0 left-4 w-1 h-1 bg-yellow-400 rounded-full"></div>
    //       </div>
    //       {/* Phone receiver */}
    //       <div className="w-6 h-4 bg-black rounded-sm transform rotate-12"></div>
    //     </div>
    //   ),
    // },
    {
      id: "profile",
      label: "Profile cá nhân",
      requiresAuth: true,
      icon: (
        <div className="relative">
          <div className="w-6 h-6 bg-black rounded-full relative">
            <div className="absolute top-1 left-1/2 transform -translate-x-1/2 w-2 h-1 bg-white rounded-full"></div>
            <div className="absolute top-3 left-1/2 transform -translate-x-1/2 w-3 h-1 bg-white rounded-sm"></div>
            <div className="absolute -bottom-1 left-1/2 transform -translate-x-1/2 w-1 h-1 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
    {
      id: "achievements",
      label: "Thành tích",
      requiresAuth: true,
      link: "/giba/achievements",
      icon: (
        <div className="relative">
          <div className="w-6 h-6 bg-black rounded-sm relative">
            <div className="absolute top-1 left-1/2 transform -translate-x-1/2 w-3 h-2 bg-white rounded-sm"></div>
            <div className="absolute top-3 left-1/2 transform -translate-x-1/2 w-2 h-1 bg-yellow-400 rounded-full"></div>
            <div className="absolute -bottom-1 left-1/2 transform -translate-x-1/2 w-1 h-1 bg-yellow-400 rounded-full"></div>
          </div>
        </div>
      ),
    },
  ];

  const handleItemClick = (item: QuickAccessItem) => {
    if (item.requiresAuth) {
      if (!isLoggedIn) {
        snackbar.openSnackbar({
          text: "Vui lòng đăng nhập để sử dụng tính năng này!",
          type: "warning",
          duration: 4000,
        });
        return;
      }
      if (
        membershipInfo.approvalStatus === 0 ||
        membershipInfo.approvalStatus === null
      ) {
        snackbar.openSnackbar({
          text: "Tài khoản đang trong thời gian xét duyệt. Vui lòng chờ admin phê duyệt.",
          type: "warning",
          duration: 4000,
        });
        return;
      }
      if (membershipInfo.approvalStatus === 2) {
        snackbar.openSnackbar({
          text: "Tài khoản đã bị từ chối. Vui lòng liên hệ admin.",
          type: "error",
          duration: 4000,
        });
        return;
      }
    }

    if (item.id === "profile") {
      if (userSlug) {
        navigate(`/giba/member-detail/${userSlug}`);
      } else {
        navigate("/giba/profile-membership");
      }
      return;
    }

    if (item.link) {
      navigate(item.link);
    } else if (item.action) {
      // Thực thi action tùy chỉnh nếu có
      item.action();
    } else {
      console.log(`Navigate to ${item.id}`);
    }
  };

  const renderItem = (item: QuickAccessItem) => (
    <div
      key={item.id}
      className="flex flex-col items-center gap-1 p-1 cursor-pointer"
      onClick={() => handleItemClick(item)}
    >
      <div className="w-12 h-12 bg-white border-2 border-gray-200 rounded-full flex items-center justify-center text-black shadow-sm hover:shadow-md transition-shadow">
        {item.icon}
      </div>
      <span className="text-[11px] text-black text-center font-medium leading-tight">
        {item.label}
      </span>
    </div>
  );

  return (
    <div className={`${className}`}>
      <div className="bg-white rounded-lg py-2 px-2 shadow-lg border border-gray-100">
        {/* Hàng 1 */}
        <div className="grid grid-cols-4 gap-2 mb-2">
          {row1Items.map(renderItem)}
        </div>

        {/* Hàng 2 */}
        <div className="grid grid-cols-4 gap-2 mb-2">
          {row2Items.map(renderItem)}
        </div>

        {/* Hàng 3 */}
        <div className="grid grid-cols-4 gap-2">
          {row3Items.map(renderItem)}
        </div>
      </div>
    </div>
  );
};

export default QuickAccessPanel;
