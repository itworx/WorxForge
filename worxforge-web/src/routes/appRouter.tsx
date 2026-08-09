import { createBrowserRouter, Navigate } from "react-router-dom";
import { MainLayout } from "../layouts/MainLayout";
import { AdminLayout } from "../layouts/AdminLayout";
import { AuthLayout } from "../layouts/AuthLayout";
import { ProtectedRoute } from "./ProtectedRoute";
import { AdminRoute } from "./AdminRoute";
import { LoginPage } from "../features/authentication/pages/LoginPage";
import { HomePage } from "../features/home/pages/HomePage";
import { ExplorePage } from "../features/explore/pages/ExplorePage";
import { AssetDetailsPage } from "../features/assets/pages/AssetDetailsPage";
import { CreateAssetPage } from "../features/assets/pages/CreateAssetPage";
import { EditAssetPage } from "../features/assets/pages/EditAssetPage";
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
import { ROUTES } from "../constants/routes.constants";

const toChildPath = (path: string) => path.replace(/^\//, "");

export const appRouter = createBrowserRouter([
  {
    path: toChildPath(ROUTES.AUTH.LOGIN).split("/").slice(0, -1).join("/"),
    element: <AuthLayout />,
    children: [
      { path: toChildPath(ROUTES.AUTH.LOGIN).split("/").at(-1), element: <LoginPage /> },
      { path: toChildPath(ROUTES.AUTH.REGISTER).split("/").at(-1), element: <Navigate to={ROUTES.AUTH.LOGIN} replace /> },
      { path: toChildPath(ROUTES.AUTH.VERIFY_EMAIL).split("/").at(-1), element: <Navigate to={ROUTES.AUTH.LOGIN} replace /> },
      { path: toChildPath(ROUTES.AUTH.FORGOT_PASSWORD).split("/").at(-1), element: <Navigate to={ROUTES.AUTH.LOGIN} replace /> },
      { path: toChildPath(ROUTES.AUTH.RESET_PASSWORD(":token")).split("/").slice(-2).join("/"), element: <Navigate to={ROUTES.AUTH.LOGIN} replace /> }
    ]
  },
  {
    path: "/",
    element: <MainLayout />,
    children: [
      { index: true, element: <Navigate to={ROUTES.HOME} replace /> },
      { path: toChildPath(ROUTES.HOME), element: <ProtectedRoute><HomePage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.EXPLORE), element: <ProtectedRoute><ExplorePage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.ASSET(":assetId")), element: <ProtectedRoute><AssetDetailsPage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.PROFILE_ME), element: <ProtectedRoute><MyProfilePage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.PROFILE(":userId")), element: <ProtectedRoute><PublicProfilePage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.SETTINGS), element: <ProtectedRoute><SettingsPage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.MY_LIBRARY), element: <ProtectedRoute><LibraryPage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.MY_ASSETS), element: <ProtectedRoute><MyAssetsPage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.MY_ASSETS_CREATE), element: <ProtectedRoute><CreateAssetPage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.MY_ASSETS_EDIT(":assetId")), element: <ProtectedRoute><EditAssetPage /></ProtectedRoute> },
      { path: toChildPath(ROUTES.NOTIFICATIONS), element: <ProtectedRoute><NotificationsPage /></ProtectedRoute> }
    ]
  },
  {
    path: toChildPath(ROUTES.ADMIN.USERS).split("/").slice(0, -1).join("/"),
    element: <AdminRoute><AdminLayout /></AdminRoute>,
    children: [
      { path: toChildPath(ROUTES.ADMIN.USERS).split("/").at(-1), element: <AdminUsersPage /> },
      { path: toChildPath(ROUTES.ADMIN.ASSETS).split("/").at(-1), element: <AdminAssetsPage /> },
      { path: toChildPath(ROUTES.ADMIN.COMMENTS).split("/").at(-1), element: <AdminCommentsPage /> },
      { path: toChildPath(ROUTES.ADMIN.REPORTS).split("/").at(-1), element: <AdminReportsPage /> }
    ]
  },
  { path: ROUTES.UNAUTHORIZED, element: <UnauthorizedPage /> },
  { path: ROUTES.ERROR, element: <ErrorPage /> },
  { path: "*", element: <NotFoundPage /> }
]);

