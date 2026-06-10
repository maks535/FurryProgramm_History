@echo off
cd /d "D:\desktop"
ffmpeg -i file.webm -vn -ar 44100 -ac 2 -b:a 192k file.mp3
