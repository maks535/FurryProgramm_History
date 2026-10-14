@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация MKV → MP4 (H.265/HEVC + AAC)
ffmpeg -i "file.mkv" -c:v libx265 -preset medium -crf 23 -c:a aac -b:a 320k "file_hevc.mp4"

echo Конвертация завершена: file.mkv → file_hevc.mp4
