
using FMMSMachineMonitoring.Data;
using FMMSMachineMonitoring.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();

// Connect DB
builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseSqlServer
        (builder.Configuration.GetConnectionString
        ("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// Authorization policy
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});


// Service
builder.Services.AddHostedService<SensorSimulationService>();
builder.Services.AddHostedService<PreventiveMaintenanceService>();


var app = builder.Build();

using (var scope = app.Services.CreateScope()) //เราใช้ CreateScope() เพื่อสร้าง scope ใหม่สำหรับการเข้าถึง service provider
{
    await DbSeeder.SeedUsers(scope.ServiceProvider); //เราเรียกใช้เมธอด SeedUsers() ของคลาส DbSeeder เพื่อทำการ seed ข้อมูลผู้ใช้และ role ลงในฐานข้อมูล
    await DbSeeder.SeedTechnicians(scope.ServiceProvider); //เราเรียกใช้เมธอด SeedTechnicians() ของคลาส DbSeeder เพื่อทำการ seed ข้อมูลช่างลงในฐานข้อมูล
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); //ตรวจสอบการยืนยันตัวตนของผู้ใช้ก่อนเข้าถึง resource ต่าง ๆ ของแอปพลิเคชัน
app.UseAuthorization(); //ตรวจสอบสิทธิ์ของผู้ใช้ก่อนเข้าถึง resource ต่าง ๆ ของแอปพลิเคชัน
app.MapStaticAssets().AllowAnonymous(); //อนุญาตให้ผู้ใช้ที่ไม่ได้เข้าสู่ระบบสามารถเข้าถึง static assets ได้
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
