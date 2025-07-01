namespace AuthService.Application.DTOs;

public record AuthResponse(bool Success, string Message, string? Token = null);
