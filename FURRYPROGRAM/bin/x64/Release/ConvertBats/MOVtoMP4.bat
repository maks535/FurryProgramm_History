@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация MOV → MP4 (H.264 + AAC, высокое качество)
ffmpeg -i "file.mov" -c:v libx264 -preset slow -crf 18 -c:a aac -b:a 320k "file.mp4"

echo Конвертация завершена: file.mov → file.mp4
