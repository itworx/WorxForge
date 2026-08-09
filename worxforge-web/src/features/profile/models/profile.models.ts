import type { AssetSummary } from "../../assets/models/assets.models";

export interface MyProfile {
  userId: string;
  username: string;
  displayName: string;
  email: string;
  emailVerified: boolean;
  avatarUrl?: string;
  createdAt: string;
  publishedAssets: AssetSummary[];
  statistics: ProfileStatistics;
}

export interface PublicProfile {
  userId: string;
  displayName: string;
  avatarUrl?: string;
  publishedAssets: AssetSummary[];
  followerCount: number;
  statistics: ProfileStatistics;
}

export interface ProfileStatistics {
  totalPublishedAssets: number;
  totalFollowers: number;
  totalReactionsReceived: number;
}

export interface UpdateProfileRequest {
  displayName: string;
  username: string;
  avatarUrl?: string;
}
