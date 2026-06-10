@echo off
cd /d "D:\desktop"
ffmpeg -i file.mp4 -c:v prores_ks -profile:v 3 -c:a pcm_s16le file.mov