#!/bin/bash
# Records the power state of the Raspberry Pi every 0.2 s, to find voltage drops.
#   bash tools/vigilar-tension.sh <log file> [seconds, default 600]
# Each line: time and the value of "vcgencmd get_throttled". Summarize it with tools/comparar-tension.sh.
# With the Waveshare UPS HAT (B) (INA219 at I2C 0x42), each line also has the battery voltage (mV)
# and current (mA, negative = the batteries are discharging).
# Run it detached so it survives a lost SSH session:
#   setsid -f bash tools/vigilar-tension.sh /tmp/tension.log 1800 > /dev/null 2>&1 < /dev/null
PATH="$PATH:/usr/sbin"
LOG="$1"
END=$((SECONDS + ${2:-600}))
UPS=0
if i2cget -y 1 0x42 0x02 w > /dev/null 2>&1; then
  UPS=1
fi

# Reads a 16-bit INA219 register (the chip sends the high byte first)
read_ina219() {
  local raw
  raw=$(i2cget -y 1 0x42 "$1" w)
  echo $(( (raw & 0xff) << 8 | (raw >> 8) ))
}

: > "$LOG"
while [ $SECONDS -lt $END ]; do
  line="$(date +%T.%N | cut -c1-12) $(vcgencmd get_throttled | cut -d= -f2)"
  if [ $UPS -eq 1 ]; then
    # Bus voltage: bits 15-3, 4 mV each. Shunt voltage: signed, 10 uV each, on a 0.1 ohm resistor = 0.1 mA each
    bus=$(read_ina219 0x02)
    shunt=$(read_ina219 0x01)
    if [ $shunt -ge 32768 ]; then
      shunt=$((shunt - 65536))
    fi
    line="$line $(( (bus >> 3) * 4 ))mV $((shunt / 10))mA"
  fi
  echo "$line" >> "$LOG"
  sleep 0.2
done
