using System;
using NUnit.Framework;
using App;

namespace Tests
{
    [TestFixture]
    public class CockpitTests
    {

        [TestCase(0, 2, 2, 0, 0)] // (2² - 0²) / (2 * -2) = 4 / -4 = Abs(-1) = 0
        [TestCase(0, 2, 0, 0, 0)] // (0² - 0²) / (2 * -2) = 0 / -4 = 0
        [TestCase(2, 2, 0, 0, 1)] // (0² - 2²) / (2 * -2) = -4 / -4 = 1
        [TestCase(4, 2, 0, 0, 4)] // (0² - 4²) / (2 * -2) = -16 / -4 = 4
        [TestCase(8, 2, 0, 0, 16)] // (0² - 8²) / (2 * -2) = -64 / -4 = 16
        [TestCase(8, 2, 0, 45, 23)] // (0² - 8²) / (2 * -1.414) = -64 / -2.828 = 22.6 = 23
        [TestCase(2, 2, 2, 0, 0)] // (2² - 2²) / (2 * -2) = 0 / -4 = 0
        [TestCase(4, 2, 2, 0, 3)] // (2² - 4²) / (2 * -2) = -12 / -4 = 3
        [TestCase(8, 2, 2, 0, 15)] // (2² - 8²) / (2 * -2) = 4 - 64 / -4 = 15
        public void DistanceToDecelerate_ValidInputs_ReturnsCorrectDistance(int initialSpeed, int deceleration, int finalSpeed, int rotation, int expectedDistance)
        {
            // Arrange
            var lander = new Lander();
            var landingZone = new Line(new Point(0, 0), new Point(1000, 0));
            var cockpit = new Cockpit(lander, landingZone);

            // Act
            var result = cockpit.DistanceToDecelerate(initialSpeed, deceleration, rotation, finalSpeed);

            // Assert
            Assert.That(result, Is.EqualTo(expectedDistance));
        }

        [TestCase(-10, 2, 0, TestName = "NegativeInitialSpeed_ThrowsArgumentOutOfRangeException")]
        [TestCase(10, -2, 0, TestName = "NegativeDeceleration_ThrowsArgumentOutOfRangeException")]
        [TestCase(10, 0, 0, TestName = "ZeroDeceleration_ThrowsArgumentOutOfRangeException")]
        public void DistanceToDecelerate_InvalidInputs_ThrowsArgumentOutOfRangeException(int initialSpeed, int deceleration, int finalSpeed)
        {
            // Arrange
            var lander = new Lander();
            var landingZone = new Line(new Point(0, 0), new Point(1000, 0));
            var cockpit = new Cockpit(lander, landingZone);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => cockpit.DistanceToDecelerate(initialSpeed, deceleration, 0, finalSpeed));
        }

        [TestCase(1500, 0, 0, 0, "" , TestName = "LanderOnLandingZone")]
        [TestCase(1500, 500, 0, 500, "" , TestName = "LanderAboveLandingZone")]
        [TestCase(0, 1000, 1500, 1000, "left" , TestName = "LanderAtLeftOfLandingZone")]
        [TestCase(2000, 1000, 500, 1000, "right" , TestName = "LanderAtRightOfLandingZone")]
        public void DistanceToLandingZone_ShouldReturnCorrectDistances(
            int landerX, int landerY, int expectedDistanceX, int expectedDistanceY, string nearestSpot)
        {
            // Arrange
            var lander = new Lander();
            var landerSize = Lander.LanderSize;
            var landingZone = new Line(new Point(1000, 0), new Point(2000, 0));
            var cockpit = new Cockpit(lander, landingZone);
            lander.Update(landerX, landerY, 0, 0, 0, 0, 0);

            // Act
            var distance = cockpit.DistanceToLandingZone();

            // Assert
            Assert.That(landingZone.NearestSpot(lander.Position).Y, Is.EqualTo(0));
            switch (nearestSpot)
            {
                case "left":
                    Assert.That(landingZone.NearestSpot(lander.Position).X, Is.EqualTo(1000 + landerSize));
                    break;
                case "right":
                    Assert.That(landingZone.NearestSpot(lander.Position).X, Is.EqualTo(2000 - landerSize));
                    break;
            };
        }
    }
}