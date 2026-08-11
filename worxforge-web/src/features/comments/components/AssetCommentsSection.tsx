import { useEffect, useMemo, useState } from "react";
import type { AssetCommentDto } from "../../../api/worxforgeClient";
import { useAuthContext } from "../../../contexts/AuthContext";
import { formatDate, formatRelativeTime } from "../../../utils/date.utils";
import { toast } from "sonner";
import { COMMENT_MAX_LENGTH } from "../models/comments.models";
import { useComments } from "../hooks/useComments";

interface AssetCommentsSectionProps {
  assetId: string;
  assetAuthorId?: string;
  highlightCommentId?: string;
}

function findParentCommentId(comments: AssetCommentDto[], targetCommentId: string): string | undefined {
  for (const comment of comments) {
    if (comment.replies?.some((reply) => reply.commentId === targetCommentId)) {
      return comment.commentId;
    }
  }
  return undefined;
}

function getDisplayName(comment: AssetCommentDto): string {
  const value = comment.authorDisplayName?.trim();
  return value && value.length > 0 ? value : "Unknown user";
}

function getInitials(name: string): string {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}

function normalizeText(value: string): string {
  return value.trim();
}

function normalizeIdentity(value: string | undefined): string {
  return (value ?? "").trim().toLowerCase();
}

