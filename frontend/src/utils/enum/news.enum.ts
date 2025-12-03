export enum NewsGroupType {
  ALL = "",
  NBD = "NBD",
  CLUB = "Club",
}

export const NewsGroupTypeLabel: Record<NewsGroupType, string> = {
  [NewsGroupType.ALL]: "Tất cả",
  [NewsGroupType.NBD]: "NBD",
  [NewsGroupType.CLUB]: "Club",
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

// Will be populated dynamically from API categories
export const createNewsTabsData = (categories: any[]): TabGroup[] => {
  const categoryChildren: TabItem[] = [
    { id: "all-cat", name: "Tất cả", value: "" },
    ...categories.map((cat) => ({
      id: cat.id,
      name: cat.name,
      value: cat.id,
    })),
  ];

  return [
    {
      id: "all",
      name: "Tất cả",
      value: NewsGroupType.ALL,
      children: categoryChildren,
    },
    {
      id: "nbd",
      name: "NBD",
      value: NewsGroupType.NBD,
      children: categoryChildren,
    },
    {
      id: "club",
      name: "Club",
      value: NewsGroupType.CLUB,
      children: categoryChildren,
    },
  ];
};
