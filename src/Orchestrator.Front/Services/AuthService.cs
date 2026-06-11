using Microsoft.JSInterop;

namespace Orchestrator.Front.Services;

public class AuthService
{
    private readonly OrchestratorApiClient _apiClient;
    private readonly IJSRuntime _jsRuntime;
    private string? _token;

    public event Action? OnAuthStateChanged;

    public AuthService(OrchestratorApiClient apiClient, IJSRuntime jsRuntime)
    {
        _apiClient = apiClient;
        _jsRuntime = jsRuntime;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public async Task InitializeAsync()
    {
        try
        {
            _token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
            if (!string.IsNullOrEmpty(_token))
            {
                _apiClient.SetToken(_token);
            }
        }
        catch
        {
            // Safe fallback
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var token = await _apiClient.LoginAsync(username, password);
        if (token != null)
        {
            _token = token;
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            OnAuthStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public async Task LogoutAsync()
    {
        _token = null;
        _apiClient.ClearToken();
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }
        catch
        {
            // Safe fallback
        }
        OnAuthStateChanged?.Invoke();
    }
}
