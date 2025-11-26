#!/bin/bash
set -e

echo "🔍 Starting SonarQube analysis..."

# Load token from .env if exists
if [ -f .env.sonar ]; then
  source .env.sonar
fi

if [ -z "$SONAR_TOKEN" ]; then
  echo "❌ SONAR_TOKEN not found. Set it in .env.sonar"
  exit 1
fi

dotnet sonarscanner begin \
  /k:"TaskManagerService" \
  /d:sonar.host.url="http://localhost:9000" \
  /d:sonar.token="$SONAR_TOKEN"

dotnet build TaskManagerService.sln

dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"

echo "✅ Analysis complete! View results at http://localhost:9000"