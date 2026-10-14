@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

:: Конвертация WebM → GIF (15 fps, ширина 480px, фильтр Lanczos)
ffmpeg -i "file.webm" -vf "fps=15,scale=480:-1:flags=lanczos" -c:v gif "file.gif"

echo Конвертация завершена: file.webm → file.gif
