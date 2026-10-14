@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

ffmpeg -i "file.webm" -vn -c:a libmp3lame -b:a 320k -ar 48000 -ac 2 "file.mp3"

echo Конвертация завершена: file.webm → file.mp3
