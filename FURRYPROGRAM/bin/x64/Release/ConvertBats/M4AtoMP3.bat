@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация M4A → MP3 (VBR, максимальное качество)
ffmpeg -i "file.m4a" -c:a libmp3lame -qscale:a 0 "file.mp3"

echo Конвертация завершена: file.m4a → file.mp3
