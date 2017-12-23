#!/bin/sh

# call ./Package.sh 0.4 to produce Nima-Unity-v0.4.zip

rm -fR ./v$1
rm -fR ./Nima-Unity-v$1.zip
mkdir v$1
mkdir v$1/Nima-Unity
cp -fR Nima-CSharp v$1/Nima-Unity
cp -fR Unity v$1/Nima-Unity
cp -fR Gizmos v$1
find v$1 -type f -name '*.meta' -delete
cd v$1 && zip -r ../Nima-Unity-v$1.zip . && cd ..
rm -fR v$1