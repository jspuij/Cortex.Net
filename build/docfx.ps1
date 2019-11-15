cd docs
dir
docfx docfx.json
cd ..\..\Cortex.Net.Docs\
git checkout master
cd ..\Cortex.Net\docs\
xcopy .\_site\*.* ..\..\Cortex.Net.Docs\ /Y /E
cd ..\..\Cortex.Net.Docs\
git commit -m "Automated Documentation."
git push