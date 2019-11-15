git config --global user.email "$Env:GH_EMAIL"
git config --global user.name "$Env:GH_USER"
cd docs
echo "https://jspuij:$Env:BUILD_TOKEN@github.com/jspuij/Cortex.Net.Docs.git"
git submodule add -f --name Cortex.Net.Docs "https://jspuij:$Env:BUILD_TOKEN@github.com/jspuij/Cortex.Net.Docs.git" "_site"
dir
