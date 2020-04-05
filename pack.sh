echo "Linking files in $1 to $1/Script.cs"
if [ -f $1/Script.cs ]; then
  echo "Moving $1/Script.cs to $1/.Script.old.cs"
  mv -f $1/Script.cs $1/.Script.old.cs
fi
echo "Found files:"
for file in $1/_*.cs; do
  echo $file
  # append file, starting and ending with filename in comments
  echo -e "//\n// XXX $file XXX\n//\n" >> $1/Script.cs
  cat $file >> $1/Script.cs
  echo >> $1/Script.cs
done
