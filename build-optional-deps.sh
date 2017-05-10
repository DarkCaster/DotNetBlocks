#!/bin/bash

script_dir="$( cd "$( dirname "$0" )" && pwd )"

check_error () {
if [ "$?" != "0" ]; then
	echo "Build failed"
	exit 1
fi
}

rm -f "$script_dir/NuGet.Config"
check_error

"$script_dir/External/DotNetBuildTools/prepare-and-build.sh" "$script_dir/External"
check_error

cp "$script_dir/External/DotNetBuildTools/dist/extra/NuGet.Config" "$script_dir/NuGet.Config"
check_error

chmod 644 "$script_dir/NuGet.Config"
check_error
