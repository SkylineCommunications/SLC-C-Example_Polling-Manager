namespace Skyline.DataMiner.PollingManager.Tests
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;
	using Skyline.DataMiner.PollingManager;
	using Skyline.DataMiner.Scripting;

	[TestClass]
	public class PollingManagerContainerTests
	{
		private Mock<SLProtocol> mockProtocol;
		private Mock<PollingManagerConfigurationBase> mockConfiguration;

		[TestInitialize]
		public void Setup()
		{
			this.mockProtocol = new Mock<SLProtocol>();
			this.mockConfiguration = new Mock<PollingManagerConfigurationBase>();

			this.mockProtocol.Setup(p => p.DataMinerID).Returns(1);
			this.mockProtocol.Setup(p => p.ElementID).Returns(100);
		}

		[TestMethod]
		public void AddManager_ShouldCreateNewManager_WhenNotExisting()
		{
			// Arrange
			this.mockConfiguration.Setup(c => c.Create()).Verifiable();

			// Act
			var manager = PollingManagerContainer.AddManager(this.mockProtocol.Object, this.mockConfiguration.Object);

			// Assert
			Assert.IsNotNull(manager);
			this.mockConfiguration.Verify(c => c.Create(), Times.Once, "Create should be called once during manager creation.");
		}

		[TestMethod]
		public void AddManager_ShouldReturnExistingManager_WhenAlreadyExists()
		{
			// Arrange
			PollingManagerContainer.AddManager(this.mockProtocol.Object, this.mockConfiguration.Object);

			// Act
			var manager = PollingManagerContainer.AddManager(this.mockProtocol.Object, this.mockConfiguration.Object);

			// Assert
			Assert.IsNotNull(manager);
			this.mockConfiguration.Verify(c => c.Create(), Times.Once, "Create should not be called again for an existing manager.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void AddManager_ShouldThrowArgumentException_WhenCreationFails()
		{
			// Arrange
			this.mockConfiguration.Setup(c => c.Create()).Throws(new ArgumentException());

			// Act
			PollingManagerContainer.AddManager(this.mockProtocol.Object, this.mockConfiguration.Object);
		}

		[TestMethod]
		public void GetManager_ShouldReturnExistingManager_WhenInitialized()
		{
			// Arrange
			PollingManagerContainer.AddManager(this.mockProtocol.Object, this.mockConfiguration.Object);

			// Act
			var manager = PollingManagerContainer.GetManager(this.mockProtocol.Object, 0);

			// Assert
			Assert.IsNotNull(manager);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void GetManager_ShouldThrowInvalidOperationException_WhenNotInitialized()
		{
			// Act
			PollingManagerContainer.GetManager(this.mockProtocol.Object, 0);
		}

		[TestMethod]
		public void RemoveInstance_ShouldReturnTrue_WhenManagerExists()
		{
			// Arrange
			PollingManagerContainer.AddManager(this.mockProtocol.Object, this.mockConfiguration.Object);

			// Act
			var result = PollingManagerContainer.RemoveInstance(this.mockProtocol.Object);

			// Assert
			Assert.IsTrue(result);
		}

		[TestMethod]
		public void RemoveInstance_ShouldReturnFalse_WhenManagerDoesNotExist()
		{
			// Act
			var result = PollingManagerContainer.RemoveInstance(this.mockProtocol.Object);

			// Assert
			Assert.IsFalse(result);
		}
	}
}
