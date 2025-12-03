export enum EventType {
  ALL = "",
  NBD = "NBD",
  CLUB = "Club",
  MY_GROUPS = "me",
}

// Custom field types for event registration
export enum CustomFieldType {
  Text = 1,              // Văn bản (Text)
  Integer = 2,           // Số nguyên (Integer)
  Decimal = 3,           // Số thập phân (Decimal)
  YearOfBirth = 4,       // Năm sinh (Year of birth)
  Boolean = 5,           // Boolean (Đúng/Sai)
  DateTime = 6,          // Ngày giờ (Date and time)
  Date = 7,              // Ngày (Date)
  Email = 8,             // Email
  PhoneNumber = 9,       // Số điện thoại (Phone number)
  Url = 10,              // Đường dẫn URL (URL link)
  LongText = 11,         // Văn bản dài (Long text)
  Dropdown = 12,         // Danh sách lựa chọn (Dropdown list)
  MultipleChoice = 13,   // Đa lựa chọn (Multiple choice)
}

export enum EventStatus {
  ALL = "",
  ONGOING = "2",
  UPCOMING = "1",
  ENDED = "3",
}

export enum CheckInStatus {
  NotCheckIn = 1,    // Chưa check-in
  CheckedIn = 2,     // Đã check-in
  Cancelled = 3,     // Đã hủy
}

export const EventTypeLabel: Record<EventType, string> = {
  [EventType.ALL]: "Tất cả",
  [EventType.NBD]: "NBD",
  [EventType.CLUB]: "Club",
  [EventType.MY_GROUPS]: "Sự kiện của tôi",
};

export const EventStatusLabel: Record<EventStatus, string> = {
  [EventStatus.ALL]: "Tất cả",
  [EventStatus.ONGOING]: "Đang diễn ra",
  [EventStatus.UPCOMING]: "Sắp diễn ra",
  [EventStatus.ENDED]: "Đã kết thúc",
};

export interface TabItem {
  id: string;
  name: string;
  value: string;
}

export interface TabGroup {
  id: string;
  name: string;
  value: string;
  children?: TabItem[];
}

export const EventTabsData: TabGroup[] = [
  {
    id: "all",
    name: "Tất cả",
    value: EventType.ALL,
    children: [
      { id: "all-all", name: "Tất cả", value: EventStatus.ALL },
      { id: "all-ongoing", name: "Đang diễn ra", value: EventStatus.ONGOING },
      { id: "all-upcoming", name: "Sắp diễn ra", value: EventStatus.UPCOMING },
      { id: "all-ended", name: "Đã kết thúc", value: EventStatus.ENDED },
    ],
  },
  {
    id: "nbd",
    name: "NBD",
    value: EventType.NBD,
    children: [
      { id: "nbd-all", name: "Tất cả", value: EventStatus.ALL },
      { id: "nbd-ongoing", name: "Đang diễn ra", value: EventStatus.ONGOING },
      { id: "nbd-upcoming", name: "Sắp diễn ra", value: EventStatus.UPCOMING },
      { id: "nbd-ended", name: "Đã kết thúc", value: EventStatus.ENDED },
    ],
  },
  {
    id: "club",
    name: "Club",
    value: EventType.CLUB,
    children: [
      { id: "club-all", name: "Tất cả", value: EventStatus.ALL },
      { id: "club-ongoing", name: "Đang diễn ra", value: EventStatus.ONGOING },
      { id: "club-upcoming", name: "Sắp diễn ra", value: EventStatus.UPCOMING },
      { id: "club-ended", name: "Đã kết thúc", value: EventStatus.ENDED },
    ],
  },
  {
    id: "my-groups",
    name: "Sự kiện của tôi",
    value: EventType.MY_GROUPS,
    children: [
      { id: "my-all", name: "Tất cả", value: EventStatus.ALL },
      { id: "my-ongoing", name: "Đang diễn ra", value: EventStatus.ONGOING },
      { id: "my-upcoming", name: "Sắp diễn ra", value: EventStatus.UPCOMING },
      { id: "my-ended", name: "Đã kết thúc", value: EventStatus.ENDED },
    ],
  },
];
