import { useEffect, useState } from "react";
import { FaCopy, FaDownload, FaFlag, FaRegThumbsDown, FaRegThumbsUp, FaShareAlt } from "react-icons/fa";
import ReactMarkdown from "react-markdown";
import type { Components } from "react-markdown";
import { Link, useParams } from "react-router-dom";
import { toast } from "sonner";
import { ROUTES } from "../../../constants/routes.constants";
import { useAssetDetails } from "../hooks/useAssetDetails";
import { useBookmark } from "../hooks/useBookmark";

type PrimaryTab = "overview" | "discussion";
type MarkdownTab = "rendered" | "raw";

const markdownComponents: Components = {
  h1: ({ children, ...props }) => (
    <h1 className="mt-4 mb-3 text-3xl font-bold tracking-tight text-text first:mt-0" {...props}>
      {children}
    </h1>
  ),
  h2: ({ children, ...props }) => (
    <h2 className="mt-4 mb-2 text-2xl font-semibold tracking-tight text-text" {...props}>
      {children}
    </h2>
  ),
  h3: ({ children, ...props }) => (
    <h3 className="mt-3 mb-2 text-xl font-semibold text-text" {...props}>
      {children}
    </h3>
  ),
  h4: ({ children, ...props }) => (
    <h4 className="mt-3 mb-1 text-lg font-semibold text-text" {...props}>
      {children}
    </h4>
  ),
  p: ({ children, ...props }) => (
    <p className="mb-3 text-sm leading-7 text-text" {...props}>
      {children}
    </p>
  ),
  ul: ({ children, ...props }) => (
    <ul className="mb-3 list-disc space-y-1 pl-5 text-sm text-text" {...props}>
      {children}
    </ul>
  ),
  ol: ({ children, ...props }) => (
    <ol className="mb-3 list-decimal space-y-1 pl-5 text-sm text-text" {...props}>
      {children}
    </ol>
  ),
  li: ({ children, ...props }) => (
    <li className="leading-7" {...props}>
      {children}
    </li>
  ),
  code: ({ children, ...props }) => (
    <code className="rounded bg-muted px-1 py-0.5 font-mono text-[0.9em] text-text" {...props}>
      {children}
    </code>
  )
};

