import { ApiError, type ProblemDetails } from './ApiError';

export async function handleResponse<T>(response: Response): Promise<{ data: T }> {
  if (response.status === 401) {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    window.location.reload();
    throw new ApiError(401);
  }

  if (!response.ok) {
    let problems: ProblemDetails | undefined;
    try {
      problems = await response.json();
    } catch {
      // response body is not JSON — leave problems undefined
    }
    throw new ApiError(response.status, problems);
  }

  if (response.status === 204) {
    return { data: undefined as T };
  }

  const data: T = await response.json();
  return { data };
}
