import type { LucideIcon } from "lucide-react";
import { Bell, Compass, House, Library, Package, Plus, User } from "lucide-react";
import { ROUTES } from "../constants/routes.constants";

export type NavigationGroup = "primary" | "account";

export type NavigationMatch = "exact" | "prefix";

export type NavigationItem = {
  label: string;
  path: string;
  icon: LucideIcon;
  group: NavigationGroup;
  match?: NavigationMatch;
  excludePaths?: string[];
  emphasized?: boolean;
  showNotificationBadge?: boolean;
};

export const navigationItems: NavigationItem[] = [
  {
    label: "Publish Asset",
    path: ROUTES.MY_ASSETS_CREATE,
    icon: Plus,
    group: "primary",
    match: "exact",
    emphasized: true
  },
  {
    label: "Home",
    path: ROUTES.HOME,
    icon: House,
    group: "primary",
    match: "exact"
  },
  {
    label: "Explore",
    path: ROUTES.EXPLORE,
    icon: Compass,
    group: "primary",
    match: "exact"
  },
  {
    label: "My Library",
    path: ROUTES.MY_LIBRARY,
    icon: Library,
    group: "primary",
    match: "exact"
  },
  {
    label: "My Assets",
    path: ROUTES.MY_ASSETS,
    icon: Package,
    group: "primary",
    match: "prefix",
    excludePaths: [ROUTES.MY_ASSETS_CREATE]
  },
  {
    label: "Notifications",
    path: ROUTES.NOTIFICATIONS,
    icon: Bell,
    group: "primary",
    match: "exact",
    showNotificationBadge: true
  },
  {
    label: "Profile",
    path: ROUTES.PROFILE_ME,
    icon: User,
    group: "account",
    match: "exact"
  }
];

