// File: WebApp/Program.cs

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Chỉ cần dịch vụ cho Razor Pages
            builder.Services.AddRazorPages();

            // KHÔNG CẦN AddSignalR() hay bất cứ gì khác ở đây nữa

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Vẫn rất quan trọng để phục vụ chat.js
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            app.Run();
        }
    }
}