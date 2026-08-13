export type EntityId = string;

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}

export interface PagedRequest {
  page: number;
  pageSize: number;
}
