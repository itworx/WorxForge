import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { COMMENT_MAX_LENGTH, COMMENTS_PAGE_SIZE, type CreateCommentInput, type DeleteCommentInput, type UpdateCommentInput } from "../models/comments.models";
import { createCommentsService } from "../services/commentsService";

const commentsService = createCommentsService();

function normalizeContent(content: string): string {
  return content.trim();
}

function assertContent(content: string): string {
  const normalized = normalizeContent(content);

  if (!normalized) {
    throw new Error("Comment content cannot be empty.");
  }

  if (normalized.length > COMMENT_MAX_LENGTH) {
    throw new Error(`Comment content cannot exceed ${COMMENT_MAX_LENGTH} characters.`);
  }

  return normalized;
}

export function useComments(assetId?: string, pageSize: number = COMMENTS_PAGE_SIZE) {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);

  useEffect(() => {
    setPage(1);
  }, [assetId]);

  const commentsQuery = useQuery({
    queryKey: ["assets", "comments", assetId, page, pageSize],
    enabled: Boolean(assetId),
    queryFn: ({ signal }) => commentsService.getAssetComments(assetId as string, page, pageSize, signal)
  });

  const invalidateComments = async () => {
    if (!assetId) {
      return;
    }

    await queryClient.invalidateQueries({ queryKey: ["assets", "comments", assetId] });
    await queryClient.invalidateQueries({ queryKey: ["assets", "details", assetId] });
  };

  const createComment = useMutation({
    mutationFn: async ({ content, parentCommentId }: CreateCommentInput) => {
      if (!assetId) {
        throw new Error("Asset id is required to create a comment.");
      }

      const safeContent = assertContent(content);

      if (parentCommentId) {
        await commentsService.createReply(assetId, parentCommentId, safeContent);
        return;
      }

      await commentsService.createComment(assetId, safeContent);
      setPage(1);
    },
    onSuccess: invalidateComments
  });

  const updateComment = useMutation({
    mutationFn: async ({ commentId, content }: UpdateCommentInput) => {
      if (!assetId) {
        throw new Error("Asset id is required to update a comment.");
      }

      await commentsService.updateComment(assetId, commentId, assertContent(content));
    },
    onSuccess: invalidateComments
  });

  const deleteComment = useMutation({
    mutationFn: async ({ commentId }: DeleteCommentInput) => {
      if (!assetId) {
        throw new Error("Asset id is required to delete a comment.");
      }

      await commentsService.deleteComment(assetId, commentId);
    },
    onSuccess: invalidateComments
  });

  return {
    comments: commentsQuery.data?.items ?? [],
    page,
    pageSize,
    totalCount: commentsQuery.data?.totalCount ?? 0,
    hasNextPage: commentsQuery.data?.hasNextPage ?? false,
    isLoading: commentsQuery.isLoading,
    isError: commentsQuery.isError,
    error: commentsQuery.error,
    refetch: commentsQuery.refetch,
    setPage,
    createComment,
    updateComment,
    deleteComment
  };
}


