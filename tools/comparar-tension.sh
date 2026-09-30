#!/bin/bash
# Summarizes the voltage drops of a tools/vigilar-tension.sh log in time windows.
#   bash tools/comparar-tension.sh <log file> <start> <end> <name> [<start> <end> <name> ...]
# Example:
#   bash tools/comparar-tension.sh /tmp/tension.log 20:16:30 20:17:18 "reposo" 20:17:18 20:17:50 "prueba N"
# For each window: samples, samples with low voltage right now (bit 0 of get_throttled) and number of drops.
LOG="$1"
shift
while [ $# -ge 3 ]; do
  awk -v a="$1" -v b="$2" -v n="$3" '
    $1 >= a && $1 < b {
      t++
      if ($2 ~ /[13579bdf]$/) { u++; if (!prev) runs++; prev = 1 } else prev = 0
    }
    END { printf "%-24s muestras=%4d  tension_baja=%4d (%3.0f%%)  caidas=%d\n", n, t, u, (t ? 100 * u / t : 0), runs }
  ' "$LOG"
  shift 3
done
