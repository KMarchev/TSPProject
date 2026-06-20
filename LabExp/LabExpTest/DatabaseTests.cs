using LabExp.Data;
using LabExp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace TSPProject.Tests
{
    public class DatabaseTests
    {
        private ApplicationDbContext GetDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }


        [Fact]
        public void SaveSubject_ToDatabase()
        {
            using var db = GetDatabase();

            var subject = new Subject
            {
                Name = "Mouse",
                Age = 25
            };


            db.Subjects.Add(subject);
            db.SaveChanges();


            var result = db.Subjects.FirstOrDefault();


            Assert.NotNull(result);
            Assert.Equal("Mouse", result.Name);
            Assert.Equal(25, result.Age);
        }

        [Fact]
        public void SaveSubstance_ToDatabase()
        {
            using var db = GetDatabase();

            var substance = new Substance
            {
                Name = "Chemical X"
            };

            db.Substances.Add(substance);
            db.SaveChanges();


            var saved = db.Substances.FirstOrDefault();


            Assert.Equal("Chemical X", saved.Name);
        }

        [Fact]
        public void SaveScientist_ToDatabase()
        {
            using var db = GetDatabase();

            var scientist = new Scientist
            {
                UserName = "Ivan",
                Email = "Test123@secretcorp.com"
            };

            db.Scientists.Add(scientist);
            db.SaveChanges();


            var saved = db.Scientists.FirstOrDefault();


            Assert.Equal("Ivan", saved.UserName);
            Assert.Equal("Test123@secretcorp.com",saved.Email);
        }

        [Fact]
        public void DeleteSubject_FromDatabase()
        {
            using var db = GetDatabase();


            var subject = new Subject
            {
                Name = "DeleteMe"
            };


            db.Subjects.Add(subject);
            db.SaveChanges();


            db.Subjects.Remove(subject);
            db.SaveChanges();


            Assert.Empty(db.Subjects);
        }

        [Fact]
        public void UpdateSubject_InDatabase()
        {
            using var db = GetDatabase();

            var subject = new Subject
            {
                Name = "Old Name"
            };


            db.Subjects.Add(subject);
            db.SaveChanges();

            Assert.Equal("Old Name", db.Subjects.First().Name);

            subject.Name = "New Name";

            db.SaveChanges();


            var saved = db.Subjects.First();


            Assert.Equal("New Name", saved.Name);
        }
    }
}