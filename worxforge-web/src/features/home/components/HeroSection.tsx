import type { LucideIcon } from "lucide-react";
import { Search, FilePlus2, ThumbsUp, SquarePen } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { SearchBar } from "../../../components/SearchBar";
import { Logo } from "../../../layouts/components/Logo";
import { ROUTES } from "../../../constants/routes.constants";

interface HeroFeature {
  icon: LucideIcon;
  title: string;
  description: string;
}

const HERO_FEATURES: HeroFeature[] = [
  { icon: Search, title: "Find", description: "Search across every prompt, agent, and standard in the org." },
  { icon: FilePlus2, title: "Create", description: "Create your own prompt, agent, or standard and share it with the team." },
  { icon: ThumbsUp, title: "React", description: "Tell the community what worked — and what didn't." },
  { icon: SquarePen, title: "Improve", description: "Suggest updates or publish your own to help the team." }
];

export function HeroSection() {
  const navigate = useNavigate();

  function handleSearch(query: string) {
    if (!query) return;
    navigate(`${ROUTES.EXPLORE}?q=${encodeURIComponent(query)}`);
  }

  return (
    <section className="mx-auto max-w-3xl text-center">
      <Logo className="mx-auto size-14 rounded-2xl shadow-lg" />

      <h1 className="mt-5 text-3xl font-bold tracking-tight md:text-4xl">What knowledge do you need?</h1>

      <p className="mt-3 text-muted-foreground">
        The Knowledge Hub inside WorxGPT — every prompt, agent, standard, and workflow your team has built, in one
        searchable place.
      </p>

      <SearchBar
        className="mt-6"
        placeholder='Try: "prompt for generating xUnit tests" or "API security review"'
        onSearch={handleSearch}
      />

      <div className="mt-8 grid grid-cols-2 gap-3 md:grid-cols-4">
        {HERO_FEATURES.map(({ icon: Icon, title, description }) => (
          <div key={title} className="rounded-xl border border-secondary/30 bg-white p-4 text-left shadow-sm">
            <Icon aria-hidden="true" className="h-5 w-5 text-primary" />
            <div className="mt-2 text-sm font-semibold">{title}</div>
            <div className="mt-0.5 text-xs text-muted-foreground">{description}</div>
          </div>
        ))}
      </div>
    </section>
  );
}
