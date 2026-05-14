using System.Security.Claims;

namespace Inventory.Services;

public interface ICurrentUser { int Id { get; } string Name { get; } }

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public CurrentUser(IHttpContextAccessor http) { _http = http; }
    public int Id => int.Parse(_http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    public string Name => _http.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? "";
}
