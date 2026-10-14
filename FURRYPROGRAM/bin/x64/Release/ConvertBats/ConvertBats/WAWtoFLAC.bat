@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

ffmpeg -i "file.wav" -c:a flac "file.flac"

echo Конвертация завершена: file.wav → file.flac
