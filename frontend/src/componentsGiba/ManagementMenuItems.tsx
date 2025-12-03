import React from "react";
import {
  Users,
  Group,
  FileText,
  Calendar,
  GraduationCap,
  BarChart3,
  CalendarCheck,
} from "lucide-react";

export interface ManagementMenuItem {
  id: string;
  title: string;
  subtitle?: string;
  icon: React.ReactNode;
  showSubAction?: boolean;
  subActionText?: string;
  roles: string[];
  isDevelopment?: boolean;
}

export const gibaManagementItems: ManagementMenuItem[] = [
  {
    id: "manage-members",
    title: "Quản lý thành viên",
    subtitle: "GIBA, nhóm NBD/Club",
    icon: <Users className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
  },
  {
    id: "manage-groups",
    title: "Quản lý nhóm",
    subtitle: "NBD/Club",
    icon: <Group className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
  },
  {
    id: "manage-articles",
    title: "Quản lý bản tin",
    subtitle: "Xem và xóa bản tin",
    icon: <FileText className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
  },
  {
    id: "manage-showcase",
    title: "Lịch showcase",
    subtitle: "Club Group NBD",
    icon: <Calendar className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
  },
  {
    id: "manage-meetings",
    title: "Quản lý lịch họp",
    subtitle: "GIBA, NBD, Club",
    icon: <CalendarCheck className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
  },
  {
    id: "manage-training",
    title: "Quản lý đào tạo",
    subtitle: "Tính năng đang phát triển",
    icon: <GraduationCap className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
    isDevelopment: true,
  },
  {
    id: "manage-reports",
    title: "Xem thống kê & báo cáo",
    subtitle: "GIBA, NBD, Club",
    icon: <BarChart3 className="w-5 h-5 text-gray-600" />,
    roles: ["GIBA"],
  },
];

export const nbdManagementItems: ManagementMenuItem[] = [
  {
    id: "manage-members",
    title: "Quản lý thành viên",
    subtitle: "Nhóm NBD",
    icon: <Users className="w-5 h-5 text-gray-600" />,
    roles: ["NBD"],
  },
  {
    id: "manage-groups",
    title: "Quản lý nhóm",
    subtitle: "NBD",
    icon: <Group className="w-5 h-5 text-gray-600" />,
    roles: ["NBD"],
  },
  {
    id: "manage-articles",
    title: "Quản lý bản tin",
    subtitle: "Xem và xóa bản tin",
    icon: <FileText className="w-5 h-5 text-gray-600" />,
    roles: ["NBD"],
  },
  {
    id: "manage-meetings",
    title: "Quản lý lịch họp",
    subtitle: "NBD, Club",
    icon: <CalendarCheck className="w-5 h-5 text-gray-600" />,
    roles: ["NBD"],
  },
  {
    id: "manage-training",
    title: "Quản lý đào tạo",
    subtitle: "Tính năng đang phát triển",
    icon: <GraduationCap className="w-5 h-5 text-gray-600" />,
    roles: ["NBD"],
    isDevelopment: true,
  },
  {
    id: "manage-reports",
    title: "Xem thống kê & báo cáo",
    subtitle: "NBD",
    icon: <BarChart3 className="w-5 h-5 text-gray-600" />,
    roles: ["NBD"],
  },
];

export const clubManagementItems: ManagementMenuItem[] = [
  {
    id: "manage-members",
    title: "Quản lý thành viên",
    subtitle: "Nhóm Club",
    icon: <Users className="w-5 h-5 text-gray-600" />,
    roles: ["Club"],
  },
  {
    id: "manage-groups",
    title: "Quản lý nhóm",
    subtitle: "Club",
    icon: <Group className="w-5 h-5 text-gray-600" />,
    roles: ["Club"],
  },
  {
    id: "manage-articles",
    title: "Quản lý bản tin",
    subtitle: "Xem và xóa bản tin",
    icon: <FileText className="w-5 h-5 text-gray-600" />,
    roles: ["Club"],
  },
  {
    id: "manage-showcase",
    title: "Lịch showcase",
    subtitle: "Club Group",
    icon: <Calendar className="w-5 h-5 text-gray-600" />,
    roles: ["Club"],
  },
  {
    id: "manage-training",
    title: "Quản lý đào tạo",
    subtitle: "Tính năng đang phát triển",
    icon: <GraduationCap className="w-5 h-5 text-gray-600" />,
    roles: ["Club"],
    isDevelopment: true,
  },
  {
    id: "manage-reports",
    title: "Xem thống kê & báo cáo",
    subtitle: "Club",
    icon: <BarChart3 className="w-5 h-5 text-gray-600" />,
    roles: ["Club"],
  },
];

export const groupManagementItems: ManagementMenuItem[] = [
  {
    id: "manage-members",
    title: "Quản lý thành viên",
    subtitle: "Nhóm quản lý",
    icon: <Users className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
  },
  {
    id: "manage-groups",
    title: "Quản lý nhóm",
    subtitle: "Nhóm quản lý",
    icon: <Group className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
  },
  {
    id: "manage-articles",
    title: "Quản lý bản tin",
    subtitle: "Xem và xóa bản tin",
    icon: <FileText className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
  },
  {
    id: "manage-showcase",
    title: "Lịch showcase",
    subtitle: "Nhóm quản lý thuộc Club Group",
    icon: <Calendar className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
  },
  {
    id: "manage-training",
    title: "Quản lý đào tạo",
    subtitle: "Tính năng đang phát triển",
    icon: <GraduationCap className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
    isDevelopment: true,
  },
  {
    id: "manage-meetings",
    title: "Quản lý lịch họp",
    subtitle: "Nhóm quản lý thuộc NBD/Club",
    icon: <CalendarCheck className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
  },
  {
    id: "manage-reports",
    title: "Xem thống kê & báo cáo",
    subtitle: "Nhóm quản lý",
    icon: <BarChart3 className="w-5 h-5 text-gray-600" />,
    roles: ["Group"],
  },
];

export const getManagementMenuItems = (
  roleName: string
): ManagementMenuItem[] => {
  if (!roleName || roleName.trim() === "") {
    return [];
  }

  switch (roleName) {
    case "GIBA":
      return gibaManagementItems;
    case "NBD":
      return nbdManagementItems;
    case "Club":
      return clubManagementItems;
    case "Group":
      return groupManagementItems;
    default:
      return [];
  }
};
