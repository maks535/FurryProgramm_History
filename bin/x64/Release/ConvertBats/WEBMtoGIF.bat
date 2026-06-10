@echo off
cd /d "D:\desktop"
ffmpeg -i file.webm -vf "fps=15,scale=480:-1:flags=lanczos" -c:v gif file.gif