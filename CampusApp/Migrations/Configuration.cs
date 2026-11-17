namespace CampusApp.Migrations
{
    using CampusApp.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CampusApp.Data.CampusContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "CampusApp.Data.CampusContext";
        }

        protected override void Seed(CampusApp.Data.CampusContext context)
        {
            // Seed is called every time the database is updated, so AddOrUpdate keeps
            // the rows unique by campus name / student name instead of duplicating them.
            var campuses = new List<UniversityCampus>
            {
                new UniversityCampus { Name = "Albany Campus" },
                new UniversityCampus { Name = "Manawatu Campus" },
                new UniversityCampus { Name = "Wellington Campus" }
            };

            campuses.ForEach(c => context.UniversityCampuses.AddOrUpdate(cc => cc.Name, c));
            context.SaveChanges();

            var students = new List<Student>
            {
                new Student { Name = "Ben Carter", Address = "5 Massey Rd, Albany", CampusID = campuses.Single(c => c.Name == "Albany Campus").ID },
                new Student { Name = "Emma Wilson", Address = "18 Oteha Valley Rd, Albany", CampusID = campuses.Single(c => c.Name == "Albany Campus").ID },
                new Student { Name = "Liam Scott", Address = "36 Hobsonville Rd, Albany", CampusID = campuses.Single(c => c.Name == "Albany Campus").ID },
                new Student { Name = "Aroha Patel", Address = "12 Fitzherbert Ave, Palmerston North", CampusID = campuses.Single(c => c.Name == "Manawatu Campus").ID },
                new Student { Name = "Daniel King", Address = "44 Broadway, Palmerston North", CampusID = campuses.Single(c => c.Name == "Manawatu Campus").ID },
                new Student { Name = "Hemi Rangi", Address = "23 Kelvin Grove Rd, Palmerston North", CampusID = campuses.Single(c => c.Name == "Manawatu Campus").ID },
                new Student { Name = "Chen Wei", Address = "9 Cuba St, Wellington", CampusID = campuses.Single(c => c.Name == "Wellington Campus").ID },
                new Student { Name = "Mia Chen", Address = "7 Vivian St, Wellington", CampusID = campuses.Single(c => c.Name == "Wellington Campus").ID },
                new Student { Name = "Priya Sharma", Address = "150 Lambton Quay, Wellington", CampusID = campuses.Single(c => c.Name == "Wellington Campus").ID }
            };

            students.ForEach(s => context.Students.AddOrUpdate(st => st.Name, s));
            context.SaveChanges();
        }
    }
}
