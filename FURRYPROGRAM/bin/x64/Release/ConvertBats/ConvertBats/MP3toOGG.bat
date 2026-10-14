@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация MP3 → OGG (Vorbis, максимальное качество)
ffmpeg -i "file.mp3" -c:a libvorbis -qscale:a 10 "file.ogg"

echo Конвертация завершена: file.mp3 → file.ogg
