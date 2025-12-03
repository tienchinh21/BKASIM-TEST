import React from "react";
import { Page } from "zmp-ui";
import useSetHeader from "../../components/hooks/useSetHeader";
import FeatureInDevelopment from "../../components/FeatureInDevelopment";

const ManagerMeetings: React.FC = () => {
   const setHeader = useSetHeader();
    React.useEffect(() => {
      setHeader({
        title: "QUẢN LÝ LỊCH HỌP",
        showUserInfo: false,
        showMenuButton: false,
        showCloseButton: false,
        hasLeftIcon: true,
      });
    }, [setHeader]);

  return (
    <Page style={{ marginTop: "50px", background: "#fff" }}>
      <FeatureInDevelopment
        title="Quản lý lịch họp"
        message="Tính năng quản lý lịch họp đang được phát triển và sẽ sớm ra mắt. Vui lòng quay lại sau!"
      />
    </Page>
  );
};

export default ManagerMeetings;

