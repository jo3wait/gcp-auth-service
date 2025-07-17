using AuthService.Application.Interfaces;
using AuthService.Application.Services;
using AuthService.Domain;
using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMemberService, MemberService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 註冊 CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
          .WithOrigins("https://image-frontend-821112036618.asia-east1.run.app",
                       "https://image-frontend-4sfwixwraa-de.a.run.app")
          .AllowAnyHeader()
          .AllowAnyMethod();
    });
});

// 2. 新增 Authentication：Local JWT + Google ID Token
// 2.1 先定義一個「Policy Scheme」自動挑選驗證方案
builder.Services
  .AddAuthentication(options =>
  {
      // 用這個 scheme 來挑選 Local vs Google
      options.DefaultScheme = "CombinedJwt";
  })
  // PolicyScheme 會根據 token 的 issuer 自動切到 Local 或 Google
  .AddPolicyScheme("CombinedJwt", JwtBearerDefaults.AuthenticationScheme, options =>
  {
      options.ForwardDefaultSelector = context =>
      {
          var authHeader = context.Request.Headers["Authorization"].ToString();
          if (authHeader.StartsWith("Bearer "))
          {
              var token = authHeader.Substring("Bearer ".Length).Trim();
              try
              {
                  var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token);
                  // 如果 issuer 是 Google，就用 google‐bearer
                  if (jwt.Issuer == "https://accounts.google.com" ||
                      jwt.Issuer == "accounts.google.com")
                      return "GoogleJwt";
              }
              catch
              {
                  // 忽略解析錯誤，改回 local
              }
          }
          // 其它預設都走自簽 JWT
          return "LocalJwt";
      };
  })
  // 2.2 你的本地對稱金鑰 JwtBearer
  .AddJwtBearer("LocalJwt", options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration["Jwt:Issuer"],
          ValidAudience = builder.Configuration["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(
          Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
      };
  })
  // 2.3 Google ID Token 驗證
  .AddJwtBearer("GoogleJwt", options =>
  {
      // 來源：https://cloud.google.com/run/docs/securing/authenticating/service-to-service
      options.Authority = "https://accounts.google.com";
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidIssuers = new[] {
        "https://accounts.google.com",
        "accounts.google.com"
      },
          ValidateAudience = true,
          ValidAudience = builder.Configuration["Google:CloudRunUrl"],
          ValidateLifetime = true,
      };
  });

// 2.4 把 LocalJwt + GoogleJwt 都納入預設授權政策
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
        "LocalJwt", "GoogleJwt")
      .RequireAuthenticatedUser()
      .Build();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();          // 只在本機 dev 啟用
}
// 使用 CORS
app.UseCors("FrontendPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
