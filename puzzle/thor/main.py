import sys
import math

light_x, light_y, thor_x, thor_y = [int(i) for i in input().split()]

# game loop
while True:
    # The remaining amount of turns Thor can move. Do not remove this line.
    remaining_turns = int(input())

    x = -1 if light_x - thor_x < 0 else (1 if light_x - thor_x > 0 else 0)
    thor_x += x
    we = "W" if x == -1 else ("E" if x == 1 else "")

    y = -1 if light_y - thor_y < 0 else (1 if light_y - thor_y > 0 else 0)
    thor_y += y
    ns = "N" if y == -1 else ("S" if y == 1 else "")

    print(f"Debug messages... Thor X: {light_x}-{thor_x}={light_x - thor_x}", file=sys.stderr, flush=True)
    print(f"Debug messages... Thor Y: {light_y}-{thor_y}={light_y - thor_y}", file=sys.stderr, flush=True)

    print(ns + we)