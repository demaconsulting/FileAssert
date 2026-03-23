#!/usr/bin/env bash
# Build and test FileAssert

set -e  # Exit on error

echo "🔧 Building FileAssert..."
dotnet build --configuration Release

echo "🧪 Running unit tests..."
dotnet test --configuration Release

echo "✅ Running self-validation..."
dotnet run --project src/DemaConsulting.FileAssert --configuration Release --framework net10.0 --no-build -- --validate

echo "✨ Build, tests, and validation completed successfully!"
