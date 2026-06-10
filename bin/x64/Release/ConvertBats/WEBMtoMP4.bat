@echo off
cd /d D:\desktop
ffmpeg -i "file.webm" -c:v libx264 -c:a aac "file.mp4"