@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация MP4 → GIF (15 fps, масштаб 480px, фильтр Lanczos)
ffmpeg -i "file.mp4" -vf "fps=15,scale=480:-1:flags=lanczos" -c:v gif "file.gif"

echo Конвертация завершена: file.mp4 → file.gif
