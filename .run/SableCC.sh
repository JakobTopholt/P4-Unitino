#!/usr/bin/pwsh
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

mkdir "${SCRIPT_DIR}/../Compiler/SableCC"
java -jar "${SCRIPT_DIR}/../sablecc/sablecc.jar" -t csharp ${SCRIPT_DIR}/../Compiler/grammar.sablecc3 -d Compiler/SableCC

exec $SHELL # Remove # for verbose