using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

class Solution
{
    public const string Delim = " ";

    static void Main(string[] args)
    {
        string[] inputs;
        var track = new Track();
        var player = new Pod("Player");
        var opponent = new Pod("Opponent");
        
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
            int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
            int nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
            int nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            int opponentX = int.Parse(inputs[0]);
            int opponentY = int.Parse(inputs[1]);

            var sensorInformation = new SensorInformation(x, y, opponentX, opponentY, nextCheckpointX, nextCheckpointY, nextCheckpointDist, nextCheckpointAngle);
            
            player.UpdateLocation(sensorInformation.Player);
            opponent.UpdateLocation(sensorInformation.Opponent);
            track.UpdateTrack(sensorInformation);
            var targetCheckPoint = track.GetCheckPoint(nextCheckpointX, nextCheckpointY);
            player.UpdateTarget(targetCheckPoint);
            player.MoveToTarget(nextCheckpointDist, nextCheckpointAngle, opponent);
        }
    }
}

internal class Pod
{
    public string Identifier { get; }
    public bool BoostAvailable { get; private set; }
    public Point Location { get; private set; }
    public Point LocationPrediction1 { get; private set; }
    public Point LocationPrediction2 { get; private set; }
    public Point LocationPrediction3 { get; private set; }
    public CheckPoint TargetCheckPoint { get; private set; }
    public int Speed { get; private set; }
    public string Direction { get; private set; }

    public Pod(string identifier) 
    {
        Identifier = identifier;
        BoostAvailable = true;
        TargetCheckPoint = null;
    }

    public void UpdateLocation(Point point)
    {
        Speed = MyMath.Distance(point, Location);
        Direction = MyMath.Direction(Location, point);
        LocationPrediction1 = MyMath.Flip(Location, point);
        LocationPrediction2 = MyMath.Flip(Location, point, 2);
        LocationPrediction3 = MyMath.Flip(Location, point, 3);
        Location = point;
        var goingTo = TargetCheckPoint != null ? string.Format("Going to {0}", TargetCheckPoint.Name) : "";
        Console.Error.WriteLine("Speed {0} {1}s {2} {3}", Identifier, Speed, Direction, goingTo);
    }

    public void MoveToTarget(int targetCheckpointDistance, int targetCheckpointAngle, Pod opponent)
    {
        Action action = null;
        if (BoostAvailable)
        {
            var distanceOpponentCheckPoint = MyMath.Distance(TargetCheckPoint.Location, opponent.Location);
            var distanceOpponentPlayer = MyMath.Distance(Location, opponent.Location);
            if (distanceOpponentCheckPoint < 1000 && distanceOpponentPlayer < 2000 && targetCheckpointDistance < 3000)
            {
                // Attaackkk
                BoostAvailable = false;
                action = new Action("Attack", opponent.Location, thrust:100, boost: true, message: "Bam!");
            }
            else if (targetCheckpointDistance > 5000 && Math.Abs(targetCheckpointAngle) < 3)
            {
                // Boost when correct angle & distance
                BoostAvailable = false;
                action = new Action("Boost", TargetCheckPoint.Location, thrust:100, boost: true, message: "Woohoo!");
            }
        }
        if (action == null)
        {
            if (Speed > 400)
            {
                var distancePredicted = MyMath.Distance(LocationPrediction3, TargetCheckPoint.Location);
                var thrust = distancePredicted <= targetCheckpointDistance ? 100 : 0;
                var info = string.Format("Actual {0} Predicted {1}", targetCheckpointDistance, distancePredicted);
                action = new Action("Predicted 3", TargetCheckPoint.Location, thrust:thrust, info: info);
            }
            else if (Speed > 300)
            {
                var distancePredicted = MyMath.Distance(LocationPrediction2, TargetCheckPoint.Location);
                var thrust = distancePredicted <= targetCheckpointDistance ? 100 : 25;
                var info = string.Format("Actual {0} Predicted {1}", targetCheckpointDistance, distancePredicted);
                action = new Action("Predicted 2", TargetCheckPoint.Location, thrust:thrust, info: info);
            }
            if (Speed > 200)
            {
                var distancePredicted = MyMath.Distance(LocationPrediction1, TargetCheckPoint.Location);
                var thrust = distancePredicted <= targetCheckpointDistance ? 100 : 50;
                var info = string.Format("Actual {0} Predicted {1}", targetCheckpointDistance, distancePredicted);
                action = new Action("Predicted 1", TargetCheckPoint.Location, thrust:thrust, info: info);
            }
        }
        if (action == null)
        {
            if (TargetCheckPoint.NextCheckPoint?.Distance < 2500 && targetCheckpointDistance < 1500)
            {
                var angle = Math.Abs(targetCheckpointAngle);
                var correction = Math.Max(angle + 30, 0);
                var trust = 100 - Math.Min(correction, 100);
                var info = string.Format("Angle {0}a {1}c {2}t", angle, correction, trust);
                action = new Action("Accuracy", TargetCheckPoint.Location, thrust:trust, info: info);
            }
            else if (Speed > 400 && targetCheckpointDistance < 1500 && TargetCheckPoint.NextCheckPoint != null)
            {
                action = new Action("Look ahead", TargetCheckPoint.NextCheckPoint.Location, thrust:0);
            }
            else
            {
                var angle = Math.Abs(targetCheckpointAngle);
                var correction = Math.Max(angle/2 - 10, 0);
                var trust = 100 - Math.Min(correction, 100);
                var info = string.Format("Angle {0}a {1}c {2}t", angle, correction, trust);
                action = new Action("Speed", TargetCheckPoint.Location, thrust:trust, info: info);
            }
        }

        Console.Error.WriteLine("Target {0}d {1}a", targetCheckpointDistance, targetCheckpointAngle);
        Console.Error.WriteLine("Method {0}", action.Method);
        Console.Error.WriteLine("Info {0}", action.Info);
        Console.WriteLine(action.CreateInstructions());
    }

