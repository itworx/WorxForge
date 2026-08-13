import type { AssetSummary } from "../../assets/models/assets.models";
import type { PagedRequest, PagedResult } from "../../../models/common.models";

export type SavedAssetsRequest = PagedRequest;

export type SavedAssetsResult = PagedResult<AssetSummary>;
