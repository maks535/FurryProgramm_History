@echo off
cd /d "D:\desktop"
ffmpeg -i file.mkv -c:v libx264 -preset slow -crf 18 -c:a ac3 -b:a 384k file.avi