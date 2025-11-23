using System;
using NUnit.Framework;
using App;

namespace Tests;

[TestFixture]
public class PointTests
{
    [TestCase(1000, 1200, 500, true, TestName = "WithinThreshold_ShouldReturnTrue")]
    [TestCase(1000, 1600, 500, false, TestName = "OutsideThreshold_ShouldReturnFalse")]
    [TestCase(1000, 1500, 500, true, TestName = "ExactlyAtThreshold_ShouldReturnTrue")]
    public void NearX_ShouldReturnExpectedResult(int x1, int x2, int threshold, bool expectedResult)
    {
        // Arrange
        var coordinate1 = new Point(x1, 0);
        var coordinate2 = new Point(x2, 0);

        // Act
        var result = coordinate1.NearX(coordinate2, threshold);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}