using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Server.Controllers;
using Server.Data;
using Server.DTOs;
using Server.Interfaces;
using Server.Models;
using Server.Profiles;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Xunit;

namespace Server.Tests
{
    public class InformationCardsControllerTests
    {
        [Fact]
        public void GetInformationCards_WithAListOfCards_ReturnAListOfCards()
        {
            // Arrange

            var repository = GenerateMockRepository();

            repository.Setup(repo => repo.GetAllInformationCards()).Returns(GetTestCards());

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);

            // Act
            var result = controller.GetInformationCards();

            // Assert

            // 1. Checking output result for Not Null
            Assert.NotNull(result);

            // 2. Checking output result for output object type as ActionResult<IEnumerable<InformationCardReadDto>>
            var actionResult = Assert.IsType<ActionResult<IEnumerable<InformationCardReadDto>>>(result);

            // 3. Checking output result for object type as OkObjectResult (200 OK)
            var model = Assert.IsAssignableFrom<OkObjectResult>(actionResult.Result);

            // 4. Checking if count of the returned objects equal to the real objects count
            var modelAsAList = model.Value as List<InformationCardReadDto>;
            Assert.Equal(GetTestCards().Count, modelAsAList.Count);
        }

        [Fact]
        public void GetInformationCardById_WithAnExistingId_ReturnItemWithPassedIndex()
        {
            // Arrange
            var repository = GenerateMockRepository();
            repository.Setup(repo => repo.GetInformationCardById(1)).Returns(GetTestCards().FirstOrDefault(e => e.Id == 1));

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);

            // Act
            var result = controller.GetInformationCardById(1);

            // Assert

            // 1. Checking output result for object type as OkObjectResult (200 OK)
            var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result.Result);

            // 2. Checking if data is right
            var model = Assert.IsType<InformationCardReadDto>(actionResult.Value);
            Assert.Equal(1, model.Id);
            Assert.Equal("Dog", model.Name);
            Assert.Equal("../../../Data/Images/Dog.jpg", model.Image);
        }

        [Fact]
        public void GetInformationCardById_WithNotExistingId_ReturnNotFound()
        {
            // Arrange
            var repository = GenerateMockRepository();
            repository.Setup(repo => repo.GetInformationCardById(10)).Returns(GetTestCards().FirstOrDefault(e => e.Id == 10));

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);

            // Act
            var result = controller.GetInformationCardById(10);

            // Assert

            // 1. Checking output result for type as NotFoundResult (404 Not Found)
            var actionResult = Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void AddNewInformationCard_WithInformationCardModel_ReturnCreatedAtRoute()
        {
            // Arrange
            var repository = GenerateMockRepository();

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);

            // Act

            ActionResult<InformationCardReadDto> actionResult = controller.AddNewInformationCard(new InformationCardCreateDto
            {
                Name = "Parrot",
                Image = "../../../Data/Images/Parrot.png"
            });

            // Assert

            // 1. Checking output result for type as CreatedAtRouteResult (201 Created At Route)
            var atRouteResult = Assert.IsAssignableFrom<CreatedAtRouteResult>(actionResult.Result);

        }

        [Fact]
        public void UpdateInformationCard_WithNewInformationCardModel_ReturnNoContent()
        {
            // Arrange
            var repository = GenerateMockRepository();
            repository.Setup(repo => repo.GetAllInformationCards()).Returns(GetTestCards());
            repository.Setup(repo => repo.GetInformationCardById(1)).Returns(GetTestCards().FirstOrDefault(e => e.Id == 1));

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);
            // Act

            ActionResult<InformationCardReadDto> actionResult = controller.UpdateInformationCard(1, new InformationCardUpdateDto
            {
                Name = "Parrot",
                Image = "../../../Data/Images/Parrot.png"
            });

            // Assert

            // 1. Checking output result for type as NoContentResult (204 No Content)
            var updatedResult = Assert.IsAssignableFrom<NoContentResult>(actionResult.Result);

        }

        [Fact]
        public void UpdateInformationCard_WithNewInformationCardModelWithNotExistingId_ReturnNotFound()
        {
            // Arrange
            var repository = GenerateMockRepository();

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);

            // Act

            ActionResult<InformationCardReadDto> actionResult = controller.UpdateInformationCard(10, new InformationCardUpdateDto
            {
                Name = "Parrot",
                Image = "../../../Data/Images/Parrot.png"
            });

            // Assert

            // 1. Checking output result for type as NotFoundResult (404 Not Found)
            var updatedResult = Assert.IsAssignableFrom<NotFoundResult>(actionResult.Result);

        }

        [Fact]
        public void DeleteInformationCard_WithAnExistingId_ReturnNoContent()
        {
            // Arrange
            var repository = GenerateMockRepository();
            repository.Setup(repo => repo.GetAllInformationCards()).Returns(GetTestCards());
            repository.Setup(repo => repo.GetInformationCardById(1)).Returns(GetTestCards().FirstOrDefault(e => e.Id == 1));

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);
            // Act

            ActionResult<InformationCardReadDto> actionResult = controller.DeleteInformationCard(1);

            // Assert

            // 1. Checking output result for type as NoContentResult (204 No Content)
            var deletedResult = Assert.IsAssignableFrom<NoContentResult>(actionResult.Result);
        }

        [Fact]
        public void DeleteInformationCard_WithNotExistingId_ReturnNotFound()
        {
            // Arrange
            var repository = GenerateMockRepository();
            repository.Setup(repo => repo.GetAllInformationCards()).Returns(GetTestCards());
            repository.Setup(repo => repo.GetInformationCardById(1)).Returns(GetTestCards().FirstOrDefault(e => e.Id == 1));

            var mapper = GenerateMockMapper();
            var controller = GenerateMockController(repository, mapper);
            // Act

            ActionResult<InformationCardReadDto> actionResult = controller.DeleteInformationCard(10);

            // Assert

            // 1. Checking output result for type as NotFoundResult (404 Not Found)
            var deletedResult = Assert.IsAssignableFrom<NotFoundResult>(actionResult.Result);
        }


        private List<InformationCard> GetTestCards()
        {
            var cards = new List<InformationCard>()
            {
                new InformationCard
                {
                    Id = 0,
                    Name = "Cat",
                    Image = "../../../Data/Images/Cat.png"
                },
                new InformationCard
                {
                    Id = 1,
                    Name = "Dog",
                    Image = "../../../Data/Images/Dog.jpg"
                },
                new InformationCard
                {
                    Id = 2,
                    Name = "Shark",
                    Image = "../../../Data/Images/Shark.png"
                },
                new InformationCard
                {
                    Id = 3,
                    Name = "Ant",
                    Image = "../../../Data/Images/Ant.jpg"
                },
                new InformationCard
                {
                    Id = 4,
                    Name = "Squirrel",
                    Image = "../../../Data/Images/Squirrel.png"
                }
            };

            return cards;
        }
        private Mock<IInformationCardRepo> GenerateMockRepository()
        {
            return new Mock<IInformationCardRepo>();
        }
        private IMapper GenerateMockMapper()
        {
            var mockMapper = new MapperConfiguration(config =>
            {
                config.AddProfile(new InformationCardProfile());
            });

            return mockMapper.CreateMapper();
        }
        private InformationCardsController GenerateMockController(Mock<IInformationCardRepo> mockRepository, IMapper mapper)
        {
            return new InformationCardsController(mockRepository.Object, mapper);
        }
    }
}