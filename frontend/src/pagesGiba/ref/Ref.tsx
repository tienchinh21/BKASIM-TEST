import React, { useState, useEffect } from "react";
import { Page } from "zmp-ui";
import { useLocation, useNavigate } from "react-router-dom";
import { useRecoilValue } from "recoil";
import { token, userMembershipInfo } from "../../recoil/RecoilState";
import { Input, Select, Radio } from "antd";
import {
  User,
  Users,
  FileText,
  Phone,
  Mail,
  MapPin,
  UserCircle,
} from "lucide-react";
import useSetHeader from "../../components/hooks/useSetHeader";
import RefOptionModal from "./RefOptionModal";
import { toast } from "react-toastify";
import axios from "axios";
import dfData from "../../common/DefaultConfig.json";

interface Group {
  id: string;
  name: string;
  description: string;
}

interface Member {
  id: string;
  userZaloId: string;
  memberName: string;
  zaloAvatar: string;
  company: string;
  position: string;
  phone: string;
  email: string;
  address: string;
}

type ShareType = "own" | "member" | "external";

interface RefFormDataType0 {
  type: 0;
  refTo: string;
  refToGroupId: string;
  shareType: ShareType;
  referredMemberId?: string;
  referredMemberGroupId?: string;
  referralName?: string;
  referralPhone?: string;
  referralEmail?: string;
  referralAddress?: string;
  content: string;
}

interface RefFormDataType1 {
  type: 1;
  shareType: ShareType;
  referredMemberId?: string;
  referredMemberGroupId?: string;
  referralName?: string;
  referralPhone?: string;
  referralEmail?: string;
  referralAddress?: string;
  recipientName: string;
  recipientPhone: string;
  content: string;
}

