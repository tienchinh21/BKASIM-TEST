import React, { useState, useEffect } from "react";
import { Select, Spin } from "antd";
import { Box } from "zmp-ui";
import { isShareLocation, storeCurrent } from "../../recoil/RecoilState";
import dfData from "../../common/DefaultConfig.json";
import axios from "axios";
import { MapPin, Store } from "lucide-react";
import { getLocation, getAccessToken, getSetting } from "zmp-sdk/apis";
import { useRecoilState } from "recoil";
import { isEmpty } from "../../common/Common";

const { Option } = Select;

const CustomSelect = () => {
  const [selected, setSelected] = useRecoilState(storeCurrent);
  const [isLocationAccessGranted, setIsLocationAccessGranted] =
    useRecoilState(isShareLocation);

  const [dataOption, setDataOption] = useState([]);

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    checkAuthSettingLocation();
  }, []);

  useEffect(() => {
    getBranch();
  }, [isLocationAccessGranted]);

  const checkAuthSettingLocation = async () => {
    try {
      const data = await getSetting({});
      if (
        !isEmpty(data?.authSetting) &&
        data.authSetting["scope.userLocation"] !== false
      ) {
        setIsLocationAccessGranted(true);
      } else {
        setIsLocationAccessGranted(false);
      }
    } catch (error) {}
  };

  const getBranch = async () => {
    if (isLocationAccessGranted) {
      setLoading(true);
      const accessToken = await getAccessToken({});
      getLocation({
        success: async (data) => {
          const { token } = data;
          if (accessToken && token) {
            axios
              .post(
                `${dfData.domain}/api/Branches/NearByMe`,
                {
                  accessToken: accessToken,
                  tokenNumber: token,
                  secretKey: dfData.secretKey,
                },
                {
                  params: {
                    isActive: true,
                  },
                }
              )
              .then((resp) => {
                setDataOption(resp.data.data);
                setLoading(false);
              })
              .catch((error) => {
                setLoading(false);
              });
          }
        },
        fail: (error) => {
          setIsLocationAccessGranted(false);
          setLoading(false);
        },
      });
    } else {
      axios
        .get(`${dfData.domain}/api/Branches`, {
          params: {
            isActive: true,
          },
        })
        .then((resp) => {
          setLoading(false);
          setDataOption(resp.data.data);
        })
        .catch((error) => setLoading(false));
    }
    setLoading(false);
  };

  const handleOnChangeSelect = (id) => {
    const store = dataOption.find((store) => store.id === id);
    setSelected(store);
  };

  return (
    <Select
      value={selected ? selected.name : undefined} // Chỉ hiển thị store.name
      onChange={(value) => handleOnChangeSelect(value)} // Lưu ID thay vì object
      placeholder="Chia sẻ vị trí - chọn điểm gần bạn"
      style={{ width: "100%", height: 50 }}
      dropdownRender={(menu) => (
        <div>
          <Spin spinning={loading} fullscreen />
          {/* Nút chia sẻ vị trí */}
          {!isLocationAccessGranted && (
            <div
              style={{
                padding: 10,
                display: "flex",
                alignItems: "center",
                borderBottom: "1px solid #ddd",
              }}
              onClick={() => {
                setIsLocationAccessGranted(true);
              }}
            >
              <MapPin size={24} style={{ color: "#4285F4", marginRight: 10 }} />
              <span style={{ flex: 1, fontWeight: 500 }}>
                Chia sẻ vị trí - chọn điểm gần bạn
              </span>
            </div>
          )}

          {menu}
        </div>
      )}
    >
      {dataOption.map((store) => (
        <Option key={store.id} value={store.id}>
          <Box flex flexDirection="row">
            <Box width={"15%"} className="divCenter">
              <img
                style={{ height: "5vh", width: "5vh" }}
                src={store?.images?.[0]}
              ></img>
            </Box>
            <Box
              width={"85%"}
              style={{
                display: "flex",
                flexDirection: "column",
                padding: 4,
              }}
            >
              <Box
                style={{
                  display: "flex",
                  flexDirection: "row",
                  fontSize: 14,
                }}
              >
                <Store size={24} style={{ color: "#A67D00", marginRight: 4, width: "10%" }} />
                <b width={"90%"}>{store?.name}</b>
              </Box>
              <Box
                style={{
                  display: "flex",
                  flexDirection: "row",
                  fontSize: 10,
                }}
              >
                <MapPin size={24} style={{ color: "#A67D00", marginRight: 4, width: "10%" }} />
                <Box
                  width={"90%"}
                  style={{
                    whiteSpace: "normal",
                    display: "flex",
                    alignItems: "center",
                  }}
                >
                  {store?.fullAddress}
                </Box>
              </Box>
            </Box>
          </Box>
        </Option>
      ))}
    </Select>
  );
};

export default CustomSelect;
