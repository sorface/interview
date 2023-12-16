using CSharpFunctionalExtensions;
using Interview.Domain.ServiceResults;
using Interview.Domain.ServiceResults.Errors;
using Interview.Domain.ServiceResults.Success;
using Microsoft.AspNetCore.Mvc;

namespace Interview.Backend.Responses
{
    public static class ResultExt
    {
        public static async Task<ActionResult<T>> ToResponseAsync<T>(this Task<Result<ServiceResult<T>, ServiceError>> self)
        {
            var result = await self;
            return result.ToResponse();
        }

        public static ActionResult<T> ToResponse<T>(this Result<ServiceResult<T>, ServiceError> self)
        {
            return self.Match(
                success => success.ToActionResult(),
                error => error.ToActionResult<T>());
        }

        public static async Task<ActionResult<T>> ToResponseAsync<T>(this Task<Result<ServiceResult, ServiceError>> self)
        {
            var result = await self;
            return result.ToResponse<T>();
        }

        public static ActionResult<T> ToResponse<T>(this Result<ServiceResult, ServiceError> self)
        {
            return self.Match(
                success => success.ToActionResult<T>(),
                error => error.ToActionResult<T>());
        }

        public static async Task<ActionResult> ToResponseAsync(this Task<Result<ServiceResult, ServiceError>> self)
        {
            var result = await self;
            return result.ToResponse();
        }

        public static ActionResult ToResponse(this Result<ServiceResult, ServiceError> self)
        {
            return self.Match(
                success => success.ToActionResult(),
                error => error.ToActionResult());
        }
    }
}
