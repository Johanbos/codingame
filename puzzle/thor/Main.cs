// https://www.codingame.com/ide/puzzle/power-of-thor
using System;
class Player
{
    static void Main(string[] args)
    {
        string[] i = Console.ReadLine()!.Split(' ');
        int lightX = int.Parse(i[0]);
        int lightY = int.Parse(i[1]);
        int thorX = int.Parse(i[2]);
        int thorY = int.Parse(i[3]);

        while (true)
        {
            int remainingTurns = int.Parse(Console.ReadLine()!);
            int x = (lightX - thorX) switch
            {
                < 0 => -1,
                0 => 0,
                _ => 1
            };
            thorX += x;
            string we = x switch
            {
                -1 => "W",
                0 => "",
                _ => "E"
            };
            int y = (lightY - thorY) switch
            {
                < 0 => -1,
                0 => 0,
                _ => 1
            };
            thorY += y;
            string ns = y switch
            {
                -1 => "N",
                0 => "",
                _ => "S"
            };
            Console.Error.WriteLine($"Thor X: {lightX}-{thorX}={lightX - thorX}");
            Console.Error.WriteLine($"Thor Y: {lightY}-{thorY}={lightY - thorY}");
            Console.WriteLine(ns + we);
        }
    }
}