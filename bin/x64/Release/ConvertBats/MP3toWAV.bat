@echo off
cd /d "D:\desktop"
ffmpeg -i file.mp3 -acodec pcm_s16le -ar 44100 -ac 2 file.wav