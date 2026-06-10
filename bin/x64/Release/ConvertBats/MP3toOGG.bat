@echo off
cd /d "D:\desktop"
ffmpeg -i file.mp3 -c:a libvorbis -qscale:a 10 file.ogg