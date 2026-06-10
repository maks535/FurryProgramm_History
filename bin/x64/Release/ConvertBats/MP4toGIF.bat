@echo off
cd /d "D:\desktop"
ffmpeg -i file.mp4 -vf "fps=15,scale=480:-1:flags=lanczos" -c:v gif file.gif