@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация WAV → MP3 (VBR, максимальное качество)
ffmpeg -i "file.wav" -c:a libmp3lame -qscale:a 0 "file.mp3"

echo Конвертация завершена: file.wav → file.mp3
