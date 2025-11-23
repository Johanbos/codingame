using System;
using NUnit.Framework;
using App;
using NUnit.Framework.Internal;

namespace Tests;

[TestFixture]
public class LineTests
{
    [Test]
    public void Constructor_ShouldInitializeStartAndEndPoints()
    {
        // Arrange
        var startX = 0;
        var endX = 100;
        var start = new Point(startX, 0);
        var end = new Point(endX, 100);
        var line = new Line(start, end);
        const int landerSize = Lander.LanderSize;

        // Act
        var actualStart = line.Start;
        var actualEnd = line.End;
        var actualStartEdge = line.StartEdge;
        var actualEndEdge = line.EndEdge;

        // Assert
        Assert.That(actualStartEdge, Is.EqualTo(start));
        Assert.That(actualEndEdge, Is.EqualTo(end));
        Assert.That(actualStart.X, Is.EqualTo(startX + landerSize));
        Assert.That(actualEnd.X, Is.EqualTo(endX - landerSize));
    }

    [TestCase(0, 200, 0, "start", TestName = "Left of Line")]
    [TestCase(0, 200, 200, "end", TestName = "Right of Line")]
    [TestCase(0, 200, 10, "start", TestName = "Above Start")]
    [TestCase(0, 200, 190, "end", TestName = "Above End")]
    [TestCase(0, 200, 100, "end", TestName = "Above Middle")]
    public void Constructor_ShouldCalculateNearestSpotCorrectly(int startX, int endX, int positionX, string point)
    {
        // Arrange
        var start = new Point(startX, 0);
        var end = new Point(endX, 0);
        var line = new Line(start, end);
        var position = new Point(positionX, 100);

        // Act
        var actualStart = line.Start;
        var actualEnd = line.End;
        var nearest = line.NearestSpot(position);

        // Assert
        switch (point)
        {
            case "start":
                Assert.That(nearest.X, Is.EqualTo(actualStart.X));
                break;
            case "end":
                Assert.That(nearest.X, Is.EqualTo(actualEnd.X));
                break;
            default:
                throw new ArgumentException($"Invalid point: {point}");
        }
    }
}