import type { AssetCommentDto, AssetCommentDtoPagedResultDto } from "../../../api/worxforgeClient";

export const COMMENTS_PAGE_SIZE = 20;
export const COMMENT_MAX_LENGTH = 2000;

export interface CreateCommentInput {
  content: string;
  parentCommentId?: string;
}

export interface UpdateCommentInput {
  commentId: string;
  content: string;
}

export interface DeleteCommentInput {
  commentId: string;
}

export type CommentThread = AssetCommentDto;
export type CommentsPage = AssetCommentDtoPagedResultDto;
