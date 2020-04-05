echo "Creating directory for release: $1"
mkdir -p releases/$1
echo "Copying files in $1"
cp $1/Script.cs releases/$1/Script.cs
cp $1/thumb.png releases/$1/thumb.png
echo "Creating ${1%/}.zip"
cd releases/$1/..
zip -r $(basename $1).zip $(basename $1)
