#!/bin/bash
# Emergency stop: asks the robot programs to stop (like Ctrl+C), kills them if they don't,
# and forces every motor and light pin of the Explorer HAT low.
#   bash tools/parar-robot.sh                            (on the Raspberry Pi)
#   ssh harlequin 'bash -s' < tools/parar-robot.sh       (from the PC)
PROGRAMS='[E]xplorerHat[.](BasicSample|ObstacleAvoidance|SonarDashboard)|[L]esson[0-9]+[.]'
pkill -INT -f "$PROGRAMS"
sleep 1.5
pkill -KILL -f "$PROGRAMS"
# Motor 1 (19, 20), motor 2 (21, 26) and lights (4, 17, 27, 5)
pinctrl set 19,20,21,26,4,17,27,5 op dl
pinctrl get 19,20,21,26,4,17,27,5 | awk '{printf "%s%s ", $1, $6} END {print ""}'
