const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
    entry:  [
            './node_modules/todomvc-common/base.js',
            './node_modules/todomvc-common/base.css',
            './node_modules/todomvc-app-css/index.css'
        ]
,
    module: {
        rules: [
            {
                test: /\.(js|jsx)$/,
                use: {
                    loader: "babel-loader"
                }
            },
            {
                test: /\.css$/,
                use: ['style-loader', MiniCssExtractPlugin.loader, 'css-loader']
            }
        ]
    },
    output: {
        path: path.resolve(__dirname, './wwwroot/js'),
        filename: "app.js",
        library: "cortex_net_blazortodo"
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: '../css/style.css'
        })
    ]
};