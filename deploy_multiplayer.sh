#!/bin/bash

set -x
set -e

./deploy.sh

steam_id=`cat modinfo.sbmi | perl -ne 'if (/<Id>(\d+)<\/Id>/) { print("$1"); }'`
mod_folder_name="$steam_id"
steam_path="/d/Program Files/Steam"
game_mod_folder="$steam_path/steamapps/workshop/content/244850/$mod_folder_name"
dedicated_mod_folder="$APPDATA/SpaceEngineersDedicated/content/244850/$mod_folder_name"

rm -r "$game_mod_folder" || true
mkdir -p "$game_mod_folder"
cp -r build/* "$game_mod_folder"

rm -r "$dedicated_mod_folder" || true
mkdir -p "$dedicated_mod_folder"
cp -r build/* "$dedicated_mod_folder"