const RefCreate: React.FC = () => {
  const setHeader = useSetHeader();
  const location = useLocation();
  const navigate = useNavigate();
  const userToken = useRecoilValue(token);
  const currentUser = useRecoilValue(userMembershipInfo);

  const [showOptionModal, setShowOptionModal] = useState(false);
  const [currentOption, setCurrentOption] = useState<
    "option1" | "option2" | null
  >(null);
  const [loading, setLoading] = useState(false);
  const [loadingGroups, setLoadingGroups] = useState(false);
  const [loadingMembers, setLoadingMembers] = useState(false);
  const [loadingReferredMembers, setLoadingReferredMembers] = useState(false);
  const [groups, setGroups] = useState<Group[]>([]);
  const [members, setMembers] = useState<Member[]>([]);
  const [referredMembers, setReferredMembers] = useState<Member[]>([]);
  const [selectedMember, setSelectedMember] = useState<Member | null>(null);
  const [selectedReferredMember, setSelectedReferredMember] =
    useState<Member | null>(null);
  const [isPreFilled, setIsPreFilled] = useState(false);

  const [formDataType0, setFormDataType0] = useState<RefFormDataType0>({
    type: 0,
    refTo: "",
    refToGroupId: "",
    shareType: "external",
    content: "",
  });

  const [formDataType1, setFormDataType1] = useState<RefFormDataType1>({
    type: 1,
    shareType: "own",
    recipientName: "",
    recipientPhone: "",
    content: "",
  });

  useEffect(() => {
    const state = location.state as {
      optionType?: "option1" | "option2";
      groupId?: string;
      receiverId?: string;
      receiverName?: string;
    };
    if (state?.optionType) {
      setCurrentOption(state.optionType);

      // Pre-fill form if coming from MemberDetailGiba
      if (state.groupId && state.receiverId && state.optionType === "option1") {
        setFormDataType0((prev) => ({
          ...prev,
          refToGroupId: state.groupId!,
          refTo: state.receiverId!,
        }));
        setIsPreFilled(true);
      }
    } else {
      setShowOptionModal(true);
    }
  }, [location.state]);

  useEffect(() => {
    setHeader({
      title:
        currentOption === "option1"
          ? "REFERRAL CHO THÀNH VIÊN"
          : currentOption === "option2"
          ? "REFERRAL CHO NGƯỜI NGOÀI"
          : "TẠO REFERRAL",
      showUserInfo: false,
      showMenuButton: false,
      showCloseButton: false,
      hasLeftIcon: true,
    });
  }, [setHeader, currentOption]);

  useEffect(() => {
    if (currentOption) {
      loadGroups();
    }
  }, [currentOption]);

  useEffect(() => {
    if (currentOption === "option1" && formDataType0.refToGroupId) {
      loadMembers(formDataType0.refToGroupId, true);
    } else {
      setMembers([]);
      setSelectedMember(null);
    }
  }, [formDataType0.refToGroupId, currentOption]);

  useEffect(() => {
    if (
      currentOption === "option1" &&
      formDataType0.shareType === "member" &&
      formDataType0.referredMemberGroupId
    ) {
      loadReferredMembers(formDataType0.referredMemberGroupId);
    } else if (
      currentOption === "option2" &&
      formDataType1.shareType === "member" &&
      formDataType1.referredMemberGroupId
    ) {
      loadReferredMembers(formDataType1.referredMemberGroupId);
    } else {
      setReferredMembers([]);
      setSelectedReferredMember(null);
    }
  }, [
    formDataType0.shareType,
    formDataType0.referredMemberGroupId,
    formDataType1.shareType,
    formDataType1.referredMemberGroupId,
    currentOption,
  ]);

  useEffect(() => {
    if (currentOption === "option1" && formDataType0.refTo) {
      const member = members.find((m) => m.userZaloId === formDataType0.refTo);
      setSelectedMember(member || null);
    } else {
      setSelectedMember(null);
    }
  }, [formDataType0.refTo, members, currentOption]);

  useEffect(() => {
    if (currentOption === "option1" && formDataType0.referredMemberId) {
      const member = referredMembers.find(
        (m) => m.userZaloId === formDataType0.referredMemberId
      );
      setSelectedReferredMember(member || null);
    } else if (currentOption === "option2" && formDataType1.referredMemberId) {
      const member = referredMembers.find(
        (m) => m.userZaloId === formDataType1.referredMemberId
      );
      setSelectedReferredMember(member || null);
    } else {
      setSelectedReferredMember(null);
    }
  }, [
    formDataType0.referredMemberId,
    formDataType1.referredMemberId,
    referredMembers,
    currentOption,
  ]);

  const loadGroups = async () => {
    try {
      setLoadingGroups(true);
      const response = await axios.get(
        `${dfData.domain}/api/Groups/all-for-user`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 && response.data.data?.items) {
        const groupList = response.data.data.items.map((item: any) => ({
          id: item.id,
          name: item.groupName,
          description: item.description,
        }));
        setGroups(groupList);
      }
    } catch (error) {
      console.error("Error loading groups:", error);
      toast.error("Không thể tải danh sách Club");
    } finally {
      setLoadingGroups(false);
    }
  };

  const loadMembers = async (groupId: string, excludeSelf: boolean = false) => {
    try {
      setLoadingMembers(true);
      setMembers([]);
      const response = await axios.get(
        `${dfData.domain}/api/memberships/get-membership-by-group?groupId=${groupId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 && response.data.data?.membership) {
        let memberList = response.data.data.membership;
        // Loại bỏ chính mình khỏi danh sách người nhận ref
        if (excludeSelf && currentUser?.userZaloId) {
          memberList = memberList.filter(
            (m: Member) => m.userZaloId !== currentUser.userZaloId
          );
        }
        setMembers(memberList);
      }
    } catch (error) {
      console.error("Error loading members:", error);
      toast.error("Không thể tải danh sách thành viên");
    } finally {
      setLoadingMembers(false);
    }
  };

  const loadReferredMembers = async (groupId: string) => {
    try {
      setLoadingReferredMembers(true);
      setReferredMembers([]);
      const response = await axios.get(
        `${dfData.domain}/api/memberships/get-membership-by-group?groupId=${groupId}`,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
          },
        }
      );

      if (response.data.code === 0 && response.data.data?.membership) {
        setReferredMembers(response.data.data.membership);
      }
    } catch (error) {
      console.error("Error loading referred members:", error);
      toast.error("Không thể tải danh sách thành viên");
    } finally {
      setLoadingReferredMembers(false);
    }
  };

  const validateFormType0 = (): boolean => {
    if (!formDataType0.refToGroupId) {
      toast.error("Vui lòng chọn nhóm");
      return false;
    }
    if (!formDataType0.refTo) {
      toast.error("Vui lòng chọn thành viên nhận ref");
      return false;
    }
    // Không cho phép gửi ref cho chính mình
    if (formDataType0.refTo === currentUser?.userZaloId) {
      toast.error("Không thể gửi referral cho chính mình");
      return false;
    }

    if (formDataType0.shareType === "member") {
      if (!formDataType0.referredMemberGroupId) {
        toast.error("Vui lòng chọn nhóm của thành viên được chia sẻ");
        return false;
      }
      if (!formDataType0.referredMemberId) {
        toast.error("Vui lòng chọn thành viên được chia sẻ");
        return false;
      }
    } else if (formDataType0.shareType === "external") {
      if (
        !formDataType0.referralName?.trim() &&
        !formDataType0.referralPhone?.trim()
      ) {
        toast.error(
          "Vui lòng nhập ít nhất tên hoặc số điện thoại người được giới thiệu"
        );
        return false;
      }
      if (formDataType0.referralPhone?.trim()) {
        const phoneRegex = /^[0-9]{10,11}$/;
        if (!phoneRegex.test(formDataType0.referralPhone.replace(/\s/g, ""))) {
          toast.error("Số điện thoại không hợp lệ");
          return false;
        }
      }
    }
    // shareType === "own" không cần validation thêm

    return true;
  };

  const validateFormType1 = (): boolean => {
    if (
      !formDataType1.recipientName?.trim() &&
      !formDataType1.recipientPhone?.trim()
    ) {
      toast.error("Vui lòng nhập ít nhất tên hoặc số điện thoại người nhận");
      return false;
    }
    if (formDataType1.recipientPhone?.trim()) {
      const phoneRegex = /^[0-9]{10,11}$/;
      if (!phoneRegex.test(formDataType1.recipientPhone.replace(/\s/g, ""))) {
        toast.error("Số điện thoại người nhận không hợp lệ");
        return false;
      }
    }

    if (formDataType1.shareType === "member") {
      if (!formDataType1.referredMemberId) {
        toast.error("Vui lòng chọn thành viên được chia sẻ");
        return false;
      }
      if (!formDataType1.referredMemberGroupId) {
        toast.error("Vui lòng chọn nhóm của thành viên được chia sẻ");
        return false;
      }
    } else if (formDataType1.shareType === "external") {
      if (
        !formDataType1.referralName?.trim() &&
        !formDataType1.referralPhone?.trim()
      ) {
        toast.error(
          "Vui lòng nhập ít nhất tên hoặc số điện thoại người được giới thiệu"
        );
        return false;
      }
      if (formDataType1.referralPhone?.trim()) {
        const phoneRegex = /^[0-9]{10,11}$/;
        if (!phoneRegex.test(formDataType1.referralPhone.replace(/\s/g, ""))) {
          toast.error("Số điện thoại người được giới thiệu không hợp lệ");
          return false;
        }
      }
    }
    // shareType === "own" không cần validation thêm

    return true;
  };

  const handleSubmit = async () => {
    if (currentOption === "option1") {
      if (!validateFormType0()) return;
    } else if (currentOption === "option2") {
      if (!validateFormType1()) return;
    }

    try {
      setLoading(true);

      let payload: any;

      if (currentOption === "option1") {
        // Type 0: Gửi cho thành viên
        payload = {
          type: 0,
          refTo: formDataType0.refTo,
          refToGroupId: formDataType0.refToGroupId,
          shareType: formDataType0.shareType,
          content: formDataType0.content || "",
        };

        if (formDataType0.shareType === "own") {
          // Không cần thêm field nào, backend sẽ tự lấy userZaloId của người gửi
        } else if (formDataType0.shareType === "member") {
          payload.referredMemberId = formDataType0.referredMemberId;
          payload.referredMemberGroupId = formDataType0.referredMemberGroupId;
        } else if (formDataType0.shareType === "external") {
          if (formDataType0.referralName)
            payload.referralName = formDataType0.referralName;
          if (formDataType0.referralPhone)
            payload.referralPhone = formDataType0.referralPhone;
          if (formDataType0.referralEmail)
            payload.referralEmail = formDataType0.referralEmail;
          if (formDataType0.referralAddress)
            payload.referralAddress = formDataType0.referralAddress;
        }
      } else {
        // Type 1: Gửi cho người ngoài
        payload = {
          type: 1,
          shareType: formDataType1.shareType,
          recipientName: formDataType1.recipientName || "",
          recipientPhone: formDataType1.recipientPhone || "",
          content: formDataType1.content || "",
        };

        if (formDataType1.shareType === "own") {
          // Không cần thêm field nào, backend sẽ tự lấy userZaloId của người gửi
        } else if (formDataType1.shareType === "member") {
          payload.referredMemberId = formDataType1.referredMemberId;
          payload.referredMemberGroupId = formDataType1.referredMemberGroupId;
        } else if (formDataType1.shareType === "external") {
          if (formDataType1.referralName)
            payload.referralName = formDataType1.referralName;
          if (formDataType1.referralPhone)
            payload.referralPhone = formDataType1.referralPhone;
          if (formDataType1.referralEmail)
            payload.referralEmail = formDataType1.referralEmail;
          if (formDataType1.referralAddress)
            payload.referralAddress = formDataType1.referralAddress;
        }
      }

      const response = await axios.post(
        `${dfData.domain}/api/refs/create`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${userToken}`,
            "Content-Type": "application/json",
          },
        }
      );

      if (response.data.code === 0) {
        toast.success("Tạo referral thành công!");
        navigate(-1);
      } else {
        toast.error(response.data.message || "Có lỗi xảy ra");
      }
    } catch (error: any) {
      console.error("Error creating ref:", error);
      toast.error(
        error.response?.data?.message || "Có lỗi xảy ra khi tạo referral"
      );
    } finally {
      setLoading(false);
    }
  };

  const handleSelectOption = (option: "option1" | "option2") => {
    setShowOptionModal(false);
    setCurrentOption(option);
  };

  const handleCloseModal = () => {
    setShowOptionModal(false);
    navigate(-1);
  };

  const renderType0Form = () => (
    <div className="p-4 space-y-4 pb-24">
      <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <Users size={20} className="text-gray-700" />
          <h3 className="font-semibold text-base text-gray-900">
            Thông tin nhóm và người nhận
          </h3>
        </div>

        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">
              Nhóm người nhận <span className="text-red-500">*</span>
            </label>
            <Select
              placeholder="Chọn nhóm"
              value={formDataType0.refToGroupId || undefined}
              onChange={(value) =>
                setFormDataType0({
                  ...formDataType0,
                  refToGroupId: value,
                  refTo: "",
                })
              }
              style={{ width: "100%" }}
              size="large"
              loading={loadingGroups}
              disabled={loadingGroups || isPreFilled}
            >
              {groups.map((group) => (
                <Select.Option key={group.id} value={group.id}>
                  {group.name}
                </Select.Option>
              ))}
            </Select>
          </div>

          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">
              Thành viên nhận referral <span className="text-red-500">*</span>
            </label>
            <Select
              placeholder="Chọn thành viên"
              value={formDataType0.refTo || undefined}
              onChange={(value) =>
                setFormDataType0({ ...formDataType0, refTo: value })
              }
              style={{ width: "100%" }}
              size="large"
              loading={loadingMembers}
              disabled={
                !formDataType0.refToGroupId || loadingMembers || isPreFilled
              }
            >
              {members.map((member) => (
                <Select.Option key={member.id} value={member.userZaloId}>
                  <div className="flex items-center gap-2">
                    <img
                      src={member.zaloAvatar}
                      alt={member.memberName}
                      className="w-6 h-6 rounded-full"
                    />
                    <div>
                      <div className="text-sm font-medium">
                        {member.memberName}
                      </div>
                      <div className="text-xs text-gray-500">
                        {member.position} - {member.company}
                      </div>
                    </div>
                  </div>
                </Select.Option>
              ))}
            </Select>
          </div>

          {selectedMember && (
            <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
              <div className="flex items-center gap-3">
                <img
                  src={selectedMember.zaloAvatar}
                  alt={selectedMember.memberName}
                  className="w-12 h-12 rounded-full border-2 border-gray-300"
                />
                <div className="flex-1">
                  <div className="font-semibold text-sm text-gray-900">
                    {selectedMember.memberName}
                  </div>
                  <div className="text-xs text-gray-600">
                    {selectedMember.position}
                  </div>
                  <div className="text-xs text-gray-500">
                    {selectedMember.company}
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>

      <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <UserCircle size={20} className="text-gray-700" />
          <h3 className="font-semibold text-base text-gray-900">
            Loại nội dung chia sẻ
          </h3>
        </div>

        <Radio.Group
          value={formDataType0.shareType}
          onChange={(e) =>
            setFormDataType0({
              ...formDataType0,
              shareType: e.target.value,
              referredMemberId: undefined,
              referredMemberGroupId: undefined,
              referralName: undefined,
              referralPhone: undefined,
              referralEmail: undefined,
              referralAddress: undefined,
            })
          }
          className="w-full"
        >
          <div className="space-y-3">
            <Radio value="own" className="w-full block">
              <div>
                <div className="font-medium text-gray-900">
                  Chia sẻ profile bản thân
                </div>
                <div className="text-xs text-gray-500">
                  Giới thiệu profile của bạn
                </div>
              </div>
            </Radio>
            <Radio value="member" className="w-full block">
              <div>
                <div className="font-medium text-gray-900">
                  Chia sẻ profile thành viên
                </div>
                <div className="text-xs text-gray-500">
                  Giới thiệu một thành viên khác trong nhóm
                </div>
              </div>
            </Radio>
            <Radio value="external" className="w-full block">
              <div>
                <div className="font-medium text-gray-900">
                  Nhập thông tin người ngoài
                </div>
                <div className="text-xs text-gray-500">
                  Soạn thông tin người được giới thiệu
                </div>
              </div>
            </Radio>
          </div>
        </Radio.Group>
      </div>

      {formDataType0.shareType === "member" && (
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Users size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Thành viên được chia sẻ
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Nhóm của thành viên <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn nhóm"
                value={formDataType0.referredMemberGroupId || undefined}
                onChange={(value) =>
                  setFormDataType0({
                    ...formDataType0,
                    referredMemberGroupId: value,
                    referredMemberId: undefined,
                  })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingGroups}
                disabled={loadingGroups}
              >
                {groups.map((group) => (
                  <Select.Option key={group.id} value={group.id}>
                    {group.name}
                  </Select.Option>
                ))}
              </Select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Thành viên <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn thành viên"
                value={formDataType0.referredMemberId || undefined}
                onChange={(value) =>
                  setFormDataType0({
                    ...formDataType0,
                    referredMemberId: value,
                  })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingReferredMembers}
                disabled={
                  !formDataType0.referredMemberGroupId || loadingReferredMembers
                }
              >
                {referredMembers.map((member) => (
                  <Select.Option key={member.id} value={member.userZaloId}>
                    <div className="flex items-center gap-2">
                      <img
                        src={member.zaloAvatar}
                        alt={member.memberName}
                        className="w-6 h-6 rounded-full"
                      />
                      <div>
                        <div className="text-sm font-medium">
                          {member.memberName}
                        </div>
                        <div className="text-xs text-gray-500">
                          {member.position} - {member.company}
                        </div>
                      </div>
                    </div>
                  </Select.Option>
                ))}
              </Select>
            </div>

            {selectedReferredMember && (
              <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
                <div className="flex items-center gap-3">
                  <img
                    src={selectedReferredMember.zaloAvatar}
                    alt={selectedReferredMember.memberName}
                    className="w-12 h-12 rounded-full border-2 border-gray-300"
                  />
                  <div className="flex-1">
                    <div className="font-semibold text-sm text-gray-900">
                      {selectedReferredMember.memberName}
                    </div>
                    <div className="text-xs text-gray-600">
                      {selectedReferredMember.position}
                    </div>
                    <div className="text-xs text-gray-500">
                      {selectedReferredMember.company}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {formDataType0.shareType === "external" && (
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <User size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Thông tin người được giới thiệu
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Họ tên
              </label>
              <Input
                prefix={<User size={16} className="text-gray-400" />}
                placeholder="Nhập họ tên"
                value={formDataType0.referralName || ""}
                onChange={(e) =>
                  setFormDataType0({
                    ...formDataType0,
                    referralName: e.target.value,
                  })
                }
                size="large"
                maxLength={100}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Số điện thoại
              </label>
              <Input
                prefix={<Phone size={16} className="text-gray-400" />}
                placeholder="Nhập số điện thoại"
                value={formDataType0.referralPhone || ""}
                onChange={(e) =>
                  setFormDataType0({
                    ...formDataType0,
                    referralPhone: e.target.value,
                  })
                }
                size="large"
                maxLength={15}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Email
              </label>
              <Input
                prefix={<Mail size={16} className="text-gray-400" />}
                placeholder="Nhập email"
                value={formDataType0.referralEmail || ""}
                onChange={(e) =>
                  setFormDataType0({
                    ...formDataType0,
                    referralEmail: e.target.value,
                  })
                }
                size="large"
                maxLength={100}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Địa chỉ
              </label>
              <Input.TextArea
                placeholder="Nhập địa chỉ"
                value={formDataType0.referralAddress || ""}
                onChange={(e) =>
                  setFormDataType0({
                    ...formDataType0,
                    referralAddress: e.target.value,
                  })
                }
                rows={2}
                maxLength={200}
                showCount
              />
            </div>
          </div>
        </div>
      )}

      {formDataType0.shareType === "own" && (
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <UserCircle size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Profile bản thân
            </h3>
          </div>
          <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
            <div className="text-sm text-gray-700">
              Bạn sẽ chia sẻ profile của chính mình cho thành viên này.
            </div>
          </div>
        </div>
      )}

      <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <FileText size={20} className="text-gray-700" />
          <h3 className="font-semibold text-base text-gray-900">Ghi chú</h3>
        </div>

        <div>
          <Input.TextArea
            placeholder="Nhập ghi chú, mô tả về người được giới thiệu..."
            value={formDataType0.content}
            onChange={(e) =>
              setFormDataType0({ ...formDataType0, content: e.target.value })
            }
            rows={4}
            maxLength={1000}
            showCount
          />
        </div>
      </div>
    </div>
  );

  const renderType1Form = () => (
    <div className="p-4 space-y-4 pb-24">
      <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <UserCircle size={20} className="text-gray-700" />
          <h3 className="font-semibold text-base text-gray-900">
            Loại nội dung chia sẻ
          </h3>
        </div>

        <Radio.Group
          value={formDataType1.shareType}
          onChange={(e) =>
            setFormDataType1({
              ...formDataType1,
              shareType: e.target.value,
              referredMemberId: undefined,
              referredMemberGroupId: undefined,
              referralName: undefined,
              referralPhone: undefined,
              referralEmail: undefined,
              referralAddress: undefined,
            })
          }
          className="w-full"
        >
          <div className="space-y-3">
            <Radio value="own" className="w-full block">
              <div>
                <div className="font-medium text-gray-900">
                  Chia sẻ profile bản thân
                </div>
                <div className="text-xs text-gray-500">
                  Giới thiệu profile của bạn
                </div>
              </div>
            </Radio>
            <Radio value="member" className="w-full block">
              <div>
                <div className="font-medium text-gray-900">
                  Chia sẻ profile thành viên
                </div>
                <div className="text-xs text-gray-500">
                  Giới thiệu một thành viên trong nhóm
                </div>
              </div>
            </Radio>
            <Radio value="external" className="w-full block">
              <div>
                <div className="font-medium text-gray-900">
                  Nhập thông tin người ngoài
                </div>
                <div className="text-xs text-gray-500">
                  Soạn thông tin người được giới thiệu
                </div>
              </div>
            </Radio>
          </div>
        </Radio.Group>
      </div>

      {formDataType1.shareType === "member" && (
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <Users size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Thành viên được chia sẻ
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Nhóm của thành viên <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn nhóm"
                value={formDataType1.referredMemberGroupId || undefined}
                onChange={(value) =>
                  setFormDataType1({
                    ...formDataType1,
                    referredMemberGroupId: value,
                    referredMemberId: undefined,
                  })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingGroups}
                disabled={loadingGroups}
              >
                {groups.map((group) => (
                  <Select.Option key={group.id} value={group.id}>
                    {group.name}
                  </Select.Option>
                ))}
              </Select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Thành viên <span className="text-red-500">*</span>
              </label>
              <Select
                placeholder="Chọn thành viên"
                value={formDataType1.referredMemberId || undefined}
                onChange={(value) =>
                  setFormDataType1({
                    ...formDataType1,
                    referredMemberId: value,
                  })
                }
                style={{ width: "100%" }}
                size="large"
                loading={loadingReferredMembers}
                disabled={
                  !formDataType1.referredMemberGroupId || loadingReferredMembers
                }
              >
                {referredMembers.map((member) => (
                  <Select.Option key={member.id} value={member.userZaloId}>
                    <div className="flex items-center gap-2">
                      <img
                        src={member.zaloAvatar}
                        alt={member.memberName}
                        className="w-6 h-6 rounded-full"
                      />
                      <div>
                        <div className="text-sm font-medium">
                          {member.memberName}
                        </div>
                        <div className="text-xs text-gray-500">
                          {member.position} - {member.company}
                        </div>
                      </div>
                    </div>
                  </Select.Option>
                ))}
              </Select>
            </div>

            {selectedReferredMember && (
              <div className="bg-gray-50 p-3 rounded-lg border border-gray-200">
                <div className="flex items-center gap-3">
                  <img
                    src={selectedReferredMember.zaloAvatar}
                    alt={selectedReferredMember.memberName}
                    className="w-12 h-12 rounded-full border-2 border-gray-300"
                  />
                  <div className="flex-1">
                    <div className="font-semibold text-sm text-gray-900">
                      {selectedReferredMember.memberName}
                    </div>
                    <div className="text-xs text-gray-600">
                      {selectedReferredMember.position}
                    </div>
                    <div className="text-xs text-gray-500">
                      {selectedReferredMember.company}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}

      {formDataType1.shareType === "external" && (
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <User size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Thông tin người được giới thiệu
            </h3>
          </div>

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Họ tên
              </label>
              <Input
                prefix={<User size={16} className="text-gray-400" />}
                placeholder="Nhập họ tên"
                value={formDataType1.referralName || ""}
                onChange={(e) =>
                  setFormDataType1({
                    ...formDataType1,
                    referralName: e.target.value,
                  })
                }
                size="large"
                maxLength={100}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Số điện thoại
              </label>
              <Input
                prefix={<Phone size={16} className="text-gray-400" />}
                placeholder="Nhập số điện thoại"
                value={formDataType1.referralPhone || ""}
                onChange={(e) =>
                  setFormDataType1({
                    ...formDataType1,
                    referralPhone: e.target.value,
                  })
                }
                size="large"
                maxLength={15}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Email
              </label>
              <Input
                prefix={<Mail size={16} className="text-gray-400" />}
                placeholder="Nhập email"
                value={formDataType1.referralEmail || ""}
                onChange={(e) =>
                  setFormDataType1({
                    ...formDataType1,
                    referralEmail: e.target.value,
                  })
                }
                size="large"
                maxLength={100}
              />
            </div>

            <div>
              <label className="block text-sm font-medium mb-2 text-gray-700">
                Địa chỉ
              </label>
              <Input.TextArea
                placeholder="Nhập địa chỉ"
                value={formDataType1.referralAddress || ""}
                onChange={(e) =>
                  setFormDataType1({
                    ...formDataType1,
                    referralAddress: e.target.value,
                  })
                }
                rows={2}
                maxLength={200}
                showCount
              />
            </div>
          </div>
        </div>
      )}

      {formDataType1.shareType === "own" && (
        <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
          <div className="flex items-center gap-2 mb-4">
            <UserCircle size={20} className="text-gray-700" />
            <h3 className="font-semibold text-base text-gray-900">
              Profile bản thân
            </h3>
          </div>
          <div className="bg-blue-50 p-4 rounded-lg border border-blue-200">
            <div className="text-sm text-gray-700">
              Bạn sẽ chia sẻ profile của chính mình cho người nhận bên ngoài.
            </div>
          </div>
        </div>
      )}

      <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <User size={20} className="text-gray-700" />
          <h3 className="font-semibold text-base text-gray-900">
            Thông tin người nhận bên ngoài
          </h3>
        </div>

        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">
              Họ tên
            </label>
            <Input
              prefix={<User size={16} className="text-gray-400" />}
              placeholder="Nhập họ tên người nhận"
              value={formDataType1.recipientName}
              onChange={(e) =>
                setFormDataType1({
                  ...formDataType1,
                  recipientName: e.target.value,
                })
              }
              size="large"
              maxLength={100}
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-2 text-gray-700">
              Số điện thoại
            </label>
            <Input
              prefix={<Phone size={16} className="text-gray-400" />}
              placeholder="Nhập số điện thoại người nhận"
              value={formDataType1.recipientPhone}
              onChange={(e) =>
                setFormDataType1({
                  ...formDataType1,
                  recipientPhone: e.target.value,
                })
              }
              size="large"
              maxLength={15}
            />
          </div>
        </div>
      </div>

      <div className="bg-white rounded-xl p-5 border border-gray-200 shadow-sm">
        <div className="flex items-center gap-2 mb-4">
          <FileText size={20} className="text-gray-700" />
          <h3 className="font-semibold text-base text-gray-900">Ghi chú</h3>
        </div>

        <div>
          <Input.TextArea
            placeholder="Nhập ghi chú về việc chia sẻ thông tin..."
            value={formDataType1.content}
            onChange={(e) =>
              setFormDataType1({ ...formDataType1, content: e.target.value })
            }
            rows={4}
            maxLength={1000}
            showCount
          />
        </div>
      </div>
    </div>
  );

  return (
    <Page
      style={{ marginTop: "50px", background: "#f9fafb", minHeight: "100vh" }}
    >
      {!currentOption && (
        <div className="flex items-center justify-center h-[calc(100vh-50px)]">
          <div className="text-center">
            <div className="w-20 h-20 bg-gradient-to-br from-yellow-100 to-yellow-200 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg
                className="w-10 h-10 text-yellow-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4v16m8-8H4"
                />
              </svg>
            </div>
            <div className="text-gray-800 text-lg font-bold mb-2">
              Tạo referral Mới
            </div>
            <div className="text-gray-500 text-sm">
              Chọn loại referral bạn muốn tạo
            </div>
          </div>
        </div>
      )}

      {currentOption === "option1" && renderType0Form()}
      {currentOption === "option2" && renderType1Form()}

      {currentOption && (
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 p-4 flex gap-3 shadow-lg z-50">
          <button
            onClick={() => navigate(-1)}
            className="flex-1 px-4 py-2 bg-white border border-gray-300 text-gray-700 rounded-lg font-medium text-base transition-all hover:bg-gray-50"
            disabled={loading}
          >
            Hủy
          </button>
          <button
            onClick={handleSubmit}
            className="flex-1 px-4 py-2 bg-black text-white rounded-lg font-semibold text-base transition-all hover:bg-gray-800 disabled:opacity-50 disabled:cursor-not-allowed"
            disabled={loading}
          >
            {loading ? "Đang xử lý..." : "Tạo Referral"}
          </button>
        </div>
      )}

      <RefOptionModal
        visible={showOptionModal}
        onClose={handleCloseModal}
        onSelectOption={handleSelectOption}
      />
    </Page>
  );
};

export default RefCreate;
