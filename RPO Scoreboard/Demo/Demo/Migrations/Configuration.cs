namespace Demo.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Demo.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Demo.Models.MasterContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Demo.Models.MasterContext context)
        {
            context.Database.Delete();
            context.Database.CreateIfNotExists();

            seedUser(context);
            seedTypes(context);
            seedScores(context);
            seedTasks(context);
            seedMilestones(context);
        }

        private void seedUser(MasterContext context)
        {
            for (int i = 0; i < 1000; i++)
            {
                context.UserProfiles.Add(new UserProfile()
                {
                    UserName = "Joe Jiggty " + i
                });
            }
            context.SaveChanges();
        }

        private void seedTypes(MasterContext context)
        {
            context.PointTypes.Add(new PointType()
            {
                Name = "Attendance"
            });

            context.PointTypes.Add(new PointType()
            {
                Name = "Puzzle"
            });

            context.PointTypes.Add(new PointType()
            {
                Name = "Crosscurricular"
            });

            context.PointTypes.Add(new PointType()
            {
                Name = "Cooperation"
            });

            context.PointTypes.Add(new PointType()
            {
                Name = "Story"
            });

            context.SaveChanges();
        }

        public enum SeedTypes
        {
            Attendance,
            Puzzle,
            Crosscurricular,
            Cooperation,
            Story
        }

        private void seedScores(MasterContext context)
        {
            Random rnd = new Random();

            foreach (UserProfile user in context.UserProfiles)
            {
                /*user.Score.Add(new PointScore()
                {
                    Score = 100
                });*/

                context.PointScores.Add(new PointScore()
                {
                    UserProfile = user,
                    Score = rnd.Next(0,1000),
                    PointPath = context.PointTypes.Single(pt=>pt.Name.Equals("Story"))
                });

            }
            context.SaveChanges();
        }

        private void seedTasks(MasterContext context)
        {
            Random rnd = new Random();
            Array enumVals = Enum.GetValues(typeof(SeedTypes));
            SeedTypes st;
            string typeString = "";

            context.Tasks.Add(new Task()
            {
                Name = "Test Task",
                Description = "This task is for testing purposes only",
                IsMilestone = false,
                Points = 100,
                BonusPoints = 0,
                MaxBonusAwards = 0,
                StartTime = DateTime.Now,
                PointPath = context.PointTypes.Single(pt=>pt.Name.Equals("Puzzle")),
                EndTime = DateTime.Now.AddDays(14)
            });

            context.SaveChanges();

            for (int i = 0; i < 100; i++)
            {
                st = (SeedTypes)enumVals.GetValue(rnd.Next(enumVals.Length));
                typeString = st.ToString();

                context.Tasks.Add(new Task()
                {
                    Name = "Random Task " + i,
                    Description = "A task randomly generated at iteration " + i,
                    IsMilestone = false,
                    Points = rnd.Next(1000),
                    BonusPoints = 0,
                    MaxBonusAwards = 0,
                    StartTime = DateTime.Now,
                    PointPath = context.PointTypes.Single(pt => pt.Name.Equals(typeString)),
                    EndTime = DateTime.Now.AddHours(12141)
                });
            }
            context.SaveChanges();
        }

        String[] linkArray = {
                                @"\Content\images\Gate.png",
                                @"\Content\images\key.png",
                                @"\Content\images\quarter.png"
                             };

        private void seedMilestones(MasterContext context)
        {
            Random rnd = new Random();
            Array enumVals = Enum.GetValues(typeof(SeedTypes));
            SeedTypes st;
            string typeString = "";

            context.Tasks.Add(new Task()
            {
                Name = "Test Milestone",
                Description = "This milestone is for testing purposes only",
                IsMilestone = true,
                Points = rnd.Next(1000),
                BonusPoints = rnd.Next(200),
                MaxBonusAwards = rnd.Next(20),
                StartTime = DateTime.Now,
                PointPath = context.PointTypes.Single(pt => pt.Name.Equals("Attendance")),
                EndTime = DateTime.Now.AddYears(1),
                IconLink = linkArray[rnd.Next(3)]
            });
            context.SaveChanges();

            for (int i = 0; i < 10; i++)
            {
                st = (SeedTypes)enumVals.GetValue(rnd.Next(enumVals.Length));
                typeString = st.ToString();

                context.Tasks.Add(new Task()
                {
                    Name = "Random Milestone " + i,
                    Description = "A milestone randomly generated at iteration " + i,
                    IsMilestone = true,
                    Points = rnd.Next(1000),
                    BonusPoints = rnd.Next(200),
                    MaxBonusAwards = rnd.Next(20),
                    StartTime = DateTime.Now,
                    PointPath = context.PointTypes.Single(pt => pt.Name.Equals(typeString)),
                    EndTime = DateTime.Now.AddHours(12141),
                    IconLink = linkArray[rnd.Next(3)]
                });
            }

            context.SaveChanges();
        }
    }
}
