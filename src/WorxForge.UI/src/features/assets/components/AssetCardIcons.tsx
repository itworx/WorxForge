import { FaBookmark, FaRegBookmark, FaRegComment, FaRegThumbsUp, FaRegPenToSquare, FaRegTrashCan, FaEllipsisVertical } from "react-icons/fa6";

interface IconProps {
  className?: string;
}

export function ReactionIcon({ className }: IconProps) {
  return <FaRegThumbsUp aria-hidden="true" className={className} />;
}

export function CommentIcon({ className }: IconProps) {
  return <FaRegComment aria-hidden="true" className={className} />;
}

export function EditIcon({ className }: IconProps) {
  return <FaRegPenToSquare aria-hidden="true" className={className} />;
}

export function DeleteIcon({ className }: IconProps) {
  return <FaRegTrashCan aria-hidden="true" className={className} />;
}

export function MoreIcon({ className }: IconProps) {
  return <FaEllipsisVertical aria-hidden="true" className={className} />;
}

interface BookmarkIconProps extends IconProps {
  filled: boolean;
}

export function BookmarkIcon({ className, filled }: BookmarkIconProps) {
  const Icon = filled ? FaBookmark : FaRegBookmark;
  return <Icon aria-hidden="true" className={className} />;
}
