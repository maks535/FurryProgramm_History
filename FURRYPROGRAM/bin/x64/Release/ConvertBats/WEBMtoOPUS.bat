@echo off
set desktop=%USERPROFILE%\Desktop
cd /d "%desktop%"

ffmpeg -i "file.webm" -c:a libopus -b:a 512k -vbr on -compression_level 10 "file.opus"

echo Конвертация завершена: file.webm → file.opus
