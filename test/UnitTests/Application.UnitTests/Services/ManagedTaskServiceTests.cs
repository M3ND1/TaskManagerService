using AutoMapper;
using FluentAssertions;
using Moq;
using TaskManager.Application.DTOs;
using TaskManager.Application.Mappings;
using TaskManager.Application.Services;
using TaskManager.Core.Entities;
using TaskManager.Core.Enums;
using TaskManager.Core.Interfaces;

namespace Application.UnitTests.Services
{
    public class ManagedTaskServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IManagedTaskRepository> _managedTaskRepository;
        private readonly ManagedTaskService _managedTaskService;

        public ManagedTaskServiceTests()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;
            _managedTaskRepository = new Mock<IManagedTaskRepository>();
            _managedTaskService = new ManagedTaskService(_managedTaskRepository.Object, _mapper);
        }
        [Fact]
        public async Task CreateTaskAsync_Should_Return_Dto()
        {
            //Arrange
            _managedTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<ManagedTask>())).ReturnsAsync(true);
            var createManagedTaskDto = new CreateManagedTaskDto
            {
                Title = "create",
                Description = "desc",
                DueDate = DateTime.UtcNow.AddDays(1),
                Priority = PriorityLevel.Low,
                EstimatedHours = 3
            };
            //Act
            var result = await _managedTaskService.CreateTaskAsync(createManagedTaskDto, 1, 1);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ManagedTaskResponseDto>();
            result.Title.Should().Be("create");
            result.Description.Should().Be("desc");
            result.Priority.Should().Be(PriorityLevel.Low);
            result.EstimatedHours.Should().Be(3);
        }

        [Fact]
        public async Task CreateTaskAsync_Should_Return_Null_When_Repository_Fails()
        {
            //Arrange
            _managedTaskRepository.Setup(repo => repo.AddAsync(It.IsAny<ManagedTask>())).ReturnsAsync(false);
            var createManagedTaskDto = new CreateManagedTaskDto
            {
                Title = "create",
                Description = "desc",
                Priority = PriorityLevel.Low,
            };
            //Act
            var result = await _managedTaskService.CreateTaskAsync(createManagedTaskDto, 1);
            //Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task CreateTaskAsync_Should_Map_DTO_Properties_Correctly()
        {
            //Arrange
            var createManagedTaskDto = new CreateManagedTaskDto
            {
                Title = "Test Task",
                Description = "Test Description",
                Priority = PriorityLevel.High,
                EstimatedHours = 8
            };
            ManagedTask? capturedTask = null;
            _managedTaskRepository.Setup(x => x.AddAsync(It.IsAny<ManagedTask>()))
                .Callback<ManagedTask>(task => capturedTask = task)
                .ReturnsAsync(true);

            //Act
            await _managedTaskService.CreateTaskAsync(createManagedTaskDto, 5, 10);

            //Assert
            capturedTask.Should().NotBeNull();
            capturedTask?.Title.Should().Be("Test Task");
            capturedTask?.CreatedById.Should().Be(5);
            capturedTask?.AssignedToId.Should().Be(10);
        }

        [Fact]
        public async Task CreateTaskAsync_Should_Set_Only_CreatedById_When_AssignedToId_Not_Provided()
        {
            //Arrange
            var createManagedTaskDto = new CreateManagedTaskDto
            {
                Title = "Unassigned Task",
                Priority = PriorityLevel.Medium
            };
            ManagedTask? capturedTask = null;
            _managedTaskRepository.Setup(x => x.AddAsync(It.IsAny<ManagedTask>()))
                .Callback<ManagedTask>(task => capturedTask = task)
                .ReturnsAsync(true);

            //Act
            await _managedTaskService.CreateTaskAsync(createManagedTaskDto, 5);

            //Assert
            capturedTask.Should().NotBeNull();
            capturedTask.CreatedById.Should().Be(5);
            capturedTask.AssignedToId.Should().BeNull();
        }

        [Fact]
        public async Task GetTaskAsync_Should_Return_Mapped_DTO_When_Task_Exists()
        {
            //Arrange
            var managedTask = new ManagedTask
            {
                Id = 1,
                Title = "Retrieved Task",
                Description = "Task Description",
                Priority = PriorityLevel.High,
                IsCompleted = false,
                CreatedById = 5
            };
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(managedTask);

            //Act
            var result = await _managedTaskService.GetTaskAsync(1);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ManagedTaskResponseDto>();
            result.Id.Should().Be(1);
            result.Title.Should().Be("Retrieved Task");
            result.Description.Should().Be("Task Description");
            result.Priority.Should().Be(PriorityLevel.High);
            result.IsCompleted.Should().Be(false);
            result.CreatedById.Should().Be(5);
        }
        [Theory]
        [InlineData(-1)]
        [InlineData(999999)]
        public async Task GetTaskAsync_Should_Return_Null_When_No_ManagedTask(int taskId)
        {
            _managedTaskRepository.Setup(x => x.GetAsync(taskId)).ReturnsAsync((ManagedTask?)null);
            //Act
            var result = await _managedTaskService.GetTaskAsync(taskId);
            //Assert
            result.Should().BeNull();
        }
        [Fact]
        public async Task GetTaskAsync_Should_Return_CreatedBy_AssignedTo()
        {
            var managedTaskReturn = new ManagedTask
            {
                Id = 1,
                Title = "Title1",
                Description = "Description1",
                AssignedTo = new User { Id = 1, FirstName = "John" }
            };
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(managedTaskReturn);
            var result = await _managedTaskService.GetTaskAsync(1);

            result.Should().NotBe(null);
            result?.AssignedTo.Should().NotBe(null);
            result?.AssignedTo!.Id.Should().Be(1);
            result?.AssignedTo!.FirstName.Should().Be("John");
        }
        [Fact]
        public async Task UpdateTaskAsync_Should_Return_True_When_Task_Updated_And_Update_DateTime_Field()
        {
            // Arrange
            var existingTask = new ManagedTask
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Description",
                UpdatedAt = DateTime.UtcNow
            };
            ManagedTask? capturedTask = null;
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(existingTask);
            _managedTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ManagedTask>()))
                .Callback<ManagedTask>(task => capturedTask = task)
                .ReturnsAsync(true);

            var updateDto = new UpdateManagedTaskDto
            {
                Title = "New Title",
                Description = "New Description",
            };

            // Act
            var result = await _managedTaskService.UpdateTaskAsync(1, updateDto);

            // Assert
            result.Should().BeTrue();
            capturedTask.Should().NotBeNull();
            capturedTask.Title.Should().Be("New Title");
            capturedTask.Description.Should().Be("New Description");
            capturedTask.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            capturedTask.UpdatedAt.Should().BeOnOrAfter(existingTask.UpdatedAt.Value);
            _managedTaskRepository.Verify(x => x.UpdateAsync(It.Is<ManagedTask>(t => t.Title == "New Title" && t.Description == "New Description")), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_Should_Return_False_When_Task_Not_Found()
        {
            // Arrange
            _managedTaskRepository.Setup(x => x.GetAsync(999)).ReturnsAsync((ManagedTask?)null);
            var updateDto = new UpdateManagedTaskDto
            {
                Title = "Updated Title"
            };

            // Act
            var result = await _managedTaskService.UpdateTaskAsync(999, updateDto);

            // Assert
            result.Should().BeFalse();
            _managedTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ManagedTask>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_Should_Return_False_When_Repository_Update_Fails()
        {
            // Arrange
            var existingTask = new ManagedTask
            {
                Id = 1,
                Title = "Existing Title",
                Description = "Existing Description"
            };
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(existingTask);
            _managedTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ManagedTask>())).ReturnsAsync(false);

            var updateDto = new UpdateManagedTaskDto
            {
                Title = "Updated Title"
            };

            // Act
            var result = await _managedTaskService.UpdateTaskAsync(1, updateDto);

            // Assert
            result.Should().BeFalse();
            _managedTaskRepository.Verify(x => x.UpdateAsync(It.IsAny<ManagedTask>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_Should_Map_All_DTO_Properties_Correctly()
        {
            // Arrange
            var existingTask = new ManagedTask
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Description",
                Priority = PriorityLevel.Low,
                IsCompleted = false,
                EstimatedHours = 2
            };
            ManagedTask? capturedTask = null;
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(existingTask);
            _managedTaskRepository.Setup(x => x.UpdateAsync(It.IsAny<ManagedTask>()))
                .Callback<ManagedTask>(task => capturedTask = task)
                .ReturnsAsync(true);

            var updateDto = new UpdateManagedTaskDto
            {
                Title = "Updated Title",
                Description = "Updated Description",
                Priority = PriorityLevel.High,
                IsCompleted = true,
                EstimatedHours = 5
            };

            // Act
            var result = await _managedTaskService.UpdateTaskAsync(1, updateDto);

            // Assert
            result.Should().BeTrue();
            capturedTask.Should().NotBeNull();
            capturedTask.Title.Should().Be("Updated Title");
            capturedTask.Description.Should().Be("Updated Description");
            capturedTask.Priority.Should().Be(PriorityLevel.High);
            capturedTask.IsCompleted.Should().BeTrue();
            capturedTask.EstimatedHours.Should().Be(5);
        }

        [Fact]
        public async Task DeleteTaskAsync_Should_Return_True_When_Task_Deleted_Successfully()
        {
            // Arrange
            var existingTask = new ManagedTask
            {
                Id = 1,
                Title = "Task to Delete"
            };
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(existingTask);
            _managedTaskRepository.Setup(x => x.DeleteAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _managedTaskService.DeleteTaskAsync(1);

            // Assert
            result.Should().BeTrue();
            _managedTaskRepository.Verify(x => x.GetAsync(1), Times.Once);
            _managedTaskRepository.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_Should_Return_False_When_Repository_Delete_Fails()
        {
            // Arrange
            var existingTask = new ManagedTask
            {
                Id = 1,
                Title = "Task to Delete"
            };
            _managedTaskRepository.Setup(x => x.GetAsync(1)).ReturnsAsync(existingTask);
            _managedTaskRepository.Setup(x => x.DeleteAsync(1)).ReturnsAsync(false);

            // Act
            var result = await _managedTaskService.DeleteTaskAsync(1);

            // Assert
            result.Should().BeFalse();
            _managedTaskRepository.Verify(x => x.GetAsync(1), Times.Once);
            _managedTaskRepository.Verify(x => x.DeleteAsync(1), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        [InlineData(999)]
        public async Task DeleteTaskAsync_Should_Return_False_For_Invalid_Task_Ids(int invalidId)
        {
            // Arrange
            _managedTaskRepository.Setup(x => x.GetAsync(invalidId)).ReturnsAsync((ManagedTask?)null);

            // Act
            var result = await _managedTaskService.DeleteTaskAsync(invalidId);

            // Assert
            result.Should().BeFalse();
            _managedTaskRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
        }

    }
}