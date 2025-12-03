export enum AppointmentStatus {
  PENDING = 1, // Chờ xác nhận
  CONFIRMED = 2, // Đã xác nhận
  CANCELLED = 3, // Đã hủy (người đặt hẹn hủy)
  REJECTED = 4, // Từ chối (người nhận từ chối)
}

export const AppointmentStatusLabel: Record<AppointmentStatus, string> = {
  [AppointmentStatus.PENDING]: "Chờ xác nhận",
  [AppointmentStatus.CONFIRMED]: "Đã xác nhận",
  [AppointmentStatus.CANCELLED]: "Đã hủy",
  [AppointmentStatus.REJECTED]: "Từ chối",
};

export const AppointmentStatusColor: Record<AppointmentStatus, string> = {
  [AppointmentStatus.PENDING]: "bg-yellow-100 text-yellow-700",
  [AppointmentStatus.CONFIRMED]: "bg-blue-100 text-blue-700",
  [AppointmentStatus.CANCELLED]: "bg-red-100 text-red-700",
  [AppointmentStatus.REJECTED]: "bg-orange-100 text-orange-700",
};

export const AppointmentStatusGradient: Record<AppointmentStatus, string> = {
  [AppointmentStatus.PENDING]: "linear-gradient(90deg, #0066cc, #003d82)",
  [AppointmentStatus.CONFIRMED]: "linear-gradient(90deg, #0066cc, #003d82)",
  [AppointmentStatus.CANCELLED]: "linear-gradient(90deg, #ef4444, #dc2626)",
  [AppointmentStatus.REJECTED]: "linear-gradient(90deg, #f97316, #ea580c)",
};

// Tab data cho TwoTierTab
export const AppointmentTabsData = [
  {
    id: "all",
    name: "Tất cả",
    value: "all",
    children: [
      { id: "status-all", name: "Tất cả", value: "" },
      { id: "status-pending", name: "Chờ xác nhận", value: "1" },
      { id: "status-confirmed", name: "Đã xác nhận", value: "2" },
      { id: "status-cancelled", name: "Đã hủy", value: "3" },
      { id: "status-rejected", name: "Từ chối", value: "4" },
    ],
  },
  {
    id: "from",
    name: "Đã gửi",
    value: "from",
    children: [
      { id: "from-status-all", name: "Tất cả", value: "" },
      { id: "from-status-pending", name: "Chờ xác nhận", value: "1" },
      { id: "from-status-confirmed", name: "Đã xác nhận", value: "2" },
      { id: "from-status-cancelled", name: "Đã hủy", value: "3" },
      // Ẩn "Từ chối" vì người gửi không thể bị từ chối bởi chính mình
    ],
  },
  {
    id: "to",
    name: "Đã nhận",
    value: "to",
    children: [
      { id: "to-status-all", name: "Tất cả", value: "" },
      { id: "to-status-pending", name: "Chờ xác nhận", value: "1" },
      { id: "to-status-confirmed", name: "Đã xác nhận", value: "2" },
      // Ẩn "Đã hủy" vì người nhận không thể hủy lịch hẹn
      { id: "to-status-rejected", name: "Từ chối", value: "4" },
    ],
  },
];
