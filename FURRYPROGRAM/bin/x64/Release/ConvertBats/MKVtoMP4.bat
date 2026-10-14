@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация MKV → MP4 без перекодирования
ffmpeg -i "file.mkv" -c copy "file.mp4"

echo Конвертация завершена: file.mkv → file.mp4
