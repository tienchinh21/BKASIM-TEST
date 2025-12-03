// Import Recoil
import { atom } from "recoil";

export const phoneNumberUser = atom({
    key: "phoneNumberUser", // tên duy nhất cho atom
    default: "", // giá trị mặc định là chuỗi rỗng
});

export const actionTab = atom({
    key: "actionTab", // tên duy nhất cho atom
    default: "/", // giá trị mặc định là chuỗi rỗng
});

export const infoUser = atom({
    key: "infoUser", // unique ID (with respect to other atoms/selectors)
    default: {}, // default value (can be any valid JavaScript value)
});


export const checkURLLink = atom({
    key: "checkURLLink",
    default: {
        isLink: false,
        url: "",
    },
});

export const headerState = atom({
    key: "headerState",
    default: {},
});

export const token = atom({
    key: "token",
    default: "",
});

export const orderList = atom({
    key: "orderList",
    default: [],
});

export const amount = atom({
    key: "amount",
    default: 1,
});

export const totalPrice = atom({
    key: "totalPrice",
    default: 0,
});

export const voucherGlobal = atom({
    key: "voucherGlobal",
    default: "",
});

export const pointVoucher = atom({
    key: "pointVoucher",
    default: "",
});

export const keyActice = atom({
    key: "keyActice",
    default: "",
});

export const infoShare = atom({
    key: "infoShare",
    default: {},
});

export const price = atom({
    key: "price",
    default: {
        total: 0,
        discount: 0,
        shippingFee: 0,
    },
});

export const isRegister = atom({
    key: "isRegister",
    default: false,
});

export const codeAffiliate = atom({
    key: "codeAffiliate",
    default: "",
});

export const userMembershipInfo = atom({
    key: "userMembershipInfo",
    default: {
        id: "",
        fullname: "",
        phoneNumber: "",
        approvalStatus: null, // null | 0 (Chờ duyệt) | 1 (Đã duyệt) | 2 (Từ chối)
        userZaloId: "",
        idByOA: "",
    },
});

export const isShareLocation = atom({
    key: "isShareLocation",
    default: false,
});

export const selectedVouchersState = atom({
    key: "selectedVouchersState",
    default: [],
});

export const storeCurrent = atom({
    key: "storeCurrent",
    default: {},
});

export const cartState = atom({
    key: "cartState",
    default: 0,
});

export const defaultAddressState = atom({
    key: "defaultAddressState",
    default: {
        id: "",
        name: "",
        phoneNumber: "",
        fullAddress: "",
    },
});

export const noteState = atom({
    key: "noteState",
    default: "",
});

export const serviceBookingSelected = atom({
    key: "serviceBookingSelected",
    default: [],
});

export const source = atom({
    key: "sourceRef",
    default: null,
});

export const featureConfig = atom({
    key: "featureConfig",
    default: [],
});

export const configButtonHome = atom({
    key: "configButtonHome",
    default: {},
});

export const defaultMembershipVAT = atom({
    key: "defaultMembershipVAT",
    default: {
        id: "",
        taxCode: "",
        ownerName: "",
        email: "",
        fullAddress: "",
    },
});

export const productDataCache = atom({
    key: "productDataCache",
    default: {
        listProduct: [],
        listCategory: [],
        filterSearch: {
            page: 1,
            pageSize: 10,
            type: "sanpham",
            sort: "asc",
            keyword: "",
            CategoryId: "",
            CategoryChildId: "",
            minPrice: null,
            maxPrice: null,
            isGift: false,
            branchId: null,
        },
        scrollTop: 0,
        timestamp: null,
        totalPages: 1,
    },
});

export const newsDataCache = atom({
    key: "newsDataCache",
    default: {
        listNews: [],
        listCategory: [],
        filterSearch: {
            page: 1,
            pageSize: 10,
            keyword: "",
            categoryId: "",
        },
        scrollTop: 0,
        timestamp: null,
        totalPages: 1,
    },
});


export const phoneNumberLoading = atom({
    key: "phoneNumberLoading",
    default: true,
});

// Booking Form Cache
export const bookingDateTime = atom({
    key: 'bookingDateTime',
    default: null,
});

export const bookingNote = atom({
    key: 'bookingNote',
    default: '',
});


export const isFollowedOA = atom({
    key: 'isFollowedOA',
    default: false,
})


export const groupsDataCache = atom({
    key: 'groupsDataCache',
    default: {
        listGroups: [],
        filterSearch: {
            page: 1,
            pageSize: 10,
            keyword: '',
            groupType: '',
            joinStatus: '',
        },
        scrollTop: 0,
        timestamp: null,
        totalPages: 1,
    },
});

export const groupJoinRequestCache = atom({
    key: 'groupJoinRequestCache',
    default: {
        listRequests: [],
        filterSearch: {
            page: 1,
            pageSize: 10,
            statusFilter: '',
        },
        scrollTop: 0,
        timestamp: null,
        totalPages: 1,
    },
});

export const newsDataCacheGiba = atom({
    key: 'newsDataCacheGiba',
    default: {
        listNews: [],
        filterSearch: {
            page: 1,
            pageSize: 10,
            keyword: '',
            categoryId: '',
            groupType: '',
        },
        scrollTop: 0,
        timestamp: null,
        totalPages: 1,
    },
});