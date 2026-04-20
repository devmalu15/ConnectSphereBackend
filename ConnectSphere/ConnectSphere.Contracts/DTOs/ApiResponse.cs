namespace ConnectSphere.Contracts.DTOs; 
  
public record ApiResponse<T>(bool Success, T? Data, string? Message, IList<string>? 
Errors = null) 
{ 
    public static ApiResponse<T> Ok(T data, string? message = null) => new(true, 
data, message); 
    public static ApiResponse<T> Fail(string message, IList<string>? errors = null) 
=> new(false, default, message, errors); 
}