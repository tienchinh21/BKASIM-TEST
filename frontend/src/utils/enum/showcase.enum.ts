export enum ShowcaseGroupType {
  ALL = "",
  NBD = "NBD",
  CLUB = "Club",
}

export enum ShowcaseStatus {
  ALL = "",
  SCHEDULED = "1",
  ONGOING = "2",
  COMPLETED = "3",
  CANCELLED = "4",
}

export const ShowcaseGroupLabel: Record<ShowcaseGroupType, string> = {
  [ShowcaseGroupType.ALL]: "Tất cả",
  [ShowcaseGroupType.NBD]: "NBD",
  [ShowcaseGroupType.CLUB]: "Club",
};

export const ShowcaseStatusLabel: Record<ShowcaseStatus, string> = {
  [ShowcaseStatus.ALL]: "Tất cả",
  [ShowcaseStatus.SCHEDULED]: "Sắp diễn ra",
  [ShowcaseStatus.ONGOING]: "Đang diễn ra",
  [ShowcaseStatus.COMPLETED]: "Đã hoàn thành",
  [ShowcaseStatus.CANCELLED]: "Đã hủy",
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

export const ShowcaseTabsData: TabGroup[] = [
  {
    id: "all",
    name: "Tất cả",
    value: ShowcaseGroupType.ALL,
    children: [
      { id: "all-all", name: "Tất cả", value: ShowcaseStatus.ALL },
      { id: "all-scheduled", name: "Sắp diễn ra", value: ShowcaseStatus.SCHEDULED },
      { id: "all-ongoing", name: "Đang diễn ra", value: ShowcaseStatus.ONGOING },
      { id: "all-completed", name: "Đã hoàn thành", value: ShowcaseStatus.COMPLETED },
      { id: "all-cancelled", name: "Đã hủy", value: ShowcaseStatus.CANCELLED },
    ],
  },
  {
    id: "nbd",
    name: "NBD",
    value: ShowcaseGroupType.NBD,
    children: [
      { id: "nbd-all", name: "Tất cả", value: ShowcaseStatus.ALL },
      { id: "nbd-scheduled", name: "Sắp diễn ra", value: ShowcaseStatus.SCHEDULED },
      { id: "nbd-ongoing", name: "Đang diễn ra", value: ShowcaseStatus.ONGOING },
      { id: "nbd-completed", name: "Đã hoàn thành", value: ShowcaseStatus.COMPLETED },
      { id: "nbd-cancelled", name: "Đã hủy", value: ShowcaseStatus.CANCELLED },
    ],
  },
  {
    id: "club",
    name: "Club",
    value: ShowcaseGroupType.CLUB,
    children: [
      { id: "club-all", name: "Tất cả", value: ShowcaseStatus.ALL },
      { id: "club-scheduled", name: "Sắp diễn ra", value: ShowcaseStatus.SCHEDULED },
      { id: "club-ongoing", name: "Đang diễn ra", value: ShowcaseStatus.ONGOING },
      { id: "club-completed", name: "Đã hoàn thành", value: ShowcaseStatus.COMPLETED },
      { id: "club-cancelled", name: "Đã hủy", value: ShowcaseStatus.CANCELLED },
    ],
  },
];
