@echo off
cd /d "%USERPROFILE%\Desktop"

:: Конвертация MP4 → MP4 (H.265/HEVC + AAC, 320 kbps аудио)
ffmpeg -i "file.mp4" -c:v libx265 -preset slow -crf 23 -c:a aac -b:a 320k "file_hevc.mp4"

echo Конвертация завершена: file.mp4 → file_hevc.mp4
