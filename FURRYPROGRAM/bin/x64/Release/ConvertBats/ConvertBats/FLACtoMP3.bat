@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация FLAC → MP3 (VBR, максимальное качество)
ffmpeg -i "file.flac" -c:a libmp3lame -qscale:a 0 "file.mp3"

echo Конвертация завершена: file.flac → file.mp3