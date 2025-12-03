export interface BasicInfo {
  id: string;
  userZaloId: string;
  slug: string;
  createdDate: string;
  updatedDate: string;
  userZaloName: string;
  fullName: string;
  phoneNumber: string;
  email: string;
  zaloAvatar: string;
  profile: string;
  dayOfBirth: string;
  address: string;
  company: string;
  position: string;
  oldSlugs: string;
  code?: string;
}

export interface RatingInfo {
  averageRating: number;
  totalRatings: number;
}

export interface StatusInfo {
  approvalStatus: number;
  approvalReason: string | null;
  approvedDate: string;
  approvedBy: string;
}

export interface FieldDetail {
  id: string;
  name: string;
  isActive: boolean;
  parentId: string | null;
}

export interface FieldInfo {
  fieldIds: string;
  fieldDetails: FieldDetail[];
  fieldNames: string[];
}

export interface GroupInfo {
  groupId: string;
  groupName: string;
  groupPosition?: string;
  sortOrder?: number;
}

export interface CompanyInfo {
  companyFullName: string;
  companyBrandName: string;
  taxCode: string;
  businessField: string;
  businessType: string;
  headquartersAddress: string;
  companyWebsite: string;
  companyPhoneNumber: string;
  companyEmail: string;
  legalRepresentative: string;
  legalRepresentativePosition: string;
  companyLogo: string;
  businessRegistrationNumber: string;
  businessRegistrationDate: string;
  businessRegistrationPlace: string;
}

export interface CustomField {
  id: string;
  fieldName: string;
  fieldValue: string;
  fieldType: string;
  displayOrder: number;
  isVisible: boolean;
}

export interface ProfileTemplate {
  visibleFields: string[];
  hiddenFields: string[];
  customDescription: string;
  coverImage: string;
  themeColor: string;
  isPublic: boolean;
  customFields: CustomField[];
}

export interface MemberDetail {
  basicInfo: BasicInfo;
  ratingInfo: RatingInfo;
  statusInfo: StatusInfo;
  fieldInfo: FieldInfo;
  groupInfo: GroupInfo | GroupInfo[];
  companyInfo: CompanyInfo;
  profileTemplate: ProfileTemplate;
}

