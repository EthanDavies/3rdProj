namespace FinalYearProject.Migrations.BlogDbContext
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<FinalYearProject.Models.BlogDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            MigrationsDirectory = @"Migrations\BlogDbContext";
        }

        protected override void Seed(Models.BlogDbContext context)
        {

            context.Categories.AddOrUpdate(new Models.Category { Id = "cat1", Name = "CS348", UrlSeo = "CS348", Description = "Web Development" });
            context.Categories.AddOrUpdate(new Models.Category { Id = "cat2", Name = "CS170", UrlSeo = "CS170", Description = "Relational Algebra" });
            context.Categories.AddOrUpdate(new Models.Category { Id = "cat3", Name = "CS150", UrlSeo = "CS150", Description = "Programming 1" });
            context.Categories.AddOrUpdate(new Models.Category { Id = "cat4", Name = "CS270", UrlSeo = "CS270", Description = "Databases" });
            context.Categories.AddOrUpdate(new Models.Category { Id = "cat5", Name = "CS306", UrlSeo = "CS306", Description = "Mobile Apps" });
            context.Categories.AddOrUpdate(new Models.Category { Id = "cat6", Name = "CSP300", UrlSeo = "CSP300", Description = "Disseration Module" });


            context.Posts.AddOrUpdate(new Models.Post
            {
                Id = "1",
                Title = "Coursework 2 & 3 Reminder",
                Body = "The coursework is due soon, don't worry have faith in yourself and follow my tutorials and you will do fine!",
                ShortDescription = "Don't panic, you can do this!",
                PostedOn = DateTime.Now,
                UrlSeo = "Coursework",
                Meta = "Coursework",
                Published = true
            });

            context.Posts.AddOrUpdate(new Models.Post
            {
                Id = "2",
                Title = "Labs Cancelled",
                Body = "Just to let you all know that the labs this week are cancelled becuase I am quite ill, aplogies.",
                ShortDescription = "No labs friday 13th",
                PostedOn = DateTime.Now,
                UrlSeo = "Labs",
                Meta = "Labs",
                Published = true
            });

            context.Posts.AddOrUpdate(new Models.Post
            {
                Id = "3",
                Title = "Deadline Extension",
                Body = "The deadline for the mobile apps coursework has been extended, the deadline will now be on the 18th not the 13th",
                ShortDescription = "Mobile apps deadline extension",
                PostedOn = DateTime.Now,
                UrlSeo = "Extension",
                Meta = "Extension",
                Published = true
            });

            context.Posts.AddOrUpdate(new Models.Post
            {
                Id = "4",
                Title = "Gregynog Feedback",
                Body = "Please go an see your tutor if you have not yet recieved feedback for your gregynog presentation",
                ShortDescription = "Collect your feedback",
                PostedOn = DateTime.Now,
                UrlSeo = "Gregynog",
                Meta = "Gregynog",
                Published = true
            });

            context.PostCategories.AddOrUpdate(new Models.PostCategory { PostId = "1", CategoryId = "cat1" });
            context.PostCategories.AddOrUpdate(new Models.PostCategory { PostId = "3", CategoryId = "cat5" });
            context.PostCategories.AddOrUpdate(new Models.PostCategory { PostId = "2", CategoryId = "cat5" });
            context.PostCategories.AddOrUpdate(new Models.PostCategory { PostId = "4", CategoryId = "cat6" });

            context.Comments.AddOrUpdate(new Models.Comment { Id = "cmt1", PostId = "1", Body = "Thanks for the motivation! almost there.", DateTime = DateTime.Now, UserName = "Student1@email.com" });
            context.Comments.AddOrUpdate(new Models.Comment { Id = "cmt2", PostId = "2", Body = "Such a shame!", DateTime = DateTime.Now, UserName = "Student3@email.com" });
            context.Comments.AddOrUpdate(new Models.Comment { Id = "cmt3", PostId = "1", Body = "Found a new tutorial online, progress is going good!", DateTime = DateTime.Now, UserName = "Student2@email.com" });
            context.Comments.AddOrUpdate(new Models.Comment { Id = "cmt4", PostId = "3", Body = "Another extension...?!", DateTime = DateTime.Now, UserName = "Student3@email.com" });



            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep1", PostId = "1", CommentId = "cmt3", DateTime = DateTime.Now, UserName = "Student3@email.com", Body = "Cras sodales justo sit amet libero placerat consectetur. Duis hendrerit facilisis tempor. Nullam ut nisl nec neque posuere semper. Praesent vestibulum id purus quis maximus." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep9", PostId = "1", CommentId = "cmt3", DateTime = DateTime.Now, UserName = "Student3@email.com", Body = "Cras sodales justo sit amet libero placerat consectetur. Duis hendrerit facilisis tempor. Nullam ut nisl nec neque posuere semper. Praesent vestibulum id purus quis maximus." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep2", PostId = "1", CommentId = "cmt3", ParentReplyId = "rep1", DateTime = DateTime.Now, UserName = "Student4@email.com", Body = "Mauris pulvinar tristique libero id ornare. Quisque sit amet accumsan leo. Vestibulum dapibus elit sed lorem lacinia suscipit. In hac habitasse platea dictumst. Vivamus egestas leo eu nulla faucibus cursus." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep3", PostId = "1", CommentId = "cmt3", ParentReplyId = "rep2", DateTime = DateTime.Now, UserName = "Student4@email.com", Body = "Suspendisse consequat dolor urna, sit amet accumsan lectus luctus eget. Vestibulum maximus ante vel placerat cursus. Nulla luctus augue ac vulputate aliquet." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep4", PostId = "1", CommentId = "cmt3", ParentReplyId = "rep3", DateTime = DateTime.Now, UserName = "Student5@email.com", Body = "Donec aliquam, sem a tincidunt tincidunt, orci velit mollis magna, vel auctor arcu augue nec risus. Integer luctus enim ac viverra luctus." });

            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep5", PostId = "1", CommentId = "cmt2", DateTime = DateTime.Now, UserName = "Student4@email.com", Body = "Cras sodales justo sit amet libero placerat consectetur. Duis hendrerit facilisis tempor. Nullam ut nisl nec neque posuere semper. Praesent vestibulum id purus quis maximus." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep6", PostId = "1", CommentId = "cmt2", ParentReplyId = "rep5", DateTime = DateTime.Now, UserName = "Student2@email.com", Body = "Mauris pulvinar tristique libero id ornare. Quisque sit amet accumsan leo. Vestibulum dapibus elit sed lorem lacinia suscipit." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep7", PostId = "1", CommentId = "cmt2", ParentReplyId = "rep6", DateTime = DateTime.Now, UserName = "Student1@email.com", Body = "Suspendisse consequat dolor urna, sit amet accumsan lectus luctus eget. Vestibulum maximus ante vel placerat cursus. Nulla luctus augue ac vulputate aliquet." });
            context.Replies.AddOrUpdate(new Models.Reply { Id = "rep8", PostId = "1", CommentId = "cmt2", ParentReplyId = "rep7", DateTime = DateTime.Now, UserName = "Student2@email.com", Body = "Donec aliquam, sem a tincidunt tincidunt, orci velit mollis magna, vel auctor arcu augue nec risus. Integer luctus enim ac viverra luctus." });
        }

        
    }
}
