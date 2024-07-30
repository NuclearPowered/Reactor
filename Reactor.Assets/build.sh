#!/bin/bash

UNITY_VERSION=2020.3.45f1

if [[ ! -v UNITY_PATH ]]; then
    if command -v unityhub &>/dev/null; then
        echo "Trying to get unity path from unityhub..."

        UNITYHUB_INSTALL_PATH=$(unityhub -- --headless install-path)
        UNITY_PATH="$UNITYHUB_INSTALL_PATH/$UNITY_VERSION/Editor/Unity"

        if [[ ! -f $UNITY_PATH ]]; then
            echo "Unity $UNITY_VERSION is not installed"
            exit 1
        fi
    else
        echo "UNITY_PATH is not set"
        exit 1
    fi
fi

echo "UNITY_PATH = $UNITY_PATH"

rm Library -rf # TODO figure out why having Library cached makes invalid bundles in batchmode
"$UNITY_PATH" -batchmode -nographics -quit -logFile "-" -projectPath "$PWD" -executeMethod Build.BuildAssetBundles
