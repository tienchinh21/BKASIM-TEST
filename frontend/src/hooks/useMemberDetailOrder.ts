import { useState, useEffect } from "react";
import { zaloStorage } from "../utils/zaloStorage";

const STORAGE_KEYS = {
  CARD_ORDER: "memberDetailCardOrder",
  BASIC_INFO_ORDER: "memberDetailBasicItemOrder",
  COMPANY_INFO_ORDER: "memberDetailCompanyItemOrder",
};

const DEFAULT_ORDERS = {
  cards: ["basicInfo", "companyInfo", "fieldInfo", "customFields"],
  basicInfo: ["dayOfBirth", "address", "rating"],
  companyInfo: [
    "logo",
    "fullName",
    "brandName",
    "taxCode",
    "businessField",
    "businessType",
    "address",
    "website",
    "phone",
    "email",
    "representative",
    "position",
    "regNumber",
    "regDate",
    "regPlace",
  ],
};

export const useMemberDetailOrder = (isOwnProfile: boolean) => {
  const [cardOrder, setCardOrder] = useState<string[]>(DEFAULT_ORDERS.cards);
  const [basicInfoItemOrder, setBasicInfoItemOrder] = useState<string[]>(
    DEFAULT_ORDERS.basicInfo
  );
  const [companyInfoItemOrder, setCompanyInfoItemOrder] = useState<string[]>(
    DEFAULT_ORDERS.companyInfo
  );
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!isOwnProfile) {
      setIsLoading(false);
      return;
    }

    const loadOrders = async () => {
      try {
        const [cardResult, basicResult, companyResult] = await Promise.all([
          zaloStorage.get(STORAGE_KEYS.CARD_ORDER),
          zaloStorage.get(STORAGE_KEYS.BASIC_INFO_ORDER),
          zaloStorage.get(STORAGE_KEYS.COMPANY_INFO_ORDER),
        ]);

        if (cardResult.success && cardResult.data) {
          setCardOrder(cardResult.data);
        }
        if (basicResult.success && basicResult.data) {
          setBasicInfoItemOrder(basicResult.data);
        }
        if (companyResult.success && companyResult.data) {
          setCompanyInfoItemOrder(companyResult.data);
        }
      } catch (error) {
        console.error("Error loading orders:", error);
      } finally {
        setIsLoading(false);
      }
    };

    loadOrders();
  }, [isOwnProfile]);

  const saveCardOrder = async (newOrder: string[]) => {
    setCardOrder(newOrder);
    if (isOwnProfile) {
      const result = await zaloStorage.set(STORAGE_KEYS.CARD_ORDER, newOrder);
    }
  };

  const saveBasicInfoOrder = async (newOrder: string[]) => {
 
    setBasicInfoItemOrder(newOrder);
    if (isOwnProfile) {
      const result = await zaloStorage.set(STORAGE_KEYS.BASIC_INFO_ORDER, newOrder);
    }
  };

  const saveCompanyInfoOrder = async (newOrder: string[]) => {
    setCompanyInfoItemOrder(newOrder);
    if (isOwnProfile) {
      const result = await zaloStorage.set(STORAGE_KEYS.COMPANY_INFO_ORDER, newOrder);
    }
  };

  const resetOrders = async () => {
    setCardOrder(DEFAULT_ORDERS.cards);
    setBasicInfoItemOrder(DEFAULT_ORDERS.basicInfo);
    setCompanyInfoItemOrder(DEFAULT_ORDERS.companyInfo);

    if (isOwnProfile) {
      await Promise.all([
        zaloStorage.remove(STORAGE_KEYS.CARD_ORDER),
        zaloStorage.remove(STORAGE_KEYS.BASIC_INFO_ORDER),
        zaloStorage.remove(STORAGE_KEYS.COMPANY_INFO_ORDER),
      ]);
    }
  };

  return {
    cardOrder,
    basicInfoItemOrder,
    companyInfoItemOrder,
    isLoading,
    saveCardOrder,
    saveBasicInfoOrder,
    saveCompanyInfoOrder,
    resetOrders,
  };
};

