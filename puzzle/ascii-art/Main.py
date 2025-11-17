import sys
from collections import defaultdict

ascii_art = defaultdict(list)
width = int(input())
height = int(input())
text = input().upper()

def write_text(text, text_writer):
    for h in range(height):
        for c in text:
            key = c if c in ascii_art else '?'
            text_writer.write(ascii_art[key][h])
        text_writer.write('\n')

def load_ascii_art():
    for i in range(height):
        row = input()
        for j in range(27):
            letter = '?' if j == 26 else chr(ord('A') + j)
            if len(ascii_art[letter]) < height:
                ascii_art[letter] = [''] * height
            ascii_art[letter][i] = row[j * width:(j + 1) * width]

load_ascii_art()
write_text(text, sys.stdout)