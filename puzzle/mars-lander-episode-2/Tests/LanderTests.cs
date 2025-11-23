using System;
using App;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LanderTests
    {
        [TestCase(0, 0, ExpectedResult = 1, TestName = "Direction_ZeroHorizontalSpeed_ZeroRotate_Returns1")]
        [TestCase(0, -15, ExpectedResult = 1, TestName = "Direction_ZeroHorizontalSpeed_NegativeRotate_Returns1")]
        [TestCase(0, 15, ExpectedResult = -1, TestName = "Direction_ZeroHorizontalSpeed_PositiveRotate_ReturnsMinus1")]
        [TestCase(-10, 0, ExpectedResult = -1, TestName = "Direction_NegativeHorizontalSpeed_ZeroRotate_ReturnsMinus1")]
        [TestCase(10, 0, ExpectedResult = 1, TestName = "Direction_PositiveHorizontalSpeed_ZeroRotate_Returns1")]
        public int DirectionTest(int horizontalSpeed, int rotate)
        {
            var lander = new Lander();
            lander.Update(0, 0, horizontalSpeed, 0, 0, rotate, 0);
            return lander.Heading();
        }

        [TestCase(1000, 500, 2000, true, TestName = "Lander moving right, landing zone is to the right")]
        [TestCase(1000, -500, 500, true, TestName = "Lander moving left, landing zone is to the left")]
        [TestCase(1000, 500, 500, false, TestName = "Lander moving right, landing zone is to the left")]
        [TestCase(1000, -500, 2000, false, TestName = "Lander moving left, landing zone is to the right")]
        [TestCase(1000, 0, 2000, true, TestName = "Lander stationary, landing zone is to the right")]
        [TestCase(1000, 0, 500, false, TestName = "Lander stationary, landing zone is to the left")]
        public void GoingToLandingZone_ShouldReturnExpectedResult(int landerX, int horizontalSpeed, int landingZoneX, bool expected)
        {
            // Arrange
            var lander = new Lander();
            lander.Update(landerX, 50, horizontalSpeed, 0, 0, 0, 0);
            var line = new Line(new Point(landingZoneX - 500, 0), new Point(landingZoneX + 500, 0));

            // Act
            var result = lander.GoingToLandingZone(line);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(0, 0, false, TestName = "No rotation and no angle change needed")]
        [TestCase(15, 0, false, TestName = "Positive rotation and no angle change needed")]
        [TestCase(-15, 0, false, TestName = "Negative rotation and no angle change needed")]
        [TestCase(0, 15, false, TestName = "No rotation and positive angle change needed")]
        [TestCase(0, -15, false, TestName = "No rotation and negative angle change needed")]
        [TestCase(15, -15, true, TestName = "Positive rotation and negative angle change needed")]
        [TestCase(-15, 15, true, TestName = "Negative rotation and positive angle change needed")]
        public void NeedsToRotate_ShouldReturnExpectedResult(int rotation, int angle, bool expected)
        {
            // Arrange
            var lander = new Lander();
            lander.Update(0, 0, 0, 0, 0, rotation, 0);

            // Act
            var result = lander.NeedsToRotate(angle);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}