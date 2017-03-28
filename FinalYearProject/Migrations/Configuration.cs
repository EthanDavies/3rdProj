namespace FinalYearProject.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FinalYearProject.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "FinalYearProject.Models.ApplicationDbContext";
        }

        protected override void Seed(FinalYearProject.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            //context.Roles.AddOrUpdate(r => r.Name,
            //    new IdentityRole { Name = "Admin" },
            //    new IdentityRole { Name = "Lecturer" },
            //    new IdentityRole { Name = "Student" });

            var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            string[] roleNames = { "Admin", "Lecturer", "Student" };
            IdentityResult roleResult;
            foreach (var roleName in roleNames)
            {
                if (!RoleManager.RoleExists(roleName))
                {
                    roleResult = RoleManager.Create(new IdentityRole(roleName));
                }
            }

            string pass = "password";
           

            var lecturer = new ApplicationUser { UserName = "Lecturer@email.com", Role = roleNames[1] };

            var um = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));
            um.Create(lecturer, pass);
            um.AddToRole("43a22149-3983-4484-aa1f-57b24196228a", roleNames[1]);

            var student1 = new ApplicationUser { UserName = "Student1@email.com", Role = roleNames[2] };
            um.Create(student1, pass);
            um.AddToRole("8affe221-b8d2-411f-af44-ff769fe95226", roleNames[2]);
            var student2 = new ApplicationUser { UserName = "Student2@email.com", Role = roleNames[2] };
            um.Create(student2, pass);
            um.AddToRole("37dc6ba8-1287-4716-977a-1944eab4174c", roleNames[2]);
            var student3 = new ApplicationUser { UserName = "Student3@email.com", Role = roleNames[2] };
            um.Create(student3, pass);
            um.AddToRole("6d8534aa-fbc7-4afe-b77a-ad09a6805040", roleNames[2]);
            var student4 = new ApplicationUser { UserName = "Student4@email.com", Role = roleNames[2] };
            um.Create(student4, pass);
            um.AddToRole("be64a80c-aa25-4074-8329-56679a8d550d", roleNames[2]);
            var student5 = new ApplicationUser { UserName = "Student5@email.com", Role = roleNames[2] };
            um.Create(student5, pass);
            um.AddToRole("c7c680f1-7c6f-4545-9603-15784f62ee42", roleNames[2]);

            /*um.Create(student1, pass);
            um.Create(student2, pass);
            um.Create(student3, pass);
            um.Create(student4, pass);
            um.Create(student5, pass);*/

        }


    }
}
