using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Deliver more ore to hq (left side of the map) than your opponent. Use radars to find ore but beware of traps!
 **/
class Game
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int mapWidth = int.Parse(inputs[0]);
        int mapHeight = int.Parse(inputs[1]); // size of the map
        var map = new Map(mapWidth, mapHeight);
        var hq = new HeadQuarters();

        // game loop
        while (true)
        {
            hq.ParseScoreInputs();
            map.ParseInputs();
            List<int> myRobotOrder;
            hq.ParseEntityInputs(map, out myRobotOrder);
            hq.DoCommands(map, hq);
            Strategy1(map, hq);
            hq.DoCommands(map, hq);
            hq.WriteActions(myRobotOrder);
            map.Debug();
        }
    }

    private static void Strategy1(Map map, HeadQuarters hq)
    {
        // Avoid holes not created by me

        var radarAvailable = hq.IsRadarAvailable();
        var radarNeeded = map.RadarNeeded();
        var radarLocation = map.RadarLocation(hq);
        Console.Error.WriteLine("Radar available: {0}, Needed {1} {2},{3}", radarAvailable, radarNeeded, radarLocation?.X, radarLocation?.Y);
        if (radarAvailable && radarNeeded && radarLocation != null)
        {  
            hq.NewRadarRobot(radarLocation);
        }

        var trapAvailable = hq.IsTrapAvailable();
        var trapNeeded = hq.IsTrapNeeded();
        var trapLocation = map.OreLocation(2);
        Console.Error.WriteLine("Trap available: {0}, Needed {1} {2},{3} {4}", trapAvailable, trapNeeded, trapLocation?.X, trapLocation?.Y, trapLocation?.Ore());    
        if (trapAvailable && trapNeeded && trapLocation != null)
        {
            hq.NewOreRobotFor(trapLocation, true);
        }    

        var idleRobots = hq.GetIdleRobots();
        foreach (var robot in idleRobots)
        {
            var oreLocation = map.OreLocation();
            if (oreLocation != null)
            {
                hq.NewOreRobotFor(oreLocation, false);
            }
        }
    }
}

internal enum States
{
    Idle,
    GettingRadar,
    PlantingRadar,
    GettingTrap,
    GettingOre,
    BringOreToHq,
}

internal enum Items
{
    None = -1,
    Radar = 2,
    Trap = 3,
    Ore = 4
}

internal class Entity
{
    public int EntityId { get; }
    public int EntityType { get; } // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public Items Item { get; private set; } = Items.None;
    public States State { get; private set; } = States.Idle;
    public string Action { get; private set; } = "WAIT";
    public MapCell Destination { get; private set; } = null;

    public static States[] RadarStates = new [] { States.GettingRadar, States.PlantingRadar };
    public static States[] OreStates = new [] { States.GettingOre, States.BringOreToHq };

    public Entity (int entityId, int entityType)
    {
        this.EntityId = entityId;
        this.EntityType = entityType;
    }

    public void Command(States state)
    {
        if (this.State == state)
        {
            throw new InvalidOperationException(string.Format("Robot {0} already is in state {1}", EntityId, State));
        }
        
        this.State = state;
    }

    public bool Alive()
    {
        return this.X + this.Y >= 0;
    }

    internal void ParseInputs(string[] inputs, Map map)
    {
        this.X = int.Parse(inputs[2]);
        this.Y = int.Parse(inputs[3]); // position of the entity
        this.Item = (Items)int.Parse(inputs[4]); // if this entity is a robot, the item it is carrying (-1 for NONE, 2 for RADAR, 3 for TRAP, 4 for ORE)
        if (this.EntityType >= 2)
        {
            var cell = map.GetCell(this.X, this.Y);
            this.SetDestination(cell);
        }
        if (!Alive())
        {
            this.State = States.Idle;
            this.Item = Items.None;
            this.SetDestination(null);
        }
    }

    internal void DoCommands(Map map, HeadQuarters hq)
    {
        if (this.Alive())
        {
            switch (State)
            {
                case States.Idle: 
                    this.Idle(map, hq); 
                    break;
                case States.GettingRadar: 
                case States.PlantingRadar: 
                    this.RadarDuty(hq); 
                    break;
                case States.GettingTrap:
                case States.GettingOre:
                case States.BringOreToHq:
                    this.OreDuty(hq);
                    break;
            }
            Console.Error.WriteLine("Robot {0}, Action: {1}, Item: {2}, State: {3}", this.EntityId, this.Action, this.Item, this.State, this.X, this.Y);
        }
        else
        {
            this.Dead();
            Console.Error.WriteLine("Robot {0} DEAD", this.EntityId);
        }
    }

