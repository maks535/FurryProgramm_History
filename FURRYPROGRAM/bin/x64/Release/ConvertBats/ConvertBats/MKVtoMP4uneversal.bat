@echo off
cd /d "%USERPROFILE%\Desktop"
ffmpeg -i "file.mkv" -c:v libx264 -profile:v high -level 4.0 -pix_fmt yuv420p -preset medium -crf 23 -c:a aac -b:a 128k -movflags +faststart "file.mp4"
echo Конвертация завершена: file.mkv → file.MP4 (Универсальный)