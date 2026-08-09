import { afterEach } from "vitest";
import { cleanup } from "@testing-library/react";

Object.defineProperty(window, "scrollTo", {
  value: () => undefined,
  writable: true
});

afterEach(() => {
  cleanup();
});