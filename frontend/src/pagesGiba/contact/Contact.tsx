import React from "react";
import useSetHeader from "../../components/hooks/useSetHeader";
import FeatureInDevelopment from "../../components/FeatureInDevelopment";

const Contact: React.FC = () => {
  const setHeader = useSetHeader();
  
  React.useEffect(() => {
    setHeader({
      title: "LIÊN HỆ",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader]);

  return (
    <div className="page-contact">
      <FeatureInDevelopment 
        title="Liên hệ đang phát triển"
        message="Tính năng liên hệ đang được phát triển và sẽ sớm ra mắt. Chúng tôi sẽ thông báo cho bạn khi tính năng này sẵn sàng!"
      />
    </div>
  );
};

export default Contact;
