using api.Controllers;
using api.Entities;
using api.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_tests.Controller
{
    [TestClass]
    public class UserControllerTests
    {
        private UserController userController;

        private Mock<IUserRepo> userRepo;

        private List<AppUser> users = new List<AppUser>
        {
                new AppUser
                {
                    Id = 1,
                    UserName = "test",
                    PasswordHash = Encoding.UTF8.GetBytes("password"),
                    PasswordSalt = Encoding.UTF8.GetBytes("key")

                },

                new AppUser
                {
                    Id = 2,
                    UserName = "test2",
                    PasswordHash = Encoding.UTF8.GetBytes("password"),
                    PasswordSalt = Encoding.UTF8.GetBytes("key")

                }
                   

        };

        [TestInitialize]
        public void Setup()
        {
            userRepo = new Mock<IUserRepo>();
            userController = new UserController(userRepo.Object);


        }

        [TestMethod]

        public async Task GetUsersShouldReturnAllUsers()
        {
            //Arrange
            userRepo.Setup(repo => repo.GetAll()).ReturnsAsync(users);

            //Act
            var actualUsers = await userController.GetUsers() as OkObjectResult;

            //Assert
            userRepo.Verify(userRepo => userRepo.GetAll(), Times.Once());
            Assert.AreEqual(users, actualUsers.Value);
            


        }

        [TestMethod]

        public async Task GetUserShouldReturnCorrectUserBasedOnIdProp()
        {
            //Arrange
            var user1 = users[0];
            userRepo.Setup(repo => repo.GetUser(1)).ReturnsAsync(user1);
            //Act
            var actualUser = await userController.GetUser(1) as OkObjectResult;

            //Assert
            userRepo.Verify(userRepo => userRepo.GetUser(1), Times.Once());
            Assert.AreEqual(user1, actualUser.Value);

        }

    }
}
