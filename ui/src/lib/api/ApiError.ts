export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  readonly status: number;
  readonly detail: string;
  readonly problems?: ProblemDetails;

  constructor(status: number, problems?: ProblemDetails) {
    const detail = problems?.detail ?? `Request failed: ${status}`;
    super(detail);
    this.name = 'ApiError';
    this.status = status;
    this.detail = detail;
    this.problems = problems;
  }
}