    public void UpdateTarget(CheckPoint targetCheckPoint)
    {
        var updateCheckPoint = TargetCheckPoint != targetCheckPoint;
        if (updateCheckPoint)
        {
            if (TargetCheckPoint != null)
            {
                // Update CheckPoints' next
                TargetCheckPoint.LinkCheckPoints(targetCheckPoint);
            }
            
            // Update Pod target
            TargetCheckPoint = targetCheckPoint;
        }
        //Console.Error.WriteLine(updateCheckPoint ? "Switch to {0}" : "Continue to {0}", TargetCheckPoint.Name);
    }
}

internal class Action
{
    public string Method { get; }
    public Point Location { get; }
    public bool Boost { get; private set; }
    public int Thrust { get; private set; }
    public string Info { get; }
    public string Message { get; }

    public Action(string method, Point location, int thrust, bool boost = false, string info = "", string message = "")
    {
        if (Thrust < 0 || Thrust > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(thrust));
        }

        Method = method;
        Location = location;
        Boost = boost;
        Thrust = thrust;
        Info = info;
        Message = message;
    }

    public string CreateInstructions()
    {
        var thrust = Boost ? "BOOST" : Thrust.ToString("D");
        var message = !string.IsNullOrWhiteSpace(Message) ? string.Concat(Solution.Delim, Message) : String.Empty;
        return string.Concat(Location.X, Solution.Delim, Location.Y, Solution.Delim, thrust, message);
    }
}

internal class SensorInformation
{
    public Point Player { get; }
    public Point Opponent { get; }
    public Point NextCheckPoint { get; }
    public int NextCheckPointDist { get; }
    public int NextCheckPointAngle { get; }

    public SensorInformation(int playerX, int playerY, int opponentX, int opponentY, int nextCheckpointX, int nextCheckpointY, int nextCheckpointDist, int nextCheckpointAngle)
    {
        Player = new Point(playerX, playerY);
        Opponent = new Point(opponentX, opponentY);
        NextCheckPoint = new Point(nextCheckpointX, nextCheckpointY);
        NextCheckPointDist = nextCheckpointDist;
        NextCheckPointAngle = nextCheckpointAngle;
    }
}

internal class CheckPoint
{
    public const int Size = 600;

    public string Identifier { get; }
    public string Name { get; }
    public int Distance { get; private set; }
    public string Direction { get; private set; }
    public Point Location { get; }
    public CheckPoint NextCheckPoint;
    public CheckPoint PreviousCheckPoint;

    public CheckPoint(string identifier, string name, Point location)
    {
        Identifier = identifier;
        Name = name;
        Location = location;
        NextCheckPoint = null;
        PreviousCheckPoint = null;
    }

    public static string CreateIdentifier(int x, int y)
    {
        return string.Concat(x, ';', y);
    }

    internal void LinkCheckPoints(CheckPoint nextCheckPoint)
    {
        NextCheckPoint = nextCheckPoint;
        NextCheckPoint.PreviousCheckPoint = this;
        NextCheckPoint.Distance = MyMath.Distance(Location, nextCheckPoint.Location);
        NextCheckPoint.Direction = MyMath.Direction(Location, nextCheckPoint.Location);
    }
}

internal class Track
{
    IDictionary<string, CheckPoint> CheckPoints { get; }

    public Track()
    {
        CheckPoints = new Dictionary<string, CheckPoint>();
    }

    public void UpdateTrack(SensorInformation sensorInformation)
    {
        var key = CheckPoint.CreateIdentifier(sensorInformation.NextCheckPoint.X, sensorInformation.NextCheckPoint.Y);
        if (!CheckPoints.TryGetValue(key, out var nextCheckPoint))
        {
            // Add new CheckPoint
            var name = (CheckPoints.Count+1).ToString("D");
            nextCheckPoint = new CheckPoint(key, name, sensorInformation.NextCheckPoint);
            
            CheckPoints.Add(nextCheckPoint.Identifier, nextCheckPoint);
        }

        /* */
        foreach (var kv in CheckPoints)
        {
            var checkPoint = kv.Value;
            Console.Error.WriteLine("CheckPoint {0} Distance {1}d {2} Prev {3} Next {4}", 
                checkPoint.Name, 
                checkPoint.Distance,
                checkPoint.Direction,
                checkPoint.PreviousCheckPoint?.Name ?? "Unknown",
                checkPoint.NextCheckPoint?.Name ?? "Unknown");
        }   
        
    }

    public CheckPoint GetCheckPoint(int nextCheckpointX, int nextCheckpointY)
    {
        var key = CheckPoint.CreateIdentifier(nextCheckpointX, nextCheckpointY);
        return CheckPoints.TryGetValue(key, out var checkpoint)
            ? checkpoint
            : throw new InvalidOperationException("Next CheckPoint is unavailable");
    }
}

internal static class MyMath
{
    public static int Distance(Point a, Point b)
    {
        return (int) Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }

    public static string Direction(Point current, Point next)
    {
        return string.Concat(next.Y > current.Y ? "S" : "N", next.X < current.X ? "E" : "W");
    }

    public static Point Flip(Point current, Point next, int multiplier = 1)
    {
        var x = next.X + ((current.X - next.X) * -1 * multiplier);
        var y = next.Y + ((current.Y - next.Y) * -1 * multiplier);
        return new Point(x, y);
    }
}
