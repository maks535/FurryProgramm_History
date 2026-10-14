@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация WebM → MP4 (H.264 + AAC)
ffmpeg -i "file.webm" -c:v libx264 -c:a aac "file.mp4"

echo Конвертация завершена: file.webm → file.mp4
