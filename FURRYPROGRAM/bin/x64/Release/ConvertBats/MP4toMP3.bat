@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Извлечение аудио из MP4 → MP3 (44.1 kHz, стерео, 320 kbps)
ffmpeg -i "file.mp4" -vn -ar 44100 -ac 2 -b:a 320k "file.mp3"

echo Конвертация завершена: file.mp4 → file.mp3 (320 kbps)
