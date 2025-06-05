#!/bin/bash
while read line; do
  if [[ "$line" == "quit" ]]; then
    exit 0
  fi
  if [[ $line == go* ]]; then
    echo "bestmove a1a2"
  fi
  # ignore other commands
done
