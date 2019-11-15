git config --global user.email "$Env:GH_EMAIL"
git config --global user.name "$Env:GH_USER"
cd docs
git submodule add -f --name Cortex.Net.Docs "https://github.com/jspuij/Cortex.Net.Docs" "_site"
dir
