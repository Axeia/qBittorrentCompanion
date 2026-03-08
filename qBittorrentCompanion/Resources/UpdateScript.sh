#!/bin/bash

# Parameters
PARENT_PID=$1
INSTALL_PATH=$2
ZIP_PATH=$3

# 1. Wait for the app to exit
TIMEOUT=10
while kill -0 "$PARENT_PID" 2>/dev/null && [ $TIMEOUT -gt 0 ]; do
    sleep 1
    ((TIMEOUT--))
done

# 2. Force kill if still hanging
if kill -0 "$PARENT_PID" 2>/dev/null; then
    kill -9 "$PARENT_PID" 2>/dev/null
fi

# 3. Extract and Overwrite using tar
# -x: extract, -z: ungzip, -f: file, -C: destination directory
if tar -xzf "$ZIP_PATH" -C "$INSTALL_PATH"; then
    # 4. Restart
    EXE_PATH="$INSTALL_PATH/qBittorrentCompanion.Desktop"
    chmod +x "$EXE_PATH"
    
    # Launch using nohup to ensure it survives the script ending
    nohup "$EXE_PATH" > /dev/null 2>&1 &
else
    echo "Update failed" > "/tmp/qbc_update_error.txt"
fi

# 5. Cleanup script
rm "$0"