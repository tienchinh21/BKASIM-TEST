import React, { memo, useEffect } from "react";
import { useNavigate, Box } from "zmp-ui";
import "./news.css";
import dayjs from "dayjs";
import { GlobalStyles } from "../../store/styles/GlobalStyles";

const ItemNew = ({ item }) => {
  const navigate = useNavigate();
  return (
    <Box
      flex
      justifyContent="space-between"
      alignItems="center"
      key={item.id}
      style={{
        background: "#fff",
        overflow: "hidden",
        borderBottom: "0.5px solid #000",
        padding: "10px 14px",
      }}
      onClick={() => {
        navigate(`/detailNew/${item.id}`);
      }}
    >
      <img
        src={item?.bannerImage}
        alt={item?.title}
        style={{
          width: "122px",
          height: "122px",
          objectFit: "cover",
          borderRadius: GlobalStyles.borderRadius.sm,
        }}
      />

      <Box
        style={{
          width: "66%",
          flex: 1,
          padding: "10px 15px",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
        }}
      >
        <Box
          style={{
            fontSize: 12,
            background: "#E3E3E3",
            width: "fit-content",
            borderRadius: GlobalStyles.borderRadius.xs,
            padding: "0 8px",
            marginBottom: 4,
          }}
        >
          {dayjs(item?.createdDate).format("DD/MM/YYYY")}
        </Box>
        <Box className="title-new">{item?.title}</Box>
        <Box
          className="text-sub"
          dangerouslySetInnerHTML={{ __html: item?.summarizeContent }}
        ></Box>
      </Box>
    </Box>
  );
};

export default memo(ItemNew);
