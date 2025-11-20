using System;
using NUnit.Framework;
using App;

namespace Tests
{
    [TestFixture]
    public class CockpitTests
    {
        
        [TestCase(0, 2, 0, 0)] // (0² - 0²) / (2 * -2) = 0 / -4 = 0
        [TestCase(2, 2, 0, 1)] // (0² - 2²) / (2 * -2) = -4 / -4 = 1
        [TestCase(4, 2, 0, 4)] // (0² - 4²) / (2 * -2) = -16 / -4 = 4
        [TestCase(4, 2, 2, 3)] // (2² - 4²) / (2 * -2) = -12 / -4 = 3
        public void DistanceToDecelerate_ValidInputs_ReturnsCorrectDistance(int initialSpeed, int deceleration, int finalSpeed, int expectedDistance)
        {
            // Arrange
            var lander = new Lander();
            var landingZone = new Line(new Point(0, 0), new Point(1000, 0));
            var cockpit = new Cockpit(lander, landingZone);

            // Act
            var result = cockpit.DistanceToDecelerate(initialSpeed, deceleration, finalSpeed);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDistance));
        }

        [TestCase(-10, 2, 0, TestName = "NegativeInitialSpeed_ThrowsArgumentOutOfRangeException")]
        [TestCase(10, -2, 0, TestName = "NegativeDeceleration_ThrowsArgumentOutOfRangeException")]
        [TestCase(10, 0, 0, TestName = "ZeroDeceleration_ThrowsArgumentOutOfRangeException")]
        [TestCase(10, 2, 20, TestName = "FinalSpeedGreaterThanInitialSpeed_ThrowsArgumentOutOfRangeException")]
        public void DistanceToDecelerate_InvalidInputs_ThrowsArgumentOutOfRangeException(int initialSpeed, int deceleration, int finalSpeed)
        {
            // Arrange
            var lander = new Lander();
            var landingZone = new Line(new Point(0, 0), new Point(1000, 0));
            var cockpit = new Cockpit(lander, landingZone);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => cockpit.DistanceToDecelerate(initialSpeed, deceleration, finalSpeed));
        }
    }
}