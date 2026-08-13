import type { UserRole } from "../../../types/app.types";

export type ReportStatus = "Open" | "InReview" | "Resolved" | "Rejected";

export interface AdminUser {
  userId: string;
  username: string;
  email: string;
  role: UserRole;
  emailVerified: boolean;
  createdAt: string;
  deletedAt?: string;
}

export interface AdminAsset {
  assetId: string;
  title: string;
  authorUsername: string;
  assetTypeName: string;
  createdAt: string;
  commentCount: number;
  reportCount: number;
}

export interface AdminComment {
  commentId: string;
  authorUsername: string;
  assetId: string;
  content: string;
  createdAt: string;
  reportCount: number;
}

export interface AdminReport {
  reportId: string;
  reporterUsername: string;
  reason: string;
  status: ReportStatus;
  assetId?: string;
  commentId?: string;
  createdAt: string;
  resolvedAt?: string;
}

export interface CreateUserByAdminRequest {
  email: string;
  username: string;
  displayName: string;
  role: UserRole;
}

export interface ResolveReportRequest {
  reportId: string;
  newStatus: ReportStatus;
}
