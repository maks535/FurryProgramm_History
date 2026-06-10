@echo off
cd /d "D:\desktop"
ffmpeg -i file.flac -c:a libmp3lame -qscale:a 0 file.mp3