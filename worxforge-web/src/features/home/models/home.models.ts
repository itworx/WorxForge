import type { AssetSummary } from "../../assets/models/assets.models";

export interface DomainSummary {
  domainId: string;
  name: string;
  assetCount: number;
}

export interface HomeDashboard {
  usefulThisWeek: AssetSummary[];
  recentlyUpdated: AssetSummary[];
  recommendedForYou: AssetSummary[];
  popularAcrossTeams: AssetSummary[];
  domainSummaries: DomainSummary[];
}
