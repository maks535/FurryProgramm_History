@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация OGG → MP3 (VBR, максимальное качество)
ffmpeg -i "file.ogg" -c:a libmp3lame -qscale:a 0 "file.mp3"

echo Конвертация завершена: file.ogg → file.mp3
