import { useRecoilValue } from "recoil";
import { infoUser, userMembershipInfo } from "../recoil/RecoilState";

const VALID_ROLES = ["GIBA", "NBD", "Club", "Group"];

const ALL_MANAGEMENT_ROLES = ["GIBA", "NBD", "Club", "Group"];

export const useHasRole = (): boolean => {
  const userInfo = useRecoilValue(infoUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  //@ts-ignore
  const roleName = membershipInfo?.roleName || userInfo?.roleName;

  if (!roleName || roleName.trim() === "") {
    return false;
  }

  return VALID_ROLES.includes(roleName);
};

export const useGetUserRole = (): string => {
  const userInfo = useRecoilValue(infoUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  //@ts-ignore
  return membershipInfo?.roleName || userInfo?.roleName || "";
};

export const useHasManagementRole = (): boolean => {
  const userInfo = useRecoilValue(infoUser);
  const membershipInfo = useRecoilValue(userMembershipInfo);
  //@ts-ignore
  const roleName = membershipInfo?.roleName || userInfo?.roleName;

  if (!roleName || roleName.trim() === "") {
    return false;
  }

  return ALL_MANAGEMENT_ROLES.includes(roleName);
};
