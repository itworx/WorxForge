export type ApiEnvelope<T> = {
  success: boolean;
  data: T;
  message?: string;
};
