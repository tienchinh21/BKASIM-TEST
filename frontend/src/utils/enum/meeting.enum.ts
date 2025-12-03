export enum MeetingGroupType {
  ALL = "",
  NBD = "NBD",
  CLUB = "Club",
}

export enum MeetingStatus {
  ALL = "",
  SCHEDULED = "1",
  ONGOING = "2",
  COMPLETED = "3",
  CANCELLED = "4",
}

export const MeetingGroupLabel: Record<MeetingGroupType, string> = {
  [MeetingGroupType.ALL]: "Tất cả",
  [MeetingGroupType.NBD]: "NBD",
  [MeetingGroupType.CLUB]: "Club",
};

export const MeetingStatusLabel: Record<MeetingStatus, string> = {
  [MeetingStatus.ALL]: "Tất cả",
  [MeetingStatus.SCHEDULED]: "Sắp diễn ra",
  [MeetingStatus.ONGOING]: "Đang diễn ra",
  [MeetingStatus.COMPLETED]: "Đã hoàn thành",
  [MeetingStatus.CANCELLED]: "Đã hủy",
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

export const MeetingTabsData: TabGroup[] = [
  {
    id: "all",
    name: "Tất cả",
    value: MeetingGroupType.ALL,
    children: [
      { id: "all-all", name: "Tất cả", value: MeetingStatus.ALL },
      { id: "all-scheduled", name: "Sắp diễn ra", value: MeetingStatus.SCHEDULED },
      { id: "all-ongoing", name: "Đang diễn ra", value: MeetingStatus.ONGOING },
      { id: "all-completed", name: "Đã hoàn thành", value: MeetingStatus.COMPLETED },
      { id: "all-cancelled", name: "Đã hủy", value: MeetingStatus.CANCELLED },
    ],
  },
  {
    id: "nbd",
    name: "NBD",
    value: MeetingGroupType.NBD,
    children: [
      { id: "nbd-all", name: "Tất cả", value: MeetingStatus.ALL },
      { id: "nbd-scheduled", name: "Sắp diễn ra", value: MeetingStatus.SCHEDULED },
      { id: "nbd-ongoing", name: "Đang diễn ra", value: MeetingStatus.ONGOING },
      { id: "nbd-completed", name: "Đã hoàn thành", value: MeetingStatus.COMPLETED },
      { id: "nbd-cancelled", name: "Đã hủy", value: MeetingStatus.CANCELLED },
    ],
  },
  {
    id: "club",
    name: "Club",
    value: MeetingGroupType.CLUB,
    children: [
      { id: "club-all", name: "Tất cả", value: MeetingStatus.ALL },
      { id: "club-scheduled", name: "Sắp diễn ra", value: MeetingStatus.SCHEDULED },
      { id: "club-ongoing", name: "Đang diễn ra", value: MeetingStatus.ONGOING },
      { id: "club-completed", name: "Đã hoàn thành", value: MeetingStatus.COMPLETED },
      { id: "club-cancelled", name: "Đã hủy", value: MeetingStatus.CANCELLED },
    ],
  },
];

