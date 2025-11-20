using System;
using NUnit.Framework;
using App;

namespace App;

[TestFixture]
public class LineTests
{
    [Test]
    public void Constructor_ShouldInitializeStartAndEndPoints()
    {
        var start = new Point(0, 0);
        var end = new Point(10, 10);
        var line = new Line(start, end);

        Assert.That(start, Is.EqualTo(line.Start))  ;
        Assert.That(end, Is.EqualTo(line.End));
    }

    [TestCase(0, 0, 10, 10, 5, 5)]
    [TestCase(0, 0, 20, 0, 10, 0)]
    [TestCase(5, 5, 15, 5, 10, 5)]
    public void Constructor_ShouldCalculateMiddlePointCorrectly(int x1, int y1, int x2, int y2, int expectedX, int expectedY)
    {
        var start = new Point(x1, y1);
        var end = new Point(x2, y2);
        var line = new Line(start, end);

        Assert.That(expectedX, Is.EqualTo(line.Middle.X));
        Assert.That(expectedY, Is.EqualTo(line.Middle.Y));
    }
}