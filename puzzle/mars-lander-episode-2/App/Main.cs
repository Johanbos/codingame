using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]
namespace App;

class Solution
{
    static void Main(string[] args)
    {
        var map = new Map();
        int surfaceN = int.Parse(Console.ReadLine()!); // the number of points used to draw the surface of Mars.
        for (int i = 0; i < surfaceN; i++)
        {
            map.AddPoint(Console.ReadLine()!.Split(' '));
        }
        map.LocateLanding();
        var lander = new Lander();
        var cockpit = new Cockpit(lander, map.LandingZone);

        // game loop
        while (true)
        {
            var inputs = Console.ReadLine()!.Split(' ');
            lander.Update(inputs);
            Console.WriteLine(cockpit.Control());
        }
    }
}

class Cockpit
{
    public Lander Lander { get; }
    public Line LandingZone { get; }

    public Cockpit(Lander lander, Line landingZone)
    {
        Lander = lander;
        LandingZone = landingZone;
    }

    public string Control()
    {

        var landingZoneDelta = DistanceToLandingZone();
        var goingToLandingZone = Lander.GoingToLandingZone(LandingZone);
        if (goingToLandingZone)
        {
            if (landingZoneDelta.Y < 100 || Lander.Fuel <= 20)
            {
                return Land(landingZoneDelta);
            }
            else
            {
                if (landingZoneDelta.X <= 0)
                {
                    return PrepareForLanding(landingZoneDelta);
                }
                else
                {
                    return FlyTowardsLanding(landingZoneDelta);
                }
            }
        }
        else
        {
            return Turn(landingZoneDelta);
        }
    }

    private string Turn(Point landingDelta)
    {
        // Turn around
        Console.Error.WriteLine("Mode: TurnAround");
        var nearestSpot = LandingZone.NearestSpot(Lander.Position);
        var directionToLanding = Lander.Position.X < nearestSpot.X ? -1 : 1;
        var angle = 60 * directionToLanding;
        var thrust = Lander.Rotation * Lander.Heading() >= 0 ? Lander.MaxThrustPower : 0;
        return WithinLimits(angle, thrust, landingDelta.Y);
    }

    private string Land(Point landingDelta)
    {
        Console.Error.WriteLine("Mode: Landing");

        var angle = 0;
        var thrust = Lander.MaxThrustPower;
        return WithinLimits(angle, thrust, landingDelta.Y);
    }

    private string PrepareForLanding(Point landingDelta)
    {
        Console.Error.WriteLine("Mode: PrepareForLanding");
        var nearestSpot = LandingZone.NearestSpot(Lander.Position);
        var thrust = 2;
        var angle = 0;

        if (Lander.VerticalSpeed > 0)
        {
            // Going up, reduce vertical speed first
            thrust = 0;
            Console.Error.WriteLine("Lander is ascending, reducing vertical speed");
        }
        else
        {
            // Keep speed within landing limits, calculate meters needed to reduce vertical speed to safe landing speed
            var distanceToSafeLandingSpeed = DistanceToDecelerate(Math.Abs(Lander.VerticalSpeed), Lander.MaxThrustPower - Map.Gravity, Lander.Rotation, Lander.LandingVerticalSpeed);
            Console.Error.WriteLine("Distance to safe landing: " + distanceToSafeLandingSpeed);
            if (landingDelta.Y <= distanceToSafeLandingSpeed)
            {
                var verticalSpeedAdjustment = Lander.LandingVerticalSpeed - Math.Abs(Lander.VerticalSpeed);
                thrust = Lander.MaxThrustPower;
                Console.Error.WriteLine("Vertical brake speed margin: " + verticalSpeedAdjustment + "m/s");
            }
            else
            {
                var verticalSpeedAdjustment = Lander.FlyingVerticalSpeed - Math.Abs(Lander.VerticalSpeed);
                thrust = -verticalSpeedAdjustment;
                Console.Error.WriteLine("Vertical flying speed margin: " + verticalSpeedAdjustment + "m/s");
            }

            // Reduce horizontal speed to within landing limits
            var horizontalSpeedAdjustment = Lander.MaxLandingHorizontalSpeed - Math.Abs(Lander.HorizontalSpeed);
            angle = horizontalSpeedAdjustment * 5;
            angle *= Lander.Heading() * -1;
            thrust += Math.Abs(horizontalSpeedAdjustment);
            Console.Error.WriteLine("Horizontal speed margin: " + horizontalSpeedAdjustment + "m/s");
        }

        return WithinLimits(angle, thrust, landingDelta.Y);
    }