export function AssetCommentsSection({ assetId, assetAuthorId, highlightCommentId }: AssetCommentsSectionProps) {
  const { isAuthenticated, user } = useAuthContext();
  const {
    comments,
    page,
    totalCount,
    hasNextPage,
    isLoading,
    isError,
    refetch,
    setPage,
    createComment,
    updateComment,
    deleteComment
  } = useComments(assetId);

  const [draftComment, setDraftComment] = useState("");
  const [replyingToId, setReplyingToId] = useState<string | null>(null);
  const [replyDraft, setReplyDraft] = useState("");
  const [editingCommentId, setEditingCommentId] = useState<string | null>(null);
  const [editingDraft, setEditingDraft] = useState("");
  const [expandedReplies, setExpandedReplies] = useState<Set<string>>(new Set());

  const isAssetOwner = Boolean(isAuthenticated && user && assetAuthorId && user.userId === assetAuthorId);

  const canEditComment = (comment: AssetCommentDto): boolean => {
    if (!isAuthenticated || !user) return false;
    const commentAuthor = normalizeIdentity(comment.authorDisplayName);
    const currentUser = normalizeIdentity(user.displayName);
    return commentAuthor.length > 0 && commentAuthor === currentUser;
  };

  const canDeleteComment = (comment: AssetCommentDto): boolean => {
    if (!isAuthenticated || !user) return false;
    return canEditComment(comment) || isAssetOwner;
  };

  const toggleReplies = (commentId: string) => {
    setExpandedReplies((prev) => {
      const next = new Set(prev);
      if (next.has(commentId)) {
        next.delete(commentId);
      } else {
        next.add(commentId);
      }
      return next;
    });
  };

  const rootComments = useMemo(() => comments ?? [], [comments]);

  // Auto-expand and scroll to a comment deep-linked from a notification.
  useEffect(() => {
    if (!highlightCommentId || rootComments.length === 0) {
      return;
    }

    const parentCommentId = findParentCommentId(rootComments, highlightCommentId);
    if (parentCommentId) {
      setExpandedReplies((prev) => (prev.has(parentCommentId) ? prev : new Set(prev).add(parentCommentId)));
    }

    const element = document.getElementById(`comment-${highlightCommentId}`);
    element?.scrollIntoView({ behavior: "smooth", block: "center" });
  }, [highlightCommentId, rootComments]);

  const handleCreateComment = async () => {
    const content = normalizeText(draftComment);

    if (!content) {
      toast.error("Comment cannot be empty.");
      return;
    }

    if (content.length > COMMENT_MAX_LENGTH) {
      toast.error(`Comment cannot exceed ${COMMENT_MAX_LENGTH} characters.`);
      return;
    }

    try {
      await createComment.mutateAsync({ content });
      setDraftComment("");
      toast.success("Comment added.");
    } catch {
      toast.error("Failed to add comment. Please try again.");
    }
  };

  const handleReply = async (parentCommentId: string) => {
    const content = normalizeText(replyDraft);

    if (!content) {
      toast.error("Reply cannot be empty.");
      return;
    }

    if (content.length > COMMENT_MAX_LENGTH) {
      toast.error(`Reply cannot exceed ${COMMENT_MAX_LENGTH} characters.`);
      return;
    }

    try {
      await createComment.mutateAsync({ content, parentCommentId });
      setReplyDraft("");
      setReplyingToId(null);
      toast.success("Reply added.");
    } catch {
      toast.error("Failed to add reply. Please try again.");
    }
  };

  const handleSaveEdit = async (commentId: string) => {
    const content = normalizeText(editingDraft);

    if (!content) {
      toast.error("Comment cannot be empty.");
      return;
    }

    if (content.length > COMMENT_MAX_LENGTH) {
      toast.error(`Comment cannot exceed ${COMMENT_MAX_LENGTH} characters.`);
      return;
    }

    try {
      await updateComment.mutateAsync({ commentId, content });
      setEditingCommentId(null);
      setEditingDraft("");
      toast.success("Comment updated.");
    } catch {
      toast.error("Failed to update comment. Please try again.");
    }
  };

  const handleDelete = async (comment: AssetCommentDto) => {
    const commentId = comment.commentId;
    if (!commentId) {
      return;
    }

    if (!window.confirm("Delete this comment?")) {
      return;
    }

    try {
      await deleteComment.mutateAsync({ commentId });
      toast.success("Comment deleted.");
    } catch {
      toast.error("Failed to delete comment. Please try again.");
    }
  };

  const renderComment = (comment: AssetCommentDto, isReply: boolean = false) => {
    const commentId = comment.commentId;
    const displayName = getDisplayName(comment);
    const isEditing = editingCommentId === commentId;
    const canEdit = canEditComment(comment);
    const canDelete = canDeleteComment(comment);
    const replies = comment.replies ?? [];
    const repliesExpanded = commentId ? expandedReplies.has(commentId) : false;

    return (
      <article
        key={commentId ?? `${displayName}-${comment.createdAt ?? "comment"}`}
        id={commentId ? `comment-${commentId}` : undefined}
        className={`wf-section-enter rounded-xl border p-3 shadow-sm sm:p-4 ${isReply ? "ml-4" : ""} ${
          commentId && commentId === highlightCommentId ? "border-primary ring-2 ring-primary/50" : "border-secondary/30 bg-white"
        }`}
      >
        <header className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-center gap-2">
            {comment.authorAvatarUrl ? (
              <img src={comment.authorAvatarUrl} alt={displayName} className="size-8 rounded-full object-cover" />
            ) : (
              <span className="flex size-8 items-center justify-center rounded-full bg-muted text-xs font-semibold text-text">
                {getInitials(displayName) || "U"}
              </span>
            )}

            <div className="min-w-0">
              <p className="truncate text-sm font-semibold text-text">{displayName}</p>
              <p className="text-xs text-text/60" title={comment.createdAt ?? ""}>
                {comment.createdAt ? `${formatRelativeTime(comment.createdAt)} · ${formatDate(comment.createdAt)}` : ""}
              </p>
            </div>
          </div>

          {(canEdit || canDelete) && commentId ? (
            <div className="flex items-center gap-2">
              {canEdit ? (
                <button
                  type="button"
                  aria-label={`Edit comment by ${displayName}`}
                  className="rounded-md border border-secondary/30 px-2 py-1 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                  onClick={() => {
                    setEditingCommentId(commentId);
                    setEditingDraft(comment.content ?? "");
                  }}
                  disabled={updateComment.isPending || deleteComment.isPending}
                >
                  Edit
                </button>
              ) : null}
              <button
                type="button"
                aria-label={`Delete comment by ${displayName}`}
                className="rounded-md border border-secondary/30 px-2 py-1 text-xs font-medium text-text transition-colors hover:bg-destructive/10 hover:text-destructive focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                onClick={() => void handleDelete(comment)}
                disabled={deleteComment.isPending || updateComment.isPending}
              >
                Delete
              </button>
            </div>
          ) : null}
        </header>

        {isEditing && commentId ? (
          <div className="mt-3 space-y-2">
            <textarea
              aria-label="Edit comment"
              value={editingDraft}
              onChange={(event) => setEditingDraft(event.target.value)}
              rows={3}
              maxLength={COMMENT_MAX_LENGTH}
              className="w-full rounded-md border border-secondary/30 bg-background px-3 py-2 text-sm text-text focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
            />
            <div className="flex flex-wrap items-center gap-2">
              <button
                type="button"
                className="rounded-md bg-primary px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                onClick={() => void handleSaveEdit(commentId)}
                disabled={updateComment.isPending}
              >
                {updateComment.isPending ? "Saving..." : "Save"}
              </button>
              <button
                type="button"
                className="rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10"
                onClick={() => {
                  setEditingCommentId(null);
                  setEditingDraft("");
                }}
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <p className="mt-3 whitespace-pre-wrap break-words text-sm text-text">{comment.content ?? ""}</p>
        )}

        {!isReply && isAuthenticated && commentId ? (
          <div className="mt-3">
            {replyingToId === commentId ? (
              <div className="space-y-2 rounded-md border border-secondary/30 bg-background p-3">
                <label className="block text-xs font-medium text-text" htmlFor={`reply-${commentId}`}>
                  Reply
                </label>
                <textarea
                  id={`reply-${commentId}`}
                  aria-label="Reply to comment"
                  value={replyDraft}
                  onChange={(event) => setReplyDraft(event.target.value)}
                  rows={2}
                  maxLength={COMMENT_MAX_LENGTH}
                  className="w-full rounded-md border border-secondary/30 bg-white px-3 py-2 text-sm text-text focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                />
                <div className="flex flex-wrap items-center gap-2">
                  <button
                    type="button"
                    className="rounded-md bg-primary px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
                    onClick={() => void handleReply(commentId)}
                    disabled={createComment.isPending}
                  >
                    {createComment.isPending ? "Replying..." : "Reply"}
                  </button>
                  <button
                    type="button"
                    className="rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10"
                    onClick={() => {
                      setReplyingToId(null);
                      setReplyDraft("");
                    }}
                  >
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <button
                type="button"
                className="rounded-md border border-secondary/30 bg-white px-2.5 py-1 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                onClick={() => {
                  setReplyingToId(commentId);
                  setReplyDraft("");
                }}
              >
                Reply
              </button>
            )}
          </div>
        ) : null}

        {replies.length > 0 && commentId ? (
          <div className="mt-3">
            <button
              type="button"
              className="text-xs font-medium text-primary hover:underline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
              onClick={() => toggleReplies(commentId)}
              aria-expanded={repliesExpanded}
            >
              {repliesExpanded ? "Hide replies" : `View ${replies.length} ${replies.length === 1 ? "reply" : "replies"}`}
            </button>
            {repliesExpanded ? (
              <div className="mt-3 space-y-3 border-l border-secondary/30 pl-3">
                {replies.map((reply) => renderComment(reply, true))}
              </div>
            ) : null}
          </div>
        ) : null}
      </article>
    );
  };

  return (
    <section className="wf-section-enter space-y-4" aria-live="polite">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-text">Comments</h2>
        <p className="text-xs text-text/60">{totalCount} total</p>
      </div>

      {isAuthenticated ? (
        <div className="rounded-xl border border-secondary/30 bg-background p-3 sm:p-4">
          <label htmlFor="new-comment" className="block text-sm font-medium text-text">
            Add a comment
          </label>
          <textarea
            id="new-comment"
            aria-label="Add a comment"
            value={draftComment}
            onChange={(event) => setDraftComment(event.target.value)}
            rows={3}
            maxLength={COMMENT_MAX_LENGTH}
            className="mt-2 w-full rounded-md border border-secondary/30 bg-white px-3 py-2 text-sm text-text focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
            placeholder="Share your thoughts..."
          />
          <div className="mt-2 flex items-center justify-between gap-2">
            <p className="text-xs text-text/60">{draftComment.trim().length}/{COMMENT_MAX_LENGTH}</p>
            <button
              type="button"
              className="rounded-md bg-primary px-3 py-1.5 text-xs font-medium text-white transition-colors hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-60"
              disabled={createComment.isPending || draftComment.trim().length === 0}
              onClick={() => void handleCreateComment()}
            >
              {createComment.isPending ? "Posting..." : "Post comment"}
            </button>
          </div>
        </div>
      ) : (
        <p className="rounded-xl border border-secondary/30 bg-background p-4 text-sm text-text/70">
          Sign in to join the discussion.
        </p>
      )}

      {isError ? (
        <div className="wf-status-enter rounded-xl border border-destructive/30 bg-destructive/5 p-4 text-sm text-destructive">
          <p>Could not load comments right now.</p>
          <button
            type="button"
            className="mt-2 rounded-md border border-destructive/30 bg-background px-2.5 py-1 text-xs font-medium text-destructive transition-colors hover:bg-destructive/10"
            onClick={() => void refetch()}
          >
            Retry
          </button>
        </div>
      ) : null}

      {isLoading ? (
        <div className="space-y-3">
          <div className="wf-loading-pulse h-16 rounded-xl border border-secondary/30 bg-secondary/10" />
          <div className="wf-loading-pulse h-16 rounded-xl border border-secondary/30 bg-secondary/10" />
        </div>
      ) : rootComments.length === 0 && !isError ? (
        <p className="rounded-xl border border-secondary/30 bg-background p-4 text-sm text-text/70">
          No comments yet. Be the first to share your thoughts.
        </p>
      ) : (
        <div className="space-y-3">{rootComments.map((comment) => renderComment(comment))}</div>
      )}

      {!isLoading && hasNextPage ? (
        <div className="flex justify-center">
          <button
            type="button"
            onClick={() => setPage(page + 1)}
            className="rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            Load more comments
          </button>
        </div>
      ) : null}
    </section>
  );
}
