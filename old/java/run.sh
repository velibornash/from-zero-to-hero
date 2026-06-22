#!/bin/bash
# Run "From Zero to Hero"
set -e

cd "$(dirname "$0")"

echo ">>> Kompajliram..."
mvn -q compile

echo ">>> Pokrecem igru..."
CP="target/classes:$(mvn -q dependency:build-classpath -Dmdep.outputFile=/dev/stdout 2>/dev/null)"

java -XstartOnFirstThread -cp "$CP" com.game.DesktopLauncher
