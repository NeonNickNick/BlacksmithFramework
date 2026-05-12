#!/usr/bin/env bash
set -euo pipefail

# Check .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo ".NET SDK is not installed. Please download and install .NET 8.0 SDK from https://dotnet.microsoft.com/download/dotnet/8.0"
    exit 1
fi

OUTPUT_DIR="BlacksmithServer"

# Ensure output and config directories exist
mkdir -p "$OUTPUT_DIR/.blacksmith"
mkdir -p "$OUTPUT_DIR/.blacksmith-server"
mkdir -p "$OUTPUT_DIR/ModExamples"

# Write mod.json only if it doesn't exist (preserve user modifications)
if [ ! -f "$OUTPUT_DIR/.blacksmith/mod.json" ]; then
    cat > "$OUTPUT_DIR/.blacksmith/mod.json" << 'EOF'
{
  "modexamples": "ModExamples"
}
EOF
fi

if [ ! -f "$OUTPUT_DIR/.blacksmith-server/users.json" ]; then
    cat > "$OUTPUT_DIR/.blacksmith-server/users.json" << 'EOF'
{

}
EOF
fi

# Publish Blacksmith server (overwrites built artifacts only)
dotnet publish "./Project/Blacksmith/BlacksmithServer/BlacksmithServer.csproj" \
    -c Release -o "$OUTPUT_DIR"

# Publish and update ModExamples DLL
TEMP_DIR=$(mktemp -d)
dotnet publish "./Project/Blacksmith/ModExamples/ModExamples.csproj" \
    -c Release -o "$TEMP_DIR"

mv "$TEMP_DIR/ModExamples.dll" "$OUTPUT_DIR/ModExamples/"
rm -rf "$TEMP_DIR"

echo "BlacksmithServer build complete. Output: $OUTPUT_DIR"
echo "Run with: dotnet $OUTPUT_DIR/BlacksmithServer.dll"
