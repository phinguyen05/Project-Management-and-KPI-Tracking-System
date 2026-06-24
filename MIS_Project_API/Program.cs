using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MIS_Project_API.Interfaces;
using MIS_Project_API.Models;
using MIS_Project_API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình CORS hợp nhất ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
// ĐÃ XÓA CÁC CẤU HÌNH SWAGGER / API EXPLORER TẠI ĐÂY

// Cấu hình DatabaseContext
builder.Services.AddDbContext<MisProjectManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Authentication JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Đăng ký các Services
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ILogTimeBackgroundService, LogTimeBackgroundService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IKpiService, KpiService>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
builder.Services.AddHostedService<TaskStatusUpdaterService>();

// Cấu hình Hangfire
builder.Services.AddHangfire(config => config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();
builder.Services.AddAuthorization();

var app = builder.Build();



// ĐÃ XÓA KHỐI LỆNH TẠO TRANG GIAO DIỆN SWAGGER UI

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseHangfireDashboard();

// Đăng ký các Cron Job chạy hàng ngày (không chạy NotificationController nữa vì đã chuyển sang BackgroundService)
RecurringJob.AddOrUpdate<IBackgroundJobService>("CheckOverdueTasks", service => service.CheckAndAlertOverdueTasksAsync(), Cron.Daily);
RecurringJob.AddOrUpdate<IBackgroundJobService>("AutoApprovePendingLogTimes", service => service.AutoApprovePendingLogTimesAsync(), Cron.Daily);


app.Run();