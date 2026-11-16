using Microsoft.AspNetCore.Identity;
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

    }
}