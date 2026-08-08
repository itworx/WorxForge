import { createBrowserRouter } from "react-router-dom";
import { MainLayout } from "../layouts/MainLayout";
import { AdminLayout } from "../layouts/AdminLayout";
import { AuthLayout } from "../layouts/AuthLayout";
import { ProtectedRoute } from "./ProtectedRoute";
import { AdminRoute } from "./AdminRoute";
import { LoginPage } from "../features/authentication/pages/LoginPage";
import { RegisterPage } from "../features/authentication/pages/RegisterPage";
import { VerifyEmailPage } from "../features/authentication/pages/VerifyEmailPage";
import { ForgotPasswordPage } from "../features/authentication/pages/ForgotPasswordPage";
import { ResetPasswordPage } from "../features/authentication/pages/ResetPasswordPage";
import { HomePage } from "../features/home/pages/HomePage";
import { ExplorePage } from "../features/explore/pages/ExplorePage";
import { AssetDetailsPage } from "../features/assets/pages/AssetDetailsPage";
import { MyProfilePage } from "../features/profile/pages/MyProfilePage";
import { PublicProfilePage } from "../features/profile/pages/PublicProfilePage";
import { SettingsPage } from "../features/settings/pages/SettingsPage";
import { LibraryPage } from "../features/library/pages/LibraryPage";
import { MyAssetsPage } from "../features/assets/pages/MyAssetsPage";
import { NotificationsPage } from "../features/notifications/pages/NotificationsPage";
import { AdminUsersPage } from "../features/admin/pages/AdminUsersPage";
import { AdminAssetsPage } from "../features/admin/pages/AdminAssetsPage";
import { AdminCommentsPage } from "../features/admin/pages/AdminCommentsPage";
import { AdminReportsPage } from "../features/admin/pages/AdminReportsPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { UnauthorizedPage } from "../pages/UnauthorizedPage";
import { ErrorPage } from "../pages/ErrorPage";

export const appRouter = createBrowserRouter([
  {
    path: "/auth",
    element: <AuthLayout />,
    children: [
      { path: "login", element: <LoginPage /> },
      { path: "register", element: <RegisterPage /> },
      { path: "verify-email", element: <VerifyEmailPage /> },
      { path: "forgot-password", element: <ForgotPasswordPage /> },
      { path: "reset-password/:token", element: <ResetPasswordPage /> }
    ]
  },
  {
    path: "/",
    element: <MainLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "home", element: <ProtectedRoute><HomePage /></ProtectedRoute> },
      { path: "explore", element: <ProtectedRoute><ExplorePage /></ProtectedRoute> },
      { path: "asset/:assetId", element: <ProtectedRoute><AssetDetailsPage /></ProtectedRoute> },
      { path: "profile/me", element: <ProtectedRoute><MyProfilePage /></ProtectedRoute> },
      { path: "profile/:userId", element: <ProtectedRoute><PublicProfilePage /></ProtectedRoute> },
      { path: "settings", element: <ProtectedRoute><SettingsPage /></ProtectedRoute> },
      { path: "my-library", element: <ProtectedRoute><LibraryPage /></ProtectedRoute> },
      { path: "my-assets", element: <ProtectedRoute><MyAssetsPage /></ProtectedRoute> },
      { path: "notifications", element: <ProtectedRoute><NotificationsPage /></ProtectedRoute> }
    ]
  },
  {
    path: "/admin",
    element: <AdminRoute><AdminLayout /></AdminRoute>,
    children: [
      { path: "users", element: <AdminUsersPage /> },
      { path: "assets", element: <AdminAssetsPage /> },
      { path: "comments", element: <AdminCommentsPage /> },
      { path: "reports", element: <AdminReportsPage /> }
    ]
  },
  { path: "/unauthorized", element: <UnauthorizedPage /> },
  { path: "/error", element: <ErrorPage /> },
  { path: "*", element: <NotFoundPage /> }
]);

