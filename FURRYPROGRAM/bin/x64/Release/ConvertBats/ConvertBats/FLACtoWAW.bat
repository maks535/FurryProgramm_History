@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

ffmpeg -i "file.flac" -c:a pcm_s16le "file.wav"

echo Конвертация завершена: file.flac → file.wav