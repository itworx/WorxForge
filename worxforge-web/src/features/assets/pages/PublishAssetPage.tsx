import { useDeferredValue, useEffect, useId, useMemo, useRef, useState, type ChangeEvent } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Link, useNavigate, useParams } from "react-router-dom";
import { AlertCircle, FileUp, KeyRound, LoaderCircle, RefreshCw, Save, Upload, X } from "lucide-react";
import { toast } from "sonner";
import { ApiException, type CreateAssetRequest, type UpdateAssetRequest } from "../../../api/worxforgeClient";
import { ROUTES } from "../../../constants/routes.constants";
import { cn } from "../../../lib/utils";
import { clearAccessToken, getAccessToken, setAccessToken } from "../../../services/tokenService";
import { AssetCard } from "../components/AssetCard";
import { useAssetEditorLookups, useAssetForEdit, useCreateAsset, useUpdateAsset, useUploadMarkdown } from "../hooks/useAssets";

type AssetFormValues = {
  title: string;
  description: string;
  assetTypeId: string;
  domainId: string;
  useCaseId: string;
  toolIds: string[];
  tags: string[];
  markdownContent: string;
};

const EMPTY_FORM_VALUES: AssetFormValues = {
  title: "",
  description: "",
  assetTypeId: "",
  domainId: "",
  useCaseId: "",
  toolIds: [],
  tags: [],
  markdownContent: ""
};

function normalizeTag(value: string): string {
  return value.trim().replace(/\s+/g, " ");
}

function mergeUniqueTags(values: string[]): string[] {
  const seen = new Set<string>();
  const result: string[] = [];

  for (const value of values) {
    const normalized = normalizeTag(value);
    if (!normalized) {
      continue;
    }

    const key = normalized.toLowerCase();
    if (seen.has(key)) {
      continue;
    }

    seen.add(key);
    result.push(normalized);
  }

  return result;
}

