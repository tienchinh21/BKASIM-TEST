import React from "react";
import { useNavigate, Swiper } from "zmp-ui";
const SwiperNew = (props) => {
  const listNew = props.list || [];
  const navigate = useNavigate();

  return (
    <Swiper
      style={{ borderRadius: "5px", border: "1px solid #D6D6D6" }}
      loop={listNew.length > 1}
      dots={listNew.length > 1}
      autoplay={props.autoplay || true}
      duration={props.duration || 3000}
    >
      {listNew.map((item, index) => (
        <Swiper.Slide key={index}>
          <div
            className="slide-image-wrapper"
            onClick={() => navigate(`/detailNew/${item.id}`, { state: item })}
          >
            <img
              className="slide-image"
              src={item?.bannerImage || item?.images?.[0]}
              alt={`Slide ${index + 1}`}
            />
          </div>
          <div style={{ padding: "0 7px" }}>
            <div className="news-title-home">{item?.title}</div>
          </div>
        </Swiper.Slide>
      ))}
    </Swiper>
  );
};

export default SwiperNew;
