export enum GroupType {
  ALL = "",
  NBD = "NBD",
  CLUB = "Club",
  MY_GROUPS = "my-groups",
}

export enum GroupJoinStatus {
  ALL = "",
  NOT_JOINED = "not-joined", // joinStatus === null
  PENDING = "pending",
  APPROVED = "approved",
}

export const GroupTypeLabel: Record<GroupType, string> = {
  [GroupType.ALL]: "Tất cả",
  [GroupType.NBD]: "NBD",
  [GroupType.CLUB]: "Club",
  [GroupType.MY_GROUPS]: "Nhóm của tôi",
};

export const GroupJoinStatusLabel: Record<GroupJoinStatus, string> = {
  [GroupJoinStatus.ALL]: "Tất cả",
  [GroupJoinStatus.NOT_JOINED]: "Chưa tham gia",
  [GroupJoinStatus.PENDING]: "Chờ duyệt",
  [GroupJoinStatus.APPROVED]: "Đã tham gia",
};

export interface TabItem {
  id: string;
  name: string;
  value: string;
  disabled?: boolean;
}

export interface TabGroup {
  id: string;
  name: string;
  value: string;
  children?: TabItem[];
}

export const GroupTabsData: TabGroup[] = [
  {
    id: "all",
    name: "Tất cả",
    value: GroupType.ALL,
    children: [
      { id: "all-all", name: "Tất cả", value: GroupJoinStatus.ALL },
      { id: "all-not-joined", name: "Chưa tham gia", value: GroupJoinStatus.NOT_JOINED },
      { id: "all-pending", name: "Chờ duyệt", value: GroupJoinStatus.PENDING },
      { id: "all-approved", name: "Đã tham gia", value: GroupJoinStatus.APPROVED },
    ],
  },
  {
    id: "nbd",
    name: "NBD",
    value: GroupType.NBD,
    children: [
      { id: "nbd-all", name: "Tất cả", value: GroupJoinStatus.ALL },
      { id: "nbd-not-joined", name: "Chưa tham gia", value: GroupJoinStatus.NOT_JOINED },
      { id: "nbd-pending", name: "Chờ duyệt", value: GroupJoinStatus.PENDING },
      { id: "nbd-approved", name: "Đã tham gia", value: GroupJoinStatus.APPROVED },
    ],
  },
  {
    id: "club",
    name: "Club",
    value: GroupType.CLUB,
    children: [
      { id: "club-all", name: "Tất cả", value: GroupJoinStatus.ALL },
      { id: "club-not-joined", name: "Chưa tham gia", value: GroupJoinStatus.NOT_JOINED },
      { id: "club-pending", name: "Chờ duyệt", value: GroupJoinStatus.PENDING },
      { id: "club-approved", name: "Đã tham gia", value: GroupJoinStatus.APPROVED },
    ],
  },
  {
    id: "my-groups",
    name: "Nhóm của tôi",
    value: GroupType.MY_GROUPS,
    children: [
      { id: "my-all", name: "Tất cả", value: GroupJoinStatus.ALL },
      { id: "my-pending", name: "Chờ duyệt", value: GroupJoinStatus.PENDING },
      { id: "my-approved", name: "Đã tham gia", value: GroupJoinStatus.APPROVED },
    ],
  },
];
