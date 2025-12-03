import { useEffect, useState } from "react";
import { getSystemInfo } from "zmp-sdk";

const usePlatform = () => {
    const [platform, setPlatform] = useState(null);

    useEffect(() => {
        const fetchPlatform = async () => {
            try {
                const res = await getSystemInfo();
                setPlatform(res.platform);
            } catch (err) {
                console.error("Lỗi lấy hệ điều hành:", err);
            }
        };

        fetchPlatform();
    }, []);

    return platform;
};

export default usePlatform;