    private void Dead()
    {
        this.Action = "WAIT";
    }

    private void OreDuty(HeadQuarters hq)
    {
        if (this.State == States.GettingTrap)
        {
            if (this.Item != Items.Radar)
            {
                if (hq.IsTrapAvailable())
                {
                    if (this.X == 0)
                    {
                        this.Action = "REQUEST TRAP";
                    }
                    else
                    {
                        this.Action = string.Format("MOVE 0 {0}", this.Y);
                    }
                }
                else
                {
                    this.Action = "WAIT";
                    this.Command(States.Idle);
                }
            }
            else
            {
                this.Command(States.GettingOre);
            }
        }

        if (this.State == States.GettingOre)
        {
            if (this.Item != Items.Ore)
            {
                if (this.Destination.Ore() > 0)
                {
                    this.Action = string.Format("DIG {0} {1}", this.Destination.X, this.Destination.Y);
                }
                else
                {
                    this.Action = "WAIT";
                    this.Command(States.Idle);
                }
            }
            else
            {
                this.Command(States.BringOreToHq);
            }
        }

        if (this.State == States.BringOreToHq)
        {
            if (this.Item == Items.Ore)
            {
                // Move to HQ
                this.Action = string.Format("MOVE 0 {0}", this.Y);
            }
            else
            {
                this.Command(States.Idle);
            }
        }
    }

    private void Idle(Map map, HeadQuarters hq)
    {
        if (this.Item == Items.Ore)
        {
            this.Command(States.BringOreToHq);
            this.OreDuty(hq);
        }
        else
        {
            if (this.X < 2) 
            {
                this.Action = string.Format("MOVE {0} {1}", this.X + 4, this.Y);
            }
            else
            {
                var move = true;
                Scout(this.X+1, this.Y);
                Scout(this.X, this.Y-1);
                Scout(this.X, this.Y+1);
                void Scout(int x, int y)
                {
                    if (x > 0 && x < map.width && y > 0 && y < map.height)
                    {
                        var cell = map.GetCell(x, y);
                        if (!cell.Hole)
                        {
                            this.Action = string.Format("DIG {0} {1}", cell.X, cell.Y);
                            move = false;
                        }
                    }
                }

                if (move)
                {
                    this.Action = string.Format("MOVE {0} {1}", this.X + 1, this.Y);
                }
            }
        }
    }

    private void RadarDuty(HeadQuarters hq)
    {
        if (this.State == States.GettingRadar)
        {
            if (this.Item != Items.Radar)
            {
                if (hq.IsRadarAvailable())
                {
                    if (this.X == 0)
                    {
                        this.Action = "REQUEST RADAR";
                    }
                    else
                    {
                        this.Action = string.Format("MOVE 0 {0}", this.Y);
                    }
                }
                else
                {
                    this.Action = "WAIT";
                    this.Command(States.Idle);
                }
            }
            else
            {
                this.Command(States.PlantingRadar);
            }
        }

        if (this.State == States.PlantingRadar)
        {
            if (this.Item == Items.Radar)
            {
                this.Action = string.Format("DIG {0} {1}", this.Destination.X, this.Destination.Y);
            }
            else
            {
                this.Action = "WAIT";
                this.Command(States.Idle);
            }
        }
    }

    internal void SetDestination(MapCell location, bool assignToMap = true)
    {
        //if (this.Destination != location)
        {
            Console.Error.WriteLine($"Entity: {EntityId}, New: {location?.X}.{location?.Y}, Old: {Destination?.X}.{Destination?.Y}, Assigned.EntityType: {Destination?.AssignedEntity?.EntityType}");

            // Remove entity from old location
            if (this.Destination != null && assignToMap)
            {
                this.Destination.Assign(null);
            }

            // Assign entity to new location
            this.Destination = location;
            if (this.Destination != null && assignToMap)
            {
                this.Destination.Assign(this);
            }
        }
    }

    internal string Debug()
    {
        switch (this.EntityType)
        {
            // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap
            case 0: return "+";
            case 1: return "!";
            case 2: return "R";
            case 3: return "T";
            default:
              throw new NotSupportedException();
        }
    }

