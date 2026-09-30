#!/bin/bash
# Records the power state of the Raspberry Pi every 0.2 s, to find voltage drops.
#   bash tools/vigilar-tension.sh <log file> [seconds, default 600]
# Each line: time and the value of "vcgencmd get_throttled". Summarize it with tools/comparar-tension.sh.
# Run it detached so it survives a lost SSH session:
#   setsid nohup bash tools/vigilar-tension.sh /tmp/tension.log 1800 > /dev/null 2>&1 < /dev/null &
LOG="$1"
END=$((SECONDS + ${2:-600}))
: > "$LOG"
while [ $SECONDS -lt $END ]; do
  echo "$(date +%T.%N | cut -c1-12) $(vcgencmd get_throttled | cut -d= -f2)" >> "$LOG"
  sleep 0.2
done
