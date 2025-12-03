import { useSetRecoilState } from "recoil";
import appConfig from "../../../app-config.json";
import { headerState } from "../../recoil/RecoilState";
import { useCallback, useEffect, useRef } from "react";
import { useNavigate } from "zmp-ui";

const useSetHeader = () => {
    const setHeader = useSetRecoilState(headerState);
    const navigate = useNavigate();

    // Use refs to store latest values without causing re-renders
    const setHeaderRef = useRef(setHeader);
    const navigateRef = useRef(navigate);

    // Update refs when values change
    useEffect(() => {
        setHeaderRef.current = setHeader;
        navigateRef.current = navigate;
    }, [setHeader, navigate]);

    return useCallback(
        ({
            route = "",
            hasLeftIcon = true,
            rightIcon = null,
            title = appConfig.app.title,
            customTitle = false,
            type = "primary",
            isShowHeader = true,
            onBack = undefined,
            backTo = false, // If true, navigate to home giba when back button is clicked
            // Giba specific props
            userInfo = {
                name: "Guest",
                avatar: null,
                isLoggedIn: false
            },
            showUserInfo = false,
            showMenuButton = false,
            showCloseButton = false,
            onMenuClick = undefined,
            onCloseClick = undefined,
            groupLogo = null,
        }) => {
            // Tạo onBack function mặc định khi backTo = true
            const defaultOnBack = backTo ? () => navigateRef.current("/giba") : undefined;

            setHeaderRef.current({
                route,
                hasLeftIcon,
                rightIcon,
                title,
                customTitle,
                type,
                isShowHeader,
                onBack: onBack || defaultOnBack,
                backTo, // If true, navigate to home giba when back button is clicked
                // Giba specific props
                userInfo,
                showUserInfo,
                showMenuButton,
                showCloseButton,
                onMenuClick,
                onCloseClick,
                groupLogo,
                isGibaHeader: showUserInfo || showMenuButton || showCloseButton, // Flag to identify Giba header
            });
        },

        [] // Empty dependency array - function reference never changes
    );
};

export default useSetHeader;
