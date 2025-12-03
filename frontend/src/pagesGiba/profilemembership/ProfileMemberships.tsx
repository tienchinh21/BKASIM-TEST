import React from "react";
import useSetHeader from "../../components/hooks/useSetHeader";
import FeatureInDevelopment from "../../components/FeatureInDevelopment";

const ProfileMembership: React.FC = () => {
  const setHeader = useSetHeader();
  
  React.useEffect(() => {
    setHeader({
      title: "Profile Thành viên",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  return (
    <div className="page-contact">
      <FeatureInDevelopment 
        title="Profile thành viên đang phát triển"
        message="Tính năng Profile thành viên đang được phát triển và sẽ sớm ra mắt. Chúng tôi sẽ thông báo cho bạn khi tính năng này sẵn sàng!"
      />
    </div>
  );
};

export default ProfileMembership;