    public bool IsMyRobot()
    {
        return this.EntityType == 0;
    }
}

internal class HeadQuarters
{
    public int MyScore { get; private set; } = 0;
    public int OpponentScore { get; private set; } = 0;
    public int RadarCooldown { get; private set; } = 10;
    public int TrapCooldown { get; private set; } = 10;
    private List<Entity> MyRobots = new List<Entity>();
    private List<Entity> OpponentRobots = new List<Entity>();
    private List<Entity> Radars = new List<Entity>();
    private List<Entity> Traps = new List<Entity>();

    public void ParseScoreInputs()
    {
        var inputs = Console.ReadLine().Split(' ');
        MyScore = int.Parse(inputs[0]); // Amount of ore delivered
        OpponentScore = int.Parse(inputs[1]);
    }

    public void ParseEntityInputs(Map map, out List<int> MyRobotOrder)
    {
        var inputs = Console.ReadLine().Split(' ');
        var entityCount = int.Parse(inputs[0]); // number of entities visible to you
        MyRobotOrder = new List<int>();
        this.RadarCooldown = int.Parse(inputs[1]); // turns left until a new radar can be requested
        this.TrapCooldown = int.Parse(inputs[2]); // turns left until a new trap can be requested
        for (int i = 0; i < entityCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int entityId = int.Parse(inputs[0]); // unique id of the entity
            int entityType = int.Parse(inputs[1]); // 0 for your robot, 1 for other robot, 2 for radar, 3 for trap

            List<Entity> list = null;
            switch (entityType)
            {
                case 0: list = MyRobots; MyRobotOrder.Add(entityId); break;
                case 1: list = OpponentRobots; break;
                case 2: list = Radars; break;
                case 3: list = Traps; break;
            }               
            var entity = list.FirstOrDefault(e => e.EntityId == entityId);
            if (entity == null)
            {
                entity = new Entity(entityId, entityType);
                list.Add(entity);
            }
            entity.ParseInputs(inputs, map);
        }
        foreach (var trap in Traps)
        {
            Console.Error.WriteLine("Trap {0} {1},{2}", trap.EntityId, trap.X, trap.Y);
        }
    }

    public bool IsRadarAvailable()
    {
        return this.RadarCooldown == 0;
    }

    public List<Entity> GetRadarRobots()
    {
        return MyRobots.Where(r => r.Alive() && Entity.RadarStates.Contains(r.State)).ToList();
    }

    public void NewRadarRobot(MapCell radarLocation)
    {
        var hq = new MapCell(0, radarLocation.Y);
        var sorted = SortNearest(MyRobots, hq);
        var robot = sorted.FirstOrDefault(r => r.Alive() && r.State == States.Idle);
        if (robot != null)
        {
            robot.Command(States.GettingRadar);
            robot.SetDestination(radarLocation);
        }
    }

    public List<Entity> GetOreRobots()
    {
        return MyRobots.Where(r => r.Alive() && Entity.OreStates.Contains(r.State)).ToList();
    }

    public void NewOreRobotFor(MapCell oreLocation, bool setTrap)
    {
        var sorted = SortNearest(MyRobots, oreLocation);
        var robot = sorted.FirstOrDefault(r => r.Alive() && r.State == States.Idle);
        if (robot != null)
        {
            robot.Command(setTrap ? States.GettingTrap : States.GettingOre);
            robot.SetDestination(oreLocation);
        }
    }

    private List<Entity> SortNearest(List<Entity> entities, MapCell location)
    {
        var list = entities.ToList();
        list.Sort((r, a) => Distance(r, location).CompareTo(Distance(a, location)));
        return list;
    }

    private int Distance(Entity entity, MapCell location)
    {
        return Math.Abs(entity.X - location.X) +
            Math.Abs(entity.Y - location.Y);
    }

    internal void DoCommands(Map map, HeadQuarters hq)
    {
        foreach (var robot in MyRobots)
        {
            robot.DoCommands(map, hq);
        }
    }

    internal IEnumerable<Entity> GetIdleRobots()
    {
        return MyRobots.Where(r => r.Alive() && r.State == States.Idle).ToList();
    }

    internal void WriteActions(List<int> myRobotOrder)
    {
        foreach (var robotId in myRobotOrder)
        {
            var robot = MyRobots.First(r => r.EntityId == robotId);
            Console.WriteLine(robot.Action);
        }
    }

