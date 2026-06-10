@echo off
cd /d "D:\desktop"
ffmpeg -i file.mp4 -vn -c:a aac -b:a 256k file.m4a