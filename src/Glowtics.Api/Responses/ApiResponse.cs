using System.Collections.Generic;

namespace Glowtics.Api.Responses
{
    public class ApiResponse
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public List<string> Errors { get; }

        protected ApiResponse(bool isSuccess, string message, List<string> errors)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? new List<string>();
        }

        public static ApiResponse Success(string message = "Request completed successfully.")
            => new ApiResponse(true, message, new List<string>());

        public static ApiResponse Failure(string message, List<string> errors)
            => new ApiResponse(false, message, errors);

        public static ApiResponse<T> Success<T>(T data, string message = "Request completed successfully.")
            => ApiResponse<T>.Success(data, message);

        public static ApiResponse<T> Failure<T>(string message, List<string> errors)
            => ApiResponse<T>.Failure(message, errors);
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; }

        private ApiResponse(T data, string message)
            : base(true, message, new List<string>())
        {
            Data = data;
        }

        private ApiResponse(string message, List<string> errors)
            : base(false, message, errors)
        {
            Data = default!;
        }

        public static ApiResponse<T> Success(T data, string message = "Request completed successfully.")
            => new ApiResponse<T>(data, message);

        public static new ApiResponse<T> Failure(string message, List<string> errors)
            => new ApiResponse<T>(message, errors);
    }
}