    internal bool IsTrapAvailable()
    {
        return this.TrapCooldown == 0;
    }

    internal bool IsTrapNeeded()
    {
        return this.OpponentRobots.Count(r => r.Alive()) >= this.MyRobots.Count(r => r.Alive());
    }
}

internal class MapRow
{
    public MapCell[] cells;

    public MapRow(int y, int width)
    {
        this.cells = new MapCell[width];
        for (int x = 0; x < width; x++)
        {
            this.cells[x] = new MapCell(x, y);
        }
    }
}

internal class MapCell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public Entity AssignedEntity { get; private set; }
    public bool Hole { get; private set; }
    public bool HoleByMe { get; private set; }
    private string ore;

    public MapCell(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Assign(Entity entity)
    {
        // Leave traps, radars alone
        if (entity != null || AssignedEntity?.EntityType == 0)
        {
            AssignedEntity = entity;
            if (entity?.IsMyRobot() ?? false)
            {
                HoleByMe = true;
            }
        }
    }

    public void ParseInput(string ore, int hole)
    {
        this.ore = ore; // amount of ore or "?" if unknown
        this.Hole = hole == 1; // 1 if cell has a hole
    }

    public int? Ore()
    {
        int n;
        return Int32.TryParse(ore, out n) ? n : null;
    }

    public bool NeedsUpdate()
    {
        return Ore() == null;
    }

    public bool Safe()
    {
        if (this.AssignedEntity != null)
        {
            return false;
        }
        else
        {
            return this.Hole ? this.HoleByMe : true;
        }
    }

    public string Debug()
    {
        return string.Format("{0}{1}", this.AssignedEntity != null ? this.AssignedEntity.Debug() : "?", ore);
    }
}

internal class Map
{
    public int height { get; private set; }
    public int width { get; private set; }
    private MapRow[] rows;

    public Map(int width, int height)
    {
        this.height = height;
        this.width = width;
        this.rows = new MapRow[height];
        for (int y = 0; y < height; y++)
        {
            this.rows[y] = new MapRow(y, width);
        }
    }

    public void ParseInputs()
    {
        for (int y = 0; y < height; y++)
        {
            var inputs = Console.ReadLine().Split(' ');
            for (int x = 0; x < width; x++)
            {
                string ore = inputs[2*x];
                int hole = int.Parse(inputs[2*x+1]);
                rows[y].cells[x].ParseInput(ore, hole);
            }
        }
    }

    public void Debug()
    {
        for (int h = -1; h < height; h++)
        {
            if (h < 0)
            {
                string row = "  ";
                for (int w = 0; w < width; w++)
                {
                    row += w.ToString("00") + "|";
                }
                Console.Error.WriteLine(row);
            }
            else
            {
                string row = h.ToString("00");
                for (int w = 0; w < width; w++)
                {
                    row += rows[h].cells[w].Debug() + "|";
                }
                Console.Error.WriteLine(row);
            }
        }
    }

    internal bool RadarNeeded()
    {
        const int oreOffset = 10;
        var ore = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = rows[y].cells[x];
                ore = ore + cell.Ore() ?? 0;
                if (ore > oreOffset)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public MapCell RadarLocation(HeadQuarters hq)
    {
        MapCell RadarLocationOffset(int radarOffset)
        {
            for (int x = 2+radarOffset; x < width; x += radarOffset*2)
            {
                for (int y = 1+radarOffset; y < height; y += radarOffset*2)
                {
                    var cell = rows[y].cells[x];
                    if (cell.NeedsUpdate() && cell.Safe())
                    {
                        return cell;
                    }
                }
            }
            return null;
        }

        // Coarse radar
        var cell = RadarLocationOffset(2);
        if (cell != null) return cell;

        // Fine radar
        cell = RadarLocationOffset(1);
        if (cell != null) return cell;
        
        return null;        
    }

    public MapCell OreLocation(int minimumOre = 1)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = rows[y].cells[x];
                if (cell.Ore() >= minimumOre && cell.Safe())
                {
                    return cell;
                }
            }
        }

        return null;
    }

    internal void Assign(Entity entity)
    {
        //Console.Error.WriteLine("Map.Assign {0} at {1},{2}", entity.Debug(), entity.X, entity.Y);
        rows[entity.Y].cells[entity.X].Assign(entity);
    }

    internal MapCell GetCell(int x, int y)
    {
        return rows[y].cells[x];
    }
}
