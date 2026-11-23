using FMMSMachineMonitoring.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace FMMSMachineMonitoring.Data
{
    public static class DbSeeder
    {
        public static async Task SeedUsers(IServiceProvider service)
        {
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Operator", "Technician", "Supervisor", "Manager" };

            foreach (var role in roles)
            {
                //ถามว่า role นี้มีอยู่ในฐานข้อมูลหรือไม่ ถ้าไม่มีให้สร้าง role ใหม่
                if (!await roleManager.RoleExistsAsync(role))
                {
                    //สร้าง role ใหม่
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            //seed user สำหรับการทดสอบ
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();

            var demoUsers = new[]
            {
            (UserName: "OP001",   Role: "Operator"),
            (UserName: "TECH001", Role: "Technician"),
            (UserName: "SUP001",  Role: "Supervisor"),
            (UserName: "MGR001",  Role: "Manager"),
            };

            foreach (var item in demoUsers)
            {
                //ถามว่า user นี้มีอยู่ในฐานข้อมูลหรือไม่ ถ้าไม่มีให้สร้าง user ใหม่
                if (await userManager.FindByNameAsync(item.UserName) == null)
                {
                    var user = new IdentityUser { UserName = item.UserName };
                    var created = await userManager.CreateAsync(user, "Password123!"); //รหัสผ่านเริ่มต้นสำหรับผู้ใช้ทดสอบ
                    if (created.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, item.Role);
                    }

                }
            }
        }

        public static async Task SeedTechnicians(IServiceProvider service)
        {
            var context = service.GetRequiredService<AppDbContext>();
            //ตรวจสอบว่ามี Technician อยู่แล้วหรือไม่ ถ้าไม่มีให้เพิ่ม Technician ใหม่
            var userManager = service.GetRequiredService<UserManager<IdentityUser>>();

            var techUser = await userManager.FindByNameAsync("TECH001"); //ค้นหา user ที่มีชื่อ "TECH001" ในฐานข้อมูล ถ้าเจอจะได้ object ของ user นั้น
            if (techUser == null) return;//ถ้าไม่เจอ user "TECH001" ให้ return ออกจาก method

            var exists = await context.Technicians.AnyAsync(t => t.UserId == techUser.Id); //ตรวจสอบว่ามี Technician ที่มี UserId ตรงกับ Id ของ user "TECH001"
            if (!exists) //ถ้าไม่มี Technician ที่มี UserId ตรงกับ Id ของ user "TECH001" ให้เพิ่ม Technician ใหม่
            {
                context.Technicians.Add(new Technician
                {
                    Name = "สมชาย ใจดี",
                    Specialization = "เครื่องจักรกล",
                    UserId = techUser.Id
                });
                await context.SaveChangesAsync();
            }


        }

    }
}