using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Glowtics.Api.Responses
{
    public class ApiResponse
    {
        public bool IsSuccess { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorCode { get; }
        public string Message { get; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Errors { get; }

        protected ApiResponse(bool isSuccess, string? errorCode, string message, List<string>? errors)
        {
            IsSuccess = isSuccess;
            ErrorCode = errorCode;
            Message = message;
            Errors = isSuccess ? null : (errors ?? new List<string>());
        }

        public static ApiResponse Success(string message = "Request completed successfully.")
            => new ApiResponse(true, null, message, null);

        public static ApiResponse Failure(string errorCode, string message, List<string> errors)
            => new ApiResponse(false, errorCode, message, errors);

        public static ApiResponse<T> Success<T>(T data, string message = "Request completed successfully.")
            => ApiResponse<T>.Success(data, message);

        public static ApiResponse<T> Failure<T>(string errorCode, string message, List<string> errors)
            => ApiResponse<T>.Failure(errorCode, message, errors);
    }

    public class ApiResponse<T> : ApiResponse
    {
        [JsonPropertyOrder(5)]
        public T Data { get; }

        private ApiResponse(T data, string message)
            : base(true, null, message, null)
        {
            Data = data;
        }

        private ApiResponse(string errorCode, string message, List<string> errors)
            : base(false, errorCode, message, errors)
        {
            Data = default!;
        }

        public static ApiResponse<T> Success(T data, string message = "Request completed successfully.")
            => new ApiResponse<T>(data, message);

        public static new ApiResponse<T> Failure(string errorCode, string message, List<string> errors)
            => new ApiResponse<T>(errorCode, message, errors);
    }
}
