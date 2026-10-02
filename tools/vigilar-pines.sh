#!/bin/bash
# Records the motor pins and whether a robot program is running, every 0.2 s.
#   bash tools/vigilar-pines.sh <log file> [seconds, default 1200]
# Motor 1 = GPIO 19 (speed) / 20 (direction), motor 2 = GPIO 21 (speed) / 26 (direction).
# A speed pin stuck "hi" while no program is running (procs=0) means a motor left at full speed.
# Run it detached so it survives a lost SSH session:
#   setsid -f bash tools/vigilar-pines.sh /tmp/pines.log 1800 > /dev/null 2>&1 < /dev/null
LOG="$1"
END=$((SECONDS + ${2:-1200}))
: > "$LOG"
while [ $SECONDS -lt $END ]; do
  pins=$(pinctrl get 19,20,21,26 | awk '{printf "%s=%s ", $1, $6}')
  procs=$(pgrep -fc '[E]xplorerHat[.](BasicSample|ObstacleAvoidance|SonarDashboard)|[L]esson[0-9]+[.]')
  echo "$(date +%T.%N | cut -c1-10) $pins procs=$procs" >> "$LOG"
  sleep 0.2
done
