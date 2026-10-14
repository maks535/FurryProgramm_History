@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

ffmpeg -i "file.webm" -vn -c copy "file.opus"

echo Конвертация завершена: file.webm → file.opus (без перекодирования)