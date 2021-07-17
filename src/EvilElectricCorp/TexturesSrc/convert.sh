#!/bin/bash


texconv="/d/Program Files/Steam/steamapps/common/SpaceEngineersModSDK/Tools/TexturePacking/Tools/texconv.exe"

rm -r converted || true
mkdir converted
pushd converted
"$texconv" -f BC7_UNORM_SRGB -sepalpha "..\\Recipes\\*.PNG"
"$texconv" -f BC7_UNORM_SRGB -sepalpha "..\\Items\\*.PNG"
find -name "*.DDS" | while read line; do newname=`echo $line | sed s/DDS/dds/g`; mv "$line" "$newname"; done
popd
