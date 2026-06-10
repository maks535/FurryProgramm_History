@echo off
cd /d "D:\desktop"
ffmpeg -i file.mp4 -c:v libx265 -preset medium -crf 23 -c:a aac -b:a 320k file_hevc.mp4
