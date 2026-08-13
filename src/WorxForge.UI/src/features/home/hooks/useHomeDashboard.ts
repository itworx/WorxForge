import { useQuery } from "@tanstack/react-query";
import { createHomeService } from "../services/homeService";

const homeService = createHomeService();

export function useHomeDashboard() {
  return useQuery({
    queryKey: ["home", "dashboard"],
    queryFn: ({ signal }) => homeService.getHomeDashboard(signal)
  });
}
