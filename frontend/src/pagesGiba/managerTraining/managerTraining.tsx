import React from "react";
import { Page } from "zmp-ui";
import useSetHeader from "../../components/hooks/useSetHeader";
import FeatureInDevelopment from "../../components/FeatureInDevelopment";

const ManagerTraining: React.FC = () => {
  const setHeader = useSetHeader();
  React.useEffect(() => {
    setHeader({
      title: "Quản lý đào tạo",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  return (
    <Page style={{ marginTop: "50px", background: "#fff" }}>
      <FeatureInDevelopment
        title="Quản lý đào tạo"
        message="Tính năng quản lý đào tạo đang được phát triển và sẽ sớm ra mắt. Vui lòng quay lại sau!"
      />
    </Page>
  );
};

export default ManagerTraining;
