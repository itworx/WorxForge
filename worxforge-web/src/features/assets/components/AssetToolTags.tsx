interface AssetToolTagsProps {
  tools: string[];
}

const MAX_VISIBLE_TOOLS = 6;

export function AssetToolTags({ tools }: AssetToolTagsProps) {
  if (tools.length === 0) {
    return null;
  }

  const visibleTools = tools.slice(0, MAX_VISIBLE_TOOLS);
  const hiddenCount = tools.length - visibleTools.length;

  return (
    <ul className="contents" aria-label="Compatible tools">
      {visibleTools.map((tool) => (
        <li key={tool} className="rounded-full bg-gray-100 px-2 py-0.5 text-[11px] leading-4 text-gray-600">
          {tool}
        </li>
      ))}
      {hiddenCount > 0 && (
        <li className="rounded-full bg-gray-100 px-2 py-0.5 text-[11px] leading-4 text-gray-600">
          +{hiddenCount}
        </li>
      )}
    </ul>
  );
}
