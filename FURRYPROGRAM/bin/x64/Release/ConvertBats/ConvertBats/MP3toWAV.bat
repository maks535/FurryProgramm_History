@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация MP3 → WAV (PCM, 44.1 kHz, стерео)
ffmpeg -i "file.mp3" -acodec pcm_s16le -ar 44100 -ac 2 "file.wav"

echo Конвертация завершена: file.mp3 → file.wav
