using System;
using System.IO;
using System.Collections.Generic;

class Solution
{
    private static readonly Dictionary<char, string[]> _asciiArt = [];
    private static int _width;
    private static int _height;

    static void Main(string[] args)
    {
        _width = int.Parse(Console.ReadLine()!);
        _height = int.Parse(Console.ReadLine()!);
        var text = Console.ReadLine()!.ToUpper();
        LoadAsciiArt();
        WriteText(text, Console.Out);
    }

    private static void WriteText(string text, TextWriter textWriter)
    {
        for (var h = 0; h < _height; h++)
        {
            foreach (var c in text)
            {
                var key = _asciiArt.ContainsKey(c) ? c : '?';
                textWriter.Write(_asciiArt[key][h]);
            }
            textWriter.WriteLine();
        }
    }

    static void LoadAsciiArt()
    {
        for (var i = 0; i < _height; i++)
        {
            var row = Console.ReadLine()!;
            for (var j = 0; j < 27; j++)
            {
                var letter = j == 26 ? '?' : (char)('A' + j);
                _asciiArt.TryAdd(letter, new string[_height]);
                _asciiArt[letter][i] = row.Substring(j * _width, _width);
            }
        }
    }
}