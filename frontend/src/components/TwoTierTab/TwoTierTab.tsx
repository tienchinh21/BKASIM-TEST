import React, { useState, useEffect, useRef, memo } from "react";
import { Box } from "zmp-ui";
import "./two-tier-tab.css";

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

interface TwoTierTabProps {
  tabs: TabGroup[];

  activeTab: string;

  onTabChange: (tabValue: string) => void;

  activeChildTab?: string;

  onChildTabChange?: (childValue: string) => void;

  containerStyle?: React.CSSProperties;

  backgroundColor?: string;
}

const TwoTierTab: React.FC<TwoTierTabProps> = ({
  tabs,
  activeTab,
  onTabChange,
  activeChildTab = "",
  onChildTabChange = () => {},
  containerStyle = {},
  backgroundColor = "#fff",
}) => {
  const tabRefs = useRef<{ [key: string]: HTMLDivElement | null }>({});
  const [childTabs, setChildTabs] = useState<TabItem[]>([]);

  useEffect(() => {
    const currentTab = tabs.find((tab) => tab.value === activeTab);

    if (currentTab?.children && currentTab.children.length > 0) {
      setChildTabs(currentTab.children);
    } else {
      setChildTabs([]);
    }

    if (tabRefs.current[activeTab]) {
      tabRefs.current[activeTab]?.scrollIntoView({
        behavior: "smooth",
        inline: "center",
        block: "nearest",
      });
    }
  }, [activeTab, tabs]);

  return (
    <div className="two-tier-tab-container">
      <Box
        className="two-tier-tab-tier1 hidden-scrollbar-y"
        style={{
          backgroundColor: backgroundColor,
          ...containerStyle,
        }}
      >
        {tabs.map((tab) => (
          <div
            key={tab.id}
            ref={(el) => (tabRefs.current[tab.value] = el)}
            className={`tier1-tab-item ${
              activeTab === tab.value ? "tier1-tab-item-active" : ""
            }`}
            onClick={() => onTabChange(tab.value)}
          >
            {tab.name}
          </div>
        ))}
      </Box>

      {childTabs.length > 0 && (
        <Box className="two-tier-tab-tier2">
          <div className="tier2-tabs-wrapper hidden-scrollbar-y">
            {childTabs.map((child) => (
              <div
                key={child.id}
                className={`tier2-tab-item ${
                  activeChildTab === child.value
                    ? "tier2-tab-item-active"
                    : ""
                } ${child.disabled ? "tier2-tab-item-disabled" : ""}`}
                onClick={() => !child.disabled && onChildTabChange(child.value)}
                style={{
                  cursor: child.disabled ? "not-allowed" : "pointer",
                  opacity: child.disabled ? 0.5 : 1,
                }}
              >
                {child.name}
              </div>
            ))}
          </div>
        </Box>
      )}
    </div>
  );
};

export default memo(TwoTierTab);
