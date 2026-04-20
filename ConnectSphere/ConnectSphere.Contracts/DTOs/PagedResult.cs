namespace ConnectSphere.Contracts.DTOs; 
  
public record PagedResult<T>(IList<T> Items, int Page, int PageSize, int 
TotalCount) 
{ 
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); 
} 