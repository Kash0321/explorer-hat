#!/bin/bash
# Summarizes the voltage drops of a tools/vigilar-tension.sh log in time windows.
#   bash tools/comparar-tension.sh <log file> <start> <end> <name> [<start> <end> <name> ...]
# Example:
#   bash tools/comparar-tension.sh /tmp/tension.log 20:16:30 20:17:18 "reposo" 20:17:18 20:17:50 "prueba N"
# For each window: samples, samples with low voltage right now (bit 0 of get_throttled) and number of drops.
# With the UPS HAT (B) columns, also the lowest battery voltage and the highest current taken from the batteries.
LOG="$1"
shift
while [ $# -ge 3 ]; do
  awk -v a="$1" -v b="$2" -v n="$3" '
    $1 >= a && $1 < b {
      t++
      if ($2 ~ /[13579bdf]$/) { u++; if (!prev) runs++; prev = 1 } else prev = 0
      if ($3 != "") {
        mv = $3 + 0; ma = -($4 + 0)
        if (minmv == "" || mv < minmv) minmv = mv
        if (maxma == "" || ma > maxma) maxma = ma
      }
    }
    END {
      printf "%-24s muestras=%4d  tension_baja=%4d (%3.0f%%)  caidas=%d", n, t, u, (t ? 100 * u / t : 0), runs
      if (minmv != "") printf "  bateria_min=%d mV  corriente_max=%d mA", minmv, maxma
      print ""
    }
  ' "$LOG"
  shift 3
done