function formatErrorMessage(error: unknown): string {
  if (error && typeof error === "object" && ApiException.isApiException(error)) {
    const responseText = typeof error.response === "string" ? error.response : "";
    if (responseText) {
      try {
        const parsed = JSON.parse(responseText) as { message?: string; details?: string | null };
        const backendMessage = parsed?.details || parsed?.message;
        if (typeof backendMessage === "string" && backendMessage.trim().length > 0) {
          return backendMessage;
        }
      } catch {
        // Ignore malformed response payloads and use status-driven fallbacks.
      }
    }

    if (error.status === 400) return "The asset data is invalid. Review the highlighted fields and try again.";
    if (error.status === 401) return "Your session is not authorized for this action.";
    if (error.status === 409) return "This asset conflicts with an existing record.";
    if (error.status === 422) return "Some referenced metadata is no longer valid. Refresh and try again.";
    if (error.status === 500) return "The API failed while publishing this asset. Check backend logs for 'General.Unexpected' details.";
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return "Something went wrong while saving the asset.";
}

function formatLoadError(error: unknown, fallback: string): string {
  if (error instanceof Error && (error.name === "AssetLookupError" || error.name === "AssetRequestError")) {
    return error.message;
  }

  if (error && typeof error === "object" && ApiException.isApiException(error) && error.status === 401) {
    return "Your session is not authorized for this page. Sign in again and retry.";
  }

  return fallback;
}

function Section({
  title,
  description,
  delayMs = 0,
  children,
  className
}: {
  title: string;
  description?: string;
  delayMs?: number;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <section
      className={cn(
        "wf-section-enter rounded-2xl border border-border/70 bg-card p-5 shadow-sm motion-reduce:opacity-100",
        className
      )}
      style={{ animationDelay: `${delayMs}ms` }}
    >
      <div className="mb-4 space-y-1">
        <h2 className="text-base font-semibold text-foreground">{title}</h2>
        {description ? <p className="text-sm text-muted-foreground">{description}</p> : null}
      </div>
      {children}
    </section>
  );
}

function FieldError({ message }: { message?: string }) {
  if (!message) {
    return null;
  }

  return (
    <p className="inline-flex items-center gap-1 text-xs text-destructive">
      <AlertCircle className="size-3.5" aria-hidden="true" />
      <span>{message}</span>
    </p>
  );
}

function Label({ htmlFor, children }: { htmlFor: string; children: React.ReactNode }) {
  return <label htmlFor={htmlFor} className="text-sm font-medium text-foreground">{children}</label>;
}

function inputClassName(hasError: boolean) {
  return cn(
    "w-full rounded-xl border bg-background px-3 py-2.5 text-sm text-foreground shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/70 focus-visible:ring-offset-2 focus-visible:ring-offset-background",
    hasError ? "border-destructive/70" : "border-input hover:border-primary/50"
  );
}

export function PublishAssetPage() {
  const navigate = useNavigate();
  const { assetId } = useParams<{ assetId: string }>();
  const isEditMode = Boolean(assetId);
  const pageTitle = isEditMode ? "Edit Asset" : "Publish Asset";
  const submitLabel = isEditMode ? "Save Asset" : "Publish Asset";
  const submittingLabel = isEditMode ? "Saving..." : "Publishing...";
  const uploadInputId = useId();
  const [selectedFileName, setSelectedFileName] = useState<string>("");
  const [tagInputValue, setTagInputValue] = useState<string>("");
  const [hasRuntimeToken, setHasRuntimeToken] = useState<boolean>(() => Boolean(getAccessToken()));
  const [isAuthorizing, setIsAuthorizing] = useState(false);
  const editHydratedRef = useRef(false);

  const {
    register,
    control,
    handleSubmit,
    reset,
    setValue,
    setError,
    clearErrors,
    formState: { errors, isDirty, isSubmitting }
  } = useForm<AssetFormValues>({
    defaultValues: EMPTY_FORM_VALUES
  });

  const lookupsQuery = useAssetEditorLookups();
  const editAssetQuery = useAssetForEdit(assetId, lookupsQuery.data);
  const createMutation = useCreateAsset();
  const updateMutation = useUpdateAsset();
  const uploadMutation = useUploadMarkdown();
  const hasAuthorization = hasRuntimeToken;

  const watchedValues = useWatch({ control });
  const deferredValues = useDeferredValue(watchedValues);

  useEffect(() => {
    if (!isEditMode || !editAssetQuery.data || editHydratedRef.current) {
      return;
    }

    editHydratedRef.current = true;
    reset({
      title: editAssetQuery.data.title,
      description: editAssetQuery.data.description,
      assetTypeId: editAssetQuery.data.assetTypeId,
      domainId: editAssetQuery.data.domainId,
      useCaseId: editAssetQuery.data.useCaseId,
      toolIds: editAssetQuery.data.toolIds,
      tags: editAssetQuery.data.tags,
      markdownContent: editAssetQuery.data.markdownContent
    });
    setTagInputValue("");
  }, [editAssetQuery.data, isEditMode, reset]);

  useEffect(() => {
    if (!isDirty) {
      return undefined;
    }

    const handleBeforeUnload = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      event.returnValue = "";
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => window.removeEventListener("beforeunload", handleBeforeUnload);
  }, [isDirty]);

  const previewAsset = useMemo(() => {
    const assetTypeName = lookupsQuery.data?.assetTypes.find((item) => item.id === deferredValues.assetTypeId)?.name ?? "Asset Type";
    const domainName = lookupsQuery.data?.domains.find((item) => item.id === deferredValues.domainId)?.name ?? "Domain";
    const toolNames = (deferredValues.toolIds ?? [])
      .map((toolId) => lookupsQuery.data?.tools.find((item) => item.id === toolId)?.name ?? "")
      .filter(Boolean);

    return {
      assetId: assetId ?? "preview-asset",
      title: deferredValues.title?.trim() || "Untitled asset",
      description: deferredValues.description?.trim() || "Add a short description so teammates know when to use this asset.",
      assetTypeName,
      domainName,
      tools: toolNames,
      authorDisplayName: isEditMode ? (editAssetQuery.data?.preview.authorDisplayName ?? "You") : "You",
      createdAt: isEditMode ? (editAssetQuery.data?.preview.createdAt ?? new Date().toISOString()) : new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      reactionCount: isEditMode ? (editAssetQuery.data?.preview.reactionCount ?? 0) : 0,
      commentCount: isEditMode ? (editAssetQuery.data?.preview.commentCount ?? 0) : 0,
      isBookmarked: isEditMode ? (editAssetQuery.data?.preview.isBookmarked ?? false) : false
    };
  }, [assetId, deferredValues, editAssetQuery.data, isEditMode, lookupsQuery.data]);

  const isPending = isSubmitting || createMutation.isPending || updateMutation.isPending;
  const isInitialEditLoading = isEditMode && (lookupsQuery.isLoading || editAssetQuery.isLoading);
  const hasLookupError = lookupsQuery.isError;
  const hasEditError = isEditMode && editAssetQuery.isError;
  const lookupErrorMessage = formatLoadError(
    lookupsQuery.error,
    "The asset lookup data could not be loaded. Verify that the WorxForge API is running, then retry."
  );
  const editErrorMessage = formatLoadError(
    editAssetQuery.error,
    "The asset could not be loaded for editing."
  );

  async function handleMarkdownUpload(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];

    if (!file) {
      return;
    }

    const isMarkdownFile = file.name.toLowerCase().endsWith(".md");
    if (!isMarkdownFile) {
      setError("markdownContent", {
        type: "validate",
        message: "Only .md files are accepted."
      });
      toast.error("Only Markdown files are accepted.");
      event.target.value = "";
      return;
    }

    clearErrors("markdownContent");
    try {
      const markdownText = await uploadMutation.mutateAsync({ file });
      setSelectedFileName(file.name);
      setValue("markdownContent", markdownText, { shouldDirty: true, shouldValidate: true });
      toast.success("Markdown uploaded. You can keep editing the content below.");
    } catch (error) {
      toast.error(formatErrorMessage(error));
    } finally {
      event.target.value = "";
    }
  }

  function toggleTool(toolId: string) {
    const selectedTools = watchedValues.toolIds ?? [];
    const nextValue = selectedTools.includes(toolId)
      ? selectedTools.filter((value) => value !== toolId)
      : [...selectedTools, toolId];

    setValue("toolIds", nextValue, { shouldDirty: true, shouldValidate: true });
  }

  function addTagFromInput(rawValue: string) {
    const normalized = normalizeTag(rawValue);
    if (!normalized) {
      return false;
    }

    const currentTags = watchedValues.tags ?? [];
    const nextTags = mergeUniqueTags([...currentTags, normalized]);
    if (nextTags.length === currentTags.length) {
      return false;
    }

    setValue("tags", nextTags, { shouldDirty: true, shouldValidate: true });
    return true;
  }

  function removeTag(tagToRemove: string) {
    const currentTags = watchedValues.tags ?? [];
    const nextTags = currentTags.filter((tag) => tag !== tagToRemove);
    setValue("tags", nextTags, { shouldDirty: true, shouldValidate: true });
  }

  function handleCancel() {
    if (isDirty && !window.confirm("Discard your unsaved changes?")) {
      return;
    }

    navigate(ROUTES.MY_ASSETS);
  }

  async function handleConnectDevelopmentAuthorization() {
    setIsAuthorizing(true);

    try {
      const baseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.trim()?.replace(/\/$/, "") ?? "";
      const response = await fetch(`${baseUrl}/api/authentication/debug-login`, {
        method: "POST",
        headers: { Accept: "application/json" }
      });

      if (!response.ok) {
        throw new Error(`Auth endpoint returned HTTP ${response.status}.`);
      }

      const payload = await response.json() as { accessToken?: string; token?: string };
      const token = payload.accessToken ?? payload.token;
      if (!token || token.trim().length === 0) {
        throw new Error("Auth endpoint did not return an access token.");
      }

      setAccessToken(token);
      setHasRuntimeToken(true);
      toast.success("Development authorization connected.");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not connect development authorization.");
    } finally {
      setIsAuthorizing(false);
    }
  }

  function handleClearAuthorizationToken() {
    clearAccessToken();
    setHasRuntimeToken(false);
    toast.success("Local authorization token removed.");
  }

  async function onSubmit(values: AssetFormValues) {
    const payload: CreateAssetRequest = {
      title: values.title.trim(),
      description: values.description.trim() || undefined,
      assetTypeId: values.assetTypeId,
      domainId: values.domainId,
      useCaseId: values.useCaseId,
      toolIds: values.toolIds,
      tags: mergeUniqueTags(values.tags),
      markdownContent: values.markdownContent.trim()
    };

    try {
      if (isEditMode && assetId) {
        await updateMutation.mutateAsync({ assetId, body: payload as UpdateAssetRequest });
        toast.success("Asset updated successfully.");
      } else {
        await createMutation.mutateAsync({ body: payload });
        toast.success("Asset published successfully.");
      }

      navigate(ROUTES.MY_ASSETS);
    } catch (error) {
      toast.error(formatErrorMessage(error));
    }
  }

  if (isInitialEditLoading) {
    return (
      <div className="wf-page-enter mx-auto flex min-h-[60vh] w-full max-w-7xl items-center justify-center px-4 py-10 sm:px-6 lg:px-8">
        <div className="wf-status-enter w-full max-w-md rounded-2xl border border-border bg-card px-6 py-5 shadow-sm" role="status" aria-live="polite">
          <div className="flex items-center gap-3">
            <LoaderCircle className="size-5 text-primary motion-safe:animate-spin motion-reduce:animate-none" aria-hidden="true" />
            <div>
              <p className="text-sm font-medium text-foreground">Loading asset editor...</p>
              <p className="mt-1 text-xs text-muted-foreground">Retrieving metadata and the current asset content.</p>
            </div>
          </div>
          <div className="mt-4 space-y-2" aria-hidden="true">
            <div className="wf-loading-pulse h-2 w-3/4 rounded-full bg-muted" />
            <div className="wf-loading-pulse h-2 w-1/2 rounded-full bg-muted" style={{ animationDelay: "120ms" }} />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="wf-page-enter mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <header className="wf-section-enter mb-6 flex flex-col gap-4 md:flex-row md:items-end md:justify-between" style={{ animationDelay: "0ms" }}>
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">Assets</p>
          <h1 className="text-3xl font-semibold text-foreground">{pageTitle}</h1>
          <p className="max-w-2xl text-sm text-muted-foreground">
            Fill in the asset metadata, upload or edit Markdown content, and review the live card preview before saving.
          </p>
        </div>
        <div className="flex items-center gap-3 text-sm text-muted-foreground" aria-live="polite">
          {uploadMutation.isPending ? <span className="wf-status-enter inline-flex items-center gap-2"><LoaderCircle className="size-4 motion-safe:animate-spin motion-reduce:animate-none" />Uploading Markdown...</span> : null}
        </div>
      </header>

      {hasLookupError || hasEditError ? (
        <div className="wf-status-enter mb-6 rounded-2xl border border-destructive/30 bg-destructive/5 p-4 text-sm text-destructive" role="alert">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <p>{hasEditError ? editErrorMessage : lookupErrorMessage}</p>
            <button
              type="button"
              className="inline-flex items-center justify-center gap-2 rounded-lg border border-destructive/30 px-3 py-2 font-medium transition-[background-color,transform] motion-safe:duration-180 motion-reduce:transition-none hover:bg-destructive/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background active:scale-[0.98] motion-reduce:active:scale-100"
              disabled={lookupsQuery.isFetching || editAssetQuery.isFetching}
              onClick={() => {
                void lookupsQuery.refetch();
                if (isEditMode) {
                  void editAssetQuery.refetch();
                }
              }}
            >
              <RefreshCw className={cn("size-4", (lookupsQuery.isFetching || editAssetQuery.isFetching) && "motion-safe:animate-spin motion-reduce:animate-none")} aria-hidden="true" />
              {lookupsQuery.isFetching || editAssetQuery.isFetching ? "Retrying..." : "Retry"}
            </button>
          </div>
        </div>
      ) : null}

      <form onSubmit={handleSubmit(onSubmit)} className="grid gap-6 xl:grid-cols-[minmax(0,1.15fr)_minmax(320px,0.85fr)]" aria-busy={isPending}>
        <div className="space-y-6">
          <Section title="Asset Metadata" description="Use the real lookup data from WorxForge to describe how the asset should be categorized." delayMs={80}>
            {lookupsQuery.isLoading ? (
              <div className="wf-status-enter mb-4 flex items-center gap-2 rounded-xl border border-border bg-muted/40 px-3 py-2.5 text-sm text-muted-foreground" role="status" aria-live="polite">
                <LoaderCircle className="size-4 text-primary motion-safe:animate-spin motion-reduce:animate-none" aria-hidden="true" />
                Loading metadata options...
              </div>
            ) : null}
            <div className="grid gap-4 md:grid-cols-2">
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="asset-title">Title</Label>
                <input
                  id="asset-title"
                  type="text"
                  placeholder="Enter a clear asset title"
                  className={inputClassName(Boolean(errors.title))}
                  {...register("title", {
                    required: "Title is required.",
                    validate: (value) => value.trim().length > 0 || "Title is required."
                  })}
                />
                <FieldError message={errors.title?.message} />
              </div>

              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="asset-description">Description</Label>
                <textarea
                  id="asset-description"
                  rows={3}
                  placeholder="Summarize when teammates should use this asset"
                  className={inputClassName(Boolean(errors.description))}
                  {...register("description")}
                />
                <FieldError message={errors.description?.message} />
              </div>

              <div className="space-y-2">
                <Label htmlFor="asset-type">Asset Type</Label>
                <select
                  id="asset-type"
                  className={inputClassName(Boolean(errors.assetTypeId))}
                  disabled={lookupsQuery.isLoading}
                  {...register("assetTypeId", { required: "Asset type is required." })}
                >
                  <option value="">Select an asset type</option>
                  {(lookupsQuery.data?.assetTypes ?? []).map((item) => (
                    <option key={item.id} value={item.id}>{item.name}</option>
                  ))}
                </select>
                <FieldError message={errors.assetTypeId?.message} />
              </div>

              <div className="space-y-2">
                <Label htmlFor="asset-domain">Domain</Label>
                <select
                  id="asset-domain"
                  className={inputClassName(Boolean(errors.domainId))}
                  disabled={lookupsQuery.isLoading}
                  {...register("domainId", { required: "Domain is required." })}
                >
                  <option value="">Select a domain</option>
                  {(lookupsQuery.data?.domains ?? []).map((item) => (
                    <option key={item.id} value={item.id}>{item.name}</option>
                  ))}
                </select>
                <FieldError message={errors.domainId?.message} />
              </div>

              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="asset-use-case">Use Case</Label>
                <select
                  id="asset-use-case"
                  className={inputClassName(Boolean(errors.useCaseId))}
                  disabled={lookupsQuery.isLoading}
                  {...register("useCaseId", { required: "Use case is required." })}
                >
                  <option value="">Select a use case</option>
                  {(lookupsQuery.data?.useCases ?? []).map((item) => (
                    <option key={item.id} value={item.id}>{item.name}</option>
                  ))}
                </select>
                <FieldError message={errors.useCaseId?.message} />
              </div>

              <div className="space-y-2 md:col-span-2">
                <div className="flex items-center justify-between gap-3">
                  <Label htmlFor="asset-tools">Compatible Tools</Label>
                  <span className="text-xs text-muted-foreground">Choose one or more</span>
                </div>
                <input
                  id="asset-tools"
                  type="hidden"
                  {...register("toolIds", {
                    validate: (value) => (value?.length ?? 0) > 0 || "Select at least one compatible tool."
                  })}
                />
                <div className="flex flex-wrap gap-2 rounded-xl border border-input bg-background p-3">
                  {(lookupsQuery.data?.tools ?? []).map((tool) => {
                    const selected = (watchedValues.toolIds ?? []).includes(tool.id);
                    return (
                      <button
                        key={tool.id}
                        type="button"
                        aria-pressed={selected}
                        onClick={() => toggleTool(tool.id)}
                        className={cn(
                          "inline-flex items-center rounded-full border px-3 py-1.5 text-sm font-medium transition-[background-color,color,transform,box-shadow] motion-safe:duration-180 motion-safe:ease-out motion-reduce:transition-none focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/70 focus-visible:ring-offset-2 focus-visible:ring-offset-background active:scale-[0.98] motion-reduce:active:scale-100",
                          selected
                            ? "wf-control-selected wf-rainbow-active border-primary/40 bg-primary/10 text-primary shadow-sm"
                            : "border-border bg-card text-foreground hover:border-primary/40 hover:bg-primary/5"
                        )}
                      >
                        {tool.name}
                      </button>
                    );
                  })}
                </div>
                <FieldError message={errors.toolIds?.message as string | undefined} />
              </div>

              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="asset-tags-input">Tags</Label>
                <div
                  className={cn(
                    "rounded-xl border bg-background px-3 py-2 shadow-sm transition-colors focus-within:ring-2 focus-within:ring-ring/70 focus-within:ring-offset-2 focus-within:ring-offset-background",
                    errors.tags ? "border-destructive/70" : "border-input hover:border-primary/50"
                  )}
                >
                  <div className="flex flex-wrap items-center gap-2">
                    {(watchedValues.tags ?? []).map((tag) => (
                      <span
                        key={tag}
                        className="wf-tag-chip-enter inline-flex items-center gap-1 rounded-full border border-primary/35 bg-primary/12 px-2.5 py-1 text-xs font-medium text-primary"
                      >
                        <span>{tag}</span>
                        <button
                          type="button"
                          className="inline-flex size-4 items-center justify-center rounded-full text-primary/80 transition-colors hover:bg-primary/20 hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/70"
                          aria-label={`Remove tag ${tag}`}
                          onClick={() => removeTag(tag)}
                        >
                          <X className="size-3" aria-hidden="true" />
                        </button>
                      </span>
                    ))}
                    <input
                      id="asset-tags-input"
                      type="text"
                      value={tagInputValue}
                      placeholder={(watchedValues.tags ?? []).length > 0 ? "Type then press Space" : "Type a tag then press Space"}
                      className="min-w-[12ch] flex-1 bg-transparent py-1 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none"
                      onChange={(event) => setTagInputValue(event.target.value)}
                      onKeyDown={(event) => {
                        if (event.key === " ") {
                          event.preventDefault();
                          if (addTagFromInput(tagInputValue)) {
                            setTagInputValue("");
                          }
                          return;
                        }

                        if (event.key === "Backspace" && tagInputValue.length === 0 && (watchedValues.tags?.length ?? 0) > 0) {
                          event.preventDefault();
                          const currentTags = watchedValues.tags ?? [];
                          removeTag(currentTags[currentTags.length - 1]);
                        }
                      }}
                    />
                  </div>
                </div>
                <p className="text-xs text-muted-foreground">Optional. Press Space to turn each tag into a bubble.</p>
              </div>
            </div>
          </Section>

          <Section title="Markdown Content" description="Type, paste, or upload a .md document. Uploaded content stays editable." delayMs={140}>
            <div className="space-y-4">
              <div className="flex flex-col gap-3 rounded-2xl border border-dashed border-primary/35 bg-primary/5 p-4 md:flex-row md:items-center md:justify-between">
                <div className="space-y-1">
                  <p className="inline-flex items-center gap-2 text-sm font-medium text-foreground">
                    <FileUp className="size-4 text-primary" aria-hidden="true" />
                    Upload a Markdown file
                  </p>
                  <p className="text-xs text-muted-foreground">Only <span className="font-semibold text-foreground">.md</span> files are accepted.</p>
                  {selectedFileName ? <p className="wf-status-enter text-xs text-primary">Current file: {selectedFileName}</p> : null}
                </div>
                <label
                  htmlFor={uploadInputId}
                  className={cn(
                    "inline-flex cursor-pointer items-center justify-center gap-2 rounded-xl border border-primary/30 bg-background px-4 py-2.5 text-sm font-medium text-foreground shadow-sm transition-[background-color,color,transform] motion-safe:duration-180 motion-safe:ease-out motion-reduce:transition-none hover:bg-primary/10 hover:text-primary active:scale-[0.98] motion-reduce:active:scale-100",
                    (uploadMutation.isPending || !hasAuthorization) && "cursor-not-allowed opacity-60"
                  )}
                >
                  <Upload className="size-4" aria-hidden="true" />
                  {uploadMutation.isPending ? "Uploading..." : "Choose .md file"}
                </label>
                <input
                  id={uploadInputId}
                  type="file"
                  accept=".md"
                  className="sr-only"
                  disabled={uploadMutation.isPending || !hasAuthorization}
                  onChange={handleMarkdownUpload}
                />
                {!hasAuthorization ? (
                  <p className="text-xs text-destructive">Connect development authorization before uploading.</p>
                ) : null}
              </div>

              <div className="space-y-2">
                <Label htmlFor="asset-markdown">Markdown Editor</Label>
                <textarea
                  id="asset-markdown"
                  rows={18}
                  spellCheck={false}
                  placeholder="# Start writing your asset\n\nAdd your reusable prompt, guide, or workflow here."
                  className={cn(inputClassName(Boolean(errors.markdownContent)), "min-h-[24rem] resize-y font-mono text-[13px] leading-6")}
                  {...register("markdownContent", {
                    required: "Markdown content is required.",
                    validate: (value) => value.trim().length > 0 || "Markdown content is required."
                  })}
                />
                <FieldError message={errors.markdownContent?.message} />
              </div>
            </div>
          </Section>
        </div>

        <div className="space-y-6">
          <Section title="Live Preview" description="The live preview reuses the current asset card so the published summary stays visually consistent." delayMs={200} className="xl:sticky xl:top-6">
            <div className="space-y-4">
              <AssetCard asset={previewAsset} showOpenAssetAction={false} />
              <div className="rounded-xl border border-border/70 bg-muted/40 p-4 text-sm text-muted-foreground">
                <p className="font-medium text-foreground">Markdown readiness</p>
                <p className="mt-1">The full Markdown body will be saved with the asset. Keep editing the content area until it reflects the final documentation.</p>
              </div>
            </div>
          </Section>

          <Section title="Publish Controls" description={isEditMode ? "Save your asset changes without creating a second asset." : "Publish once the form and preview are ready."} delayMs={260}>
            <div className="space-y-4">
              <div className="rounded-xl border border-border/70 bg-background p-4 text-sm">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="inline-flex items-center gap-2 font-medium text-foreground">
                      <KeyRound className="size-4 text-primary" aria-hidden="true" />
                      Authorization
                    </p>
                    <p className="mt-1 text-xs text-muted-foreground">
                      Publishing and Markdown upload require authentication. In development, the backend issues the token when you connect; nothing is pasted into this page.
                    </p>
                  </div>
                  <span className={cn(
                    "rounded-full px-2 py-0.5 text-[11px] font-medium",
                    hasAuthorization
                      ? "border border-primary/35 bg-primary/10 text-primary"
                      : "border border-border bg-muted text-muted-foreground"
                  )}>
                    {hasRuntimeToken ? "Session authorized" : "Not authorized"}
                  </span>
                </div>

                <div className="mt-3 flex flex-col gap-3 sm:flex-row">
                  <div className="flex gap-2">
                    <button
                      type="button"
                      className="inline-flex items-center justify-center rounded-lg bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:opacity-60"
                      onClick={() => {
                        void handleConnectDevelopmentAuthorization();
                      }}
                      disabled={isAuthorizing}
                    >
                      {isAuthorizing ? "Connecting..." : "Connect Dev Auth"}
                    </button>
                    <button
                      type="button"
                      className="inline-flex items-center justify-center rounded-lg border border-border bg-card px-3 py-2 text-xs font-medium text-foreground transition-colors hover:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background disabled:cursor-not-allowed disabled:opacity-60"
                      onClick={handleClearAuthorizationToken}
                      disabled={!hasRuntimeToken}
                    >
                      Clear
                    </button>
                  </div>
                </div>
              </div>

              <div className="rounded-xl border border-border/70 bg-background p-4 text-sm text-muted-foreground">
                <p className="font-medium text-foreground">Checklist</p>
                <ul className="mt-2 space-y-2">
                  <li>Title, type, domain, and use case selected</li>
                  <li>At least one compatible tool selected</li>
                  <li>Markdown content ready for publication</li>
                </ul>
              </div>

              <div className="flex flex-col gap-3 sm:flex-row">
                <button
                  type="submit"
                  disabled={!hasAuthorization || isPending || lookupsQuery.isLoading || hasLookupError || hasEditError}
                  className={cn(
                    "inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-4 py-3 text-sm font-semibold text-primary-foreground shadow-sm transition-[background-color,transform,box-shadow] motion-safe:duration-180 motion-safe:ease-out motion-reduce:transition-none hover:bg-primary/90 hover:shadow-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background active:scale-[0.98] motion-reduce:active:scale-100 disabled:cursor-not-allowed disabled:opacity-60",
                    isPending && "shadow-none"
                  )}
                >
                  {isPending ? <LoaderCircle className="size-4 motion-safe:animate-spin motion-reduce:animate-none" aria-hidden="true" /> : <Save className="size-4" aria-hidden="true" />}
                  {isPending ? submittingLabel : submitLabel}
                </button>

                <button
                  type="button"
                  className="inline-flex items-center justify-center rounded-xl border border-border bg-card px-4 py-3 text-sm font-medium text-foreground transition-colors hover:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-background"
                  onClick={handleCancel}
                >
                  Cancel
                </button>
              </div>

              <p className="text-xs text-muted-foreground">
                {!hasAuthorization ? "Connect authorization above before publishing. " : null}
                Need to review published assets first? Visit <Link to={ROUTES.MY_ASSETS} className="font-medium text-primary hover:text-primary/80">My Assets</Link>.
              </p>
            </div>
          </Section>
        </div>
      </form>
    </div>
  );
}