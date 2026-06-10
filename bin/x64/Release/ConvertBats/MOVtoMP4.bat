@echo off
cd /d "D:\desktop"
ffmpeg -i file.mov -c:v libx264 -preset slow -crf 18 -c:a aac -b:a 320k file.mp4