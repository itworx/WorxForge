import type { UserRole } from "../../../types/app.types";

export interface AuthToken {
  accessToken: string;
}

export interface UserSession {
  userId: string;
  displayName: string;
  role: UserRole;
  avatarUrl?: string;
}
