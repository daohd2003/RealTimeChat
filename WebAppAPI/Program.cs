// File: WebAppAPI/Program.cs

using Core.Interfaces;
using Core.Models;
using Infrastructure.Services;
using WebApp.Hubs;

namespace WebAppAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            // 1. Cấu hình CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        // THAY BẰNG URL CỦA DỰ ÁN WEBAPP (RAZOR)
                        policy.WithOrigins("https://localhost:7106")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials(); // <-- Bắt buộc cho SignalR
                    });
            });

            // 2. Đăng ký dịch vụ
            builder.Services.AddHttpClient();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IChatService, FirebaseChatService>();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // 3. Cấu hình pipeline middleware theo đúng thứ tự
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // UseRouting đánh dấu vị trí quyết định định tuyến được thực hiện.
            app.UseRouting();

            // CORS phải được đặt sau UseRouting và trước UseAuthorization/Endpoints.
            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();

            // Map các endpoint sau khi đã cấu hình xong pipeline.
            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
