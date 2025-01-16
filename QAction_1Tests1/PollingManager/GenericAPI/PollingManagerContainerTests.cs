using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Skyline.DataMiner.PollingManager.Tests.Configuration;
using Skyline.DataMiner.Scripting;
using Skyline.Protocol.PollingManager.CustomCode.Configuration;

namespace Skyline.DataMiner.PollingManager.Tests
{
	[TestClass]
	public class PollingManagerContainerTests
	{
		private Mock<SLProtocol> mockProtocol;
		private Mock<PollingManagerConfigurationBase> mockConfiguration;

		[TestInitialize]
		public void Setup()
		{
			mockProtocol = new Mock<SLProtocol>();
			mockProtocol.Setup(p => p.DataMinerID).Returns(1);
			mockProtocol.Setup(p => p.ElementID).Returns(100);
		}

		[TestMethod]
		public void AddManager_ShouldCreateNewManager_WhenNotExisting()
		{
			// Arrange
			var configuration = new PollingManagerConfiguration(mockProtocol.Object);

			// Act
			var manager = PollingManagerContainer.AddManager(this.mockProtocol.Object, configuration);

			// Assert
			Assert.IsNotNull(manager);

			// Cleanup
			PollingManagerContainer.RemoveInstance(mockProtocol.Object);
		}

		[TestMethod]
		public void AddManager_ShouldReturnExistingManager_WhenAlreadyExists()
		{
			// Arrange
			var configuration = new PollingManagerConfiguration(mockProtocol.Object);
			var existingManager = PollingManagerContainer.AddManager(mockProtocol.Object, configuration);

			// Act
			var manager = PollingManagerContainer.AddManager(mockProtocol.Object, configuration);

			// Assert
			Assert.AreSame(existingManager, manager);

			// Cleanup
			PollingManagerContainer.RemoveInstance(mockProtocol.Object);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddManager_ShouldThrowArgumentException_WhenCreationFails()
		{
			// Arrange
			var configuration = new PollingManagerTestConfiguration(mockProtocol.Object);

			// Act
			PollingManagerContainer.AddManager(mockProtocol.Object, configuration);
		}

		[TestMethod]
		public void RemoveInstance_ShouldReturnTrue_WhenManagerExists()
		{
			// Arrange
			var configuration = new PollingManagerConfiguration(mockProtocol.Object);
			PollingManagerContainer.AddManager(mockProtocol.Object, configuration);

			// Act
			var result = PollingManagerContainer.RemoveInstance(mockProtocol.Object);

			// Assert
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void RemoveInstance_ShouldReturnFalse_WhenManagerDoesNotExist()
		{
			// Act
			var result = PollingManagerContainer.RemoveInstance(mockProtocol.Object);

			// Assert
			Assert.IsFalse(result);
		}
	}
}