    private string FlyTowardsLanding(Point landingDelta)
    {
        Console.Error.WriteLine($"Mode: FlyTowardsLanding");

        // Keep speed within flying limits. Increase angle if speed is low, decrease angle if speed is near flying speed. 
        var horizontalSpeedDifference = Lander.FlyingHorizontalSpeed - Math.Abs(Lander.HorizontalSpeed);
        var angle = horizontalSpeedDifference * 2;
        var thrust = Math.Abs(horizontalSpeedDifference / 2);
        angle *= Lander.Heading() * -1;
        Console.Error.WriteLine("Horizontal speed diff: " + horizontalSpeedDifference + "m/s");

        // Keep vertical speed within flying limits
        var verticalSpeedDifference = Lander.FlyingVerticalSpeed + Lander.VerticalSpeed;
        thrust += (verticalSpeedDifference - 10) < 0 ? Math.Abs(verticalSpeedDifference) : 0;
        Console.Error.WriteLine("Vertical speed diff: " + verticalSpeedDifference + "m/s");

        if (Lander.NeedsToRotate(angle))
        {
            thrust = 0;
        }

        return WithinLimits(angle, thrust, landingDelta.Y);
    }

    private Point PredictedLanding()
    {
        // How much meters is needed to reduce the speed to finalSpeed.
        var nearestSpot = LandingZone.NearestSpot(Lander.Position);
        var direction = Lander.Heading();
        var x = Lander.Position.X + Lander.HorizontalSpeed + (DistanceToDecelerate(Math.Abs(Lander.HorizontalSpeed), 2, Lander.Rotation, Lander.MaxLandingHorizontalSpeed) * direction);
        var y = Lander.Position.Y + Lander.VerticalSpeed + DistanceToDecelerate(Math.Abs(Lander.VerticalSpeed), Lander.MaxThrustPower - Map.Gravity, Lander.Rotation);
        Console.Error.WriteLine("Predicted landing position at: " + x + "," + y);
        Console.Error.WriteLine("Actual landingzone nearestSpot at: " + nearestSpot.X + "," + nearestSpot.Y);
        return new Point(x, y);
    }

    internal int DistanceToDecelerate(int initialSpeed, float deceleration, int rotation, int finalSpeed = 0)
    {
        if (initialSpeed < 0) throw new ArgumentOutOfRangeException(nameof(initialSpeed), "Initial speed must be be positive.");
        if (finalSpeed < 0) throw new ArgumentOutOfRangeException(nameof(finalSpeed), "Final speed must be be positive.");
        if (deceleration <= 0) throw new ArgumentOutOfRangeException(nameof(deceleration), "Deceleration must be positive.");
        rotation = Math.Abs(rotation);

        // Adjust deceleration based on rotation angle
        deceleration *= (float)Math.Cos(rotation * Math.PI / 180);

        // How much meters is needed to reduce the speed to finalSpeed.
        // Using the formula: v² = u² + 2as  =>  s = (v² - u²) / (2a)
        // where v = final speed (0 for stopping), u = initial speed, a = deceleration (negative value), s = distance
        var distance = ((finalSpeed * finalSpeed) - (initialSpeed * initialSpeed)) / (2 * -deceleration);
        return (int)Math.Round(Math.Max(0, distance), 0);
    }

    internal string WithinLimits(float angle, float thrust, int height)
    {
        var maxAngle = Math.Max(15, 60 - (int)(45 * Math.Min(1, height / 4000.0)));
        Console.Error.WriteLine($"Commanding angle: {angle}, thrust: {thrust}");
        var angleBounded = (int)Math.Round(Math.Max(-maxAngle, Math.Min(maxAngle, angle)), 0);
        var thrustBounded = (int)Math.Round(Math.Max(Lander.MinThrustPower, Math.Min(Lander.MaxThrustPower, thrust)), 0);
        return $"{angleBounded} {thrustBounded}";
    }

    internal Point DistanceToLandingZone()
    {
        if (Lander.Position.WithinX(LandingZone))
        {
            return new Point(0, Math.Abs(Lander.Position.Y - LandingZone.Start.Y));
        }
        else
        {
            var nearestSpot = LandingZone.NearestSpot(Lander.Position);
            var distanceX = Math.Abs(Lander.Position.X - nearestSpot.X);
            var distanceY = Math.Abs(Lander.Position.Y - nearestSpot.Y);
            Console.Error.WriteLine("Landing zone at: " + nearestSpot.X + "," + nearestSpot.Y);
            Console.Error.WriteLine("Distance to landing zone: " + distanceX + "m horizontal" + ", " + distanceY + "m vertical");
            return new Point(distanceX, distanceY);
        }
    }
}

internal class Lander
{
    public Point Position { get; private set; }
    // Horizontal speed is negative when going left and the unit is meters per second
    public int HorizontalSpeed { get; private set; }
    // Vertical speed is negative when going down and the unit is meters per second
    public int VerticalSpeed { get; private set; }
    public int Fuel { get; private set; }
    public int Rotation { get; private set; }
    public int Power { get; private set; }

