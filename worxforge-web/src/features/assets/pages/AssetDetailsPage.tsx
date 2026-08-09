import { useMemo, useState } from "react";
import { FaCopy, FaDownload, FaFlag, FaRegThumbsDown, FaRegThumbsUp, FaShareAlt } from "react-icons/fa";
import ReactMarkdown from "react-markdown";
import { useParams } from "react-router-dom";
import type { AssetDetails } from "../models/assets.models";

type PrimaryTab = "overview" | "discussion";
type MarkdownTab = "rendered" | "raw";

const SAMPLE_MARKDOWN = `# .NET Coding Standard

## Naming

- PascalCase for types, methods, and properties.
- camelCase for locals and parameters.
- Prefix interfaces with \`I\`.

## Async

- Suffix async methods with \`Async\`.
- Never \`.Result\` or \`.Wait()\`.
- Pass \`CancellationToken\` through public APIs.

## Dependency Injection

- Constructor injection only.
- Register in \`Program.cs\` grouped by feature.

## Testing

- One test project per production project.
- Arrange / Act / Assert with blank lines between sections.
`;

function createPageAsset(assetId: string): AssetDetails {
  return {
    assetId,
    title: ".NET Coding Standard",
    description:
      "Team-approved coding standard for C# / .NET 8 projects - naming, structure, async, DI, and testing conventions.",
    assetTypeName: "Coding standard",
    domainName: "Development",
    tools: ["Code review", "Version 2026.Q2"],
    authorDisplayName: "Author Devon Rios",
    createdAt: "2026-07-28T12:00:00.000Z",
    updatedAt: "2026-08-09T09:00:00.000Z",
    reactionCount: 7,
    commentCount: 0,
    isBookmarked: true,
    markdownContent: SAMPLE_MARKDOWN,
    useCaseName: "Code review",
    compatibleTools: ["Tool agnostic", "C#", ".NET", "style", "conventions"],
    version: "2026.Q2",
    authorId: "user-1",
    authorAvatarUrl: undefined,
    viewCount: 501,
    isFollowing: false,
  };
}

export function AssetDetailsPage() {
  const { assetId } = useParams<{ assetId: string }>();
  const asset = useMemo(() => createPageAsset(assetId ?? "preview-asset"), [assetId]);

  const [primaryTab, setPrimaryTab] = useState<PrimaryTab>("overview");
  const [markdownTab, setMarkdownTab] = useState<MarkdownTab>("rendered");
  const [isLiked, setIsLiked] = useState(false);
  const [isDisliked, setIsDisliked] = useState(false);
  const [isSaved, setIsSaved] = useState(asset.isBookmarked);

  const likeCount = asset.reactionCount + (isLiked ? 1 : 0);

  const handleCopyMarkdown = async () => {
    try {
      await navigator.clipboard.writeText(asset.markdownContent);
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
          <span>{asset.authorDisplayName}</span>
          <span>Updated about 2 months ago</span>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          {asset.compatibleTools.map((tag) => (
            <span key={tag} className="rounded-full bg-primary/10 px-2 py-1 text-[11px] font-medium text-primary">
              {tag}
            </span>
          ))}
        </div>

        <div className="mt-4 inline-flex w-24 flex-col items-center rounded-xl border border-secondary/30 bg-primary/10 p-2 text-center">
          <span className="text-xl font-semibold text-primary">+{likeCount}</span>
          <span className="text-[11px] text-text/60">{isDisliked ? "not useful" : "helpful"}</span>
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
            onClick={() => setIsSaved((current) => !current)}
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
              <article className="prose max-w-none prose-headings:text-text prose-p:text-text prose-li:text-text prose-strong:text-text">
                <ReactMarkdown>{asset.markdownContent}</ReactMarkdown>
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


