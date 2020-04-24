RC_DIR="${1%/}_$2"
echo "Creating directory for release: $RC_DIR"
mkdir -p releases/$RC_DIR
echo "Copying files from $1"
cp $1/Script.cs releases/$RC_DIR/Script.cs
cp $1/thumb.png releases/$RC_DIR/thumb.png
echo "Creating $RC_DIR.zip"
cd releases/$RC_DIR/..
zip -r $(basename $RC_DIR).zip $(basename $RC_DIR)