    public const int LandingVerticalSpeed = 10;
    public const int MaxLandingHorizontalSpeed = 2;
    public const int SafeLandingAngle = 0;
    public const int FlyingVerticalSpeed = 30;
    // Horizontal speed is up to 40 m/s
    public const int FlyingHorizontalSpeed = 40;
    // Thrust power ranges from 0 m/s² to 4 m/s²
    public const int MaxThrustPower = 4;
    public const int MinThrustPower = 0;
    public const int MaxAngleChangePerTurn = 15;
    public const int LanderSize = 10;

    public Lander()
    {
        Position = new Point(0, 0);
        HorizontalSpeed = 0;
        VerticalSpeed = 0;
        Fuel = 0;
        Rotation = 0;
        Power = 0;
    }

    internal void Update(string[] inputs)
    {
        Update(int.Parse(inputs[0]), int.Parse(inputs[1]), int.Parse(inputs[2]), int.Parse(inputs[3]), int.Parse(inputs[4]), int.Parse(inputs[5]), int.Parse(inputs[6]));
    }

    internal void Update(int x, int y, int horizontalSpeed, int verticalSpeed, int fuel, int rotation, int power)
    {
        Position = new Point(x, y);
        HorizontalSpeed = horizontalSpeed;
        VerticalSpeed = verticalSpeed;
        Fuel = fuel;
        Rotation = rotation;
        Power = power;
    }

    internal int Heading()
    {
        return HorizontalSpeed switch
        {
            < 0 => -1,
            0 => Rotation switch
            {
                < 0 => 1,
                > 0 => -1,
                _ => 1
            },
            _ => 1
        };
    }

    internal bool GoingToLandingZone(Line landingZone)
    {
        // if above the landing zone, return true
        if (Position.WithinX(landingZone))
        {
            return true;
        }
        else
        {
            // if not above the landing zone, get nearest coordinate
            var nearestSpot = landingZone.NearestSpot(Position);

            // If moving to the left, is the landing zone also to the left?
            return Heading() switch
            {
                -1 => Position.X > nearestSpot.X,
                0 => true,
                _ => Position.X < nearestSpot.X
            };
        }

    }

    internal bool NeedsToRotate(int angle)
    {
        // Return true if rotation and angle have different signs
        return (angle < 0 && Rotation > 0) || (angle > 0 && Rotation < 0);
    }
}

internal class Map
{
    public const float Gravity = 3.711f;
    public List<Point> Surface { get; }
    public Line LandingZone { get; private set; }

    public Map()
    {
        Surface = [];
        LandingZone = new Line(new Point(0, 0), new Point(0, 0));
    }

    internal void AddPoint(string[] strings)
    {
        var x = int.Parse(strings[0]);
        var y = int.Parse(strings[1]);
        AddPoint(new Point(x, y));
    }

    internal void AddPoint(Point point)
    {
        Surface.Add(point);
    }

    internal void LocateLanding()
    {
        var landX = 0;
        var landY = 0;
        for (int i = 0; i < Surface.Count; i++)
        {
            var point = Surface[i];
            var x = point.X;
            var y = point.Y;
            if (y == landY)
            {
                if (x - landX >= 1000)
                {
                    LandingZone = new Line(new Point(landX, landY), new Point(x, y));
                    Console.Error.WriteLine("Found landing zone from " + landX + "," + landY + " to " + x + "," + y);
                }
            }
            else
            {
                landX = x;
                landY = y;
            }
        }
        if (Math.Abs(LandingZone.Start.X - LandingZone.End.X) < 1000)
        {
            throw new Exception("Landing zone to small or not found!");
        }
    }
}

internal class Line
{
    public Point StartEdge { get; }
    public Point Start { get; }
    public Point EndEdge { get; }
    public Point End { get; }

    public Line(Point start, Point end)
    {
        StartEdge = start;
        Start = new Point(start.X + Lander.LanderSize, start.Y);
        EndEdge = end;
        End = new Point(end.X - Lander.LanderSize, end.Y);
    }

    public Point NearestSpot(Point coordinate)
    {
        var distanceToStart = Math.Abs(coordinate.X - Start.X);
        var distanceToEnd = Math.Abs(coordinate.X - End.X);
        return distanceToStart < distanceToEnd ? Start : End;
    }
}

internal class Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    internal bool NearX(Point coordinate1oordinate, int threshold = 500)
    {
        return Math.Abs(X - coordinate1oordinate.X) <= threshold;
    }

    internal bool WithinX(Line landingZone)
    {
        return X >= landingZone.Start.X && X <= landingZone.End.X;
    }
}