export function AssetDetailsPage() {
  const { assetId } = useParams<{ assetId: string }>();
  const { data: asset, isLoading, isError } = useAssetDetails(assetId);

  const [primaryTab, setPrimaryTab] = useState<PrimaryTab>("overview");
  const [markdownTab, setMarkdownTab] = useState<MarkdownTab>("rendered");
  const [isLiked, setIsLiked] = useState(false);
  const [isDisliked, setIsDisliked] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const { mutate: toggleBookmark, isPending: isBookmarkPending } = useBookmark();

  useEffect(() => {
    setIsSaved(asset?.isBookmarked ?? false);
  }, [asset?.isBookmarked]);

  const likeCount = (asset?.reactionCount ?? 0) + (isLiked ? 1 : 0);
  const dislikeCount = (asset?.dislikeCount ?? 0) + (isDisliked ? 1 : 0);

  if (isLoading) {
    return (
      <main className="mx-auto flex w-full max-w-6xl flex-col gap-4 px-4 py-5 text-text sm:px-6 lg:px-8">
        <section className="rounded-2xl border border-secondary/30 bg-background p-4 shadow-sm sm:p-6">
          <p className="text-sm text-text/70">Loading asset details...</p>
        </section>
      </main>
    );
  }

  if (isError || !asset) {
    return (
      <main className="mx-auto flex w-full max-w-6xl flex-col gap-4 px-4 py-5 text-text sm:px-6 lg:px-8">
        <section className="rounded-2xl border border-secondary/30 bg-background p-4 shadow-sm sm:p-6">
          <p className="text-sm text-destructive">Could not load this asset. Please try again later.</p>
        </section>
      </main>
    );
  }

  const handleCopyMarkdown = async () => {
    try {
      await navigator.clipboard.writeText(asset.markdownContent);
      toast.success("markdown copied ");
    } catch {
      // Clipboard may be blocked in some browsers; the action is best effort.
    }
  };

  const handleDownloadMarkdown = () => {
    const blob = new Blob([asset.markdownContent], { type: "text/markdown;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = `${asset.title.replace(/\s+/g, "-").toLowerCase()}.md`;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  };

  const handleLike = () => {
    setIsLiked((current) => {
      const next = !current;
      if (next) {
        setIsDisliked(false);
      }
      return next;
    });
  };

  const handleDislike = () => {
    setIsDisliked((current) => {
      const next = !current;
      if (next) {
        setIsLiked(false);
      }
      return next;
    });
  };

  const handleShare = async () => {
    const shareUrl = window.location.href;

    if (navigator.share) {
      try {
        await navigator.share({ title: asset.title, url: shareUrl });
        return;
      } catch {
        // If share is cancelled/blocked, fall back to clipboard copy.
      }
    }

    try {
      await navigator.clipboard.writeText(shareUrl);
    } catch {
      // Best effort fallback.
    }
  };

  return (
    <main className="mx-auto flex w-full max-w-6xl flex-col gap-4 px-4 py-5 text-text sm:px-6 lg:px-8">
      <section className="rounded-2xl border border-secondary/30 bg-background p-4 shadow-sm sm:p-6">
        <div className="flex flex-wrap gap-2 text-[11px] font-medium text-text/70">
          <span className="rounded-full border border-secondary/30 bg-white px-2 py-0.5">{asset.assetTypeName}</span>
          <span className="rounded-full border border-secondary/30 bg-white px-2 py-0.5">{asset.domainName}</span>
          {asset.tools.map((tool) => (
            <span key={tool} className="rounded-full border border-secondary/30 bg-white px-2 py-0.5">
              {tool}
            </span>
          ))}
        </div>

        <h1 className="mt-3 text-2xl font-semibold tracking-tight sm:text-3xl">{asset.title}</h1>
        <p className="mt-1 text-sm text-text/70">{asset.description}</p>

        <div className="mt-3 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-text/60">
          <Link
            to={ROUTES.PROFILE(asset.authorId)}
            className="font-medium text-text/80 transition-colors hover:text-primary focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            {asset.authorDisplayName}
          </Link>
          <span>Updated {new Date(asset.updatedAt).toLocaleDateString()}</span>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          {asset.compatibleTools.map((tag) => (
            <span key={tag} className="rounded-full bg-primary/10 px-2 py-1 text-[11px] font-medium text-primary">
              {tag}
            </span>
          ))}
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          <div className="inline-flex w-24 flex-col items-center rounded-xl border border-secondary/30 bg-primary/10 p-2 text-center">
            <span className="text-xl font-semibold text-primary">+{likeCount}</span>
            <span className="text-[11px] text-text/60">helpful</span>
          </div>
          <div className="inline-flex w-24 flex-col items-center rounded-xl border border-secondary/30 bg-secondary/20 p-2 text-center">
            <span className="text-xl font-semibold text-text">-{dislikeCount}</span>
            <span className="text-[11px] text-text/60">not useful</span>
          </div>
        </div>

        <div className="mt-5 flex flex-wrap items-center gap-2 border-t border-secondary/30 pt-4">
          <button
            type="button"
            onClick={handleLike}
            aria-pressed={isLiked}
            className={`inline-flex items-center gap-1 rounded-md border px-3 py-1.5 text-xs font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary ${
              isLiked
                ? "border-primary bg-primary/10 text-primary"
                : "border-secondary/30 bg-white text-text hover:bg-primary/10"
            }`}
          >
            <FaRegThumbsUp className="size-3.5" aria-hidden="true" />
            Like
          </button>

          <button
            type="button"
            onClick={handleDislike}
            aria-pressed={isDisliked}
            className={`inline-flex items-center gap-1 rounded-md border px-3 py-1.5 text-xs font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary ${
              isDisliked
                ? "border-primary bg-primary/10 text-primary"
                : "border-secondary/30 bg-white text-text hover:bg-primary/10"
            }`}
          >
            <FaRegThumbsDown className="size-3.5" aria-hidden="true" />
            Dislike
          </button>

          <button
            type="button"
            onClick={handleCopyMarkdown}
            className="inline-flex items-center gap-1 rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            <FaCopy className="size-3.5" aria-hidden="true" />
            Copy Markdown
          </button>

          <button
            type="button"
            onClick={handleDownloadMarkdown}
            className="inline-flex items-center gap-1 rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            <FaDownload className="size-3.5" aria-hidden="true" />
            Download .md
          </button>

          <button
            type="button"
            disabled={isBookmarkPending}
            onClick={() => {
              if (!assetId) return;
              const previous = isSaved;
              const next = !previous;
              setIsSaved(next);
              toggleBookmark(
                { assetId, bookmark: next },
                {
                  onError: () => {
                    setIsSaved(previous);
                    toast.error("Failed to update bookmark. Please try again.");
                  }
                }
              );
            }}
            aria-pressed={isSaved}
            className={`inline-flex items-center gap-1 rounded-md border px-3 py-1.5 text-xs font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary ${
              isSaved
                ? "border-primary bg-primary/10 text-primary"
                : "border-secondary/30 bg-white text-text hover:bg-primary/10"
            }`}
          >
            {isSaved ? "Saved" : "Save"}
          </button>

          <button
            type="button"
            onClick={handleShare}
            className="inline-flex items-center gap-1 rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            <FaShareAlt className="size-3.5" aria-hidden="true" />
            Share link
          </button>

          <button
            type="button"
            className="inline-flex items-center gap-1 rounded-md border border-secondary/30 bg-white px-3 py-1.5 text-xs font-medium text-text transition-colors hover:bg-primary/10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
          >
            <FaFlag className="size-3.5" aria-hidden="true" />
            Report
          </button>

          <span className="ml-auto text-xs text-text/60">{asset.viewCount} views</span>
        </div>
      </section>

      <section className="rounded-2xl border border-secondary/30 bg-background shadow-sm">
        <div className="border-b border-secondary/30 px-4 pt-3 sm:px-6">
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => setPrimaryTab("overview")}
              className={`rounded-t-md border-b-2 px-3 py-2 text-sm font-medium transition-colors ${
                primaryTab === "overview"
                  ? "border-primary text-primary"
                  : "border-transparent text-text/70 hover:text-text"
              }`}
            >
              Overview
            </button>
            <button
              type="button"
              onClick={() => setPrimaryTab("discussion")}
              className={`rounded-t-md border-b-2 px-3 py-2 text-sm font-medium transition-colors ${
                primaryTab === "discussion"
                  ? "border-primary text-primary"
                  : "border-transparent text-text/70 hover:text-text"
              }`}
            >
              Discussion (0)
            </button>
          </div>
        </div>

        {primaryTab === "overview" ? (
          <div className="p-4 sm:p-6">
            <div className="mb-4 inline-flex rounded-md border border-secondary/30 bg-white p-1">
              <button
                type="button"
                onClick={() => setMarkdownTab("rendered")}
                className={`rounded px-3 py-1.5 text-xs font-medium transition-colors ${
                  markdownTab === "rendered" ? "bg-primary/10 text-primary" : "text-text/70 hover:text-text"
                }`}
              >
                Rendered
              </button>
              <button
                type="button"
                onClick={() => setMarkdownTab("raw")}
                className={`rounded px-3 py-1.5 text-xs font-medium transition-colors ${
                  markdownTab === "raw" ? "bg-primary/10 text-primary" : "text-text/70 hover:text-text"
                }`}
              >
                Raw Markdown
              </button>
            </div>

            {markdownTab === "rendered" ? (
              <article className="max-w-none">
                <ReactMarkdown components={markdownComponents}>{asset.markdownContent}</ReactMarkdown>
              </article>
            ) : (
              <pre className="overflow-x-auto rounded-lg border border-secondary/30 bg-white p-4 text-xs leading-6 text-text">
                {asset.markdownContent}
              </pre>
            )}
          </div>
        ) : (
          <div className="p-4 sm:p-6" />
        )}
      </section>
    </main>
  );
}


