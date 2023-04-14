#!/usr/bin/pwsh
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

mkdir "${SCRIPT_DIR}/../Compiler/SableCC"
java -jar "${SCRIPT_DIR}/../sablecc-3-beta.3.altgen.20041114/lib/sablecc.jar" -t csharp ${SCRIPT_DIR}/../Compiler/grammar.sablecc3 -d Compiler/SableCC

exec $SHELL # Remove # for verbose