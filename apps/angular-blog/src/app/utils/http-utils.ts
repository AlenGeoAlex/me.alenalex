import {HttpErrorResponse, HttpResponse} from '@angular/common/http';

export type ProblemDetails = {
  type: string;
  title: string;
  status: number;
  detail: string;
  code: string;
};

export async function asProblemDetailsAsync(error: any) : Promise<ProblemDetails> {
  if (error instanceof HttpErrorResponse){
    return await error.error as ProblemDetails;
  }

  return {
    type: "https://httpstatuses.com/500",
    title: "Unknown Error",
    status :500,
    detail :"An unknown error occurred in an unknown shape!",
    code: "God.Knows.This"
  }
}
