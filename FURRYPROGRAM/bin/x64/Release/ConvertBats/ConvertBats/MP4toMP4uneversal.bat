@echo off
cd /d "%USERPROFILE%\Desktop"

:: Конвертация MP4 → MP4 (H.264 + AAC, оптимизация для веба)
ffmpeg -i "file.mp4" -c:v libx264 -profile:v high -level 4.0 -pix_fmt yuv420p -preset medium -crf 23 -c:a aac -b:a 128k -movflags +faststart "output.mp4"

echo Конвертация завершена: file.mp4 → output.mp4
