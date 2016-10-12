var webpackMerge = require('webpack-merge');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var commonConfig = require('./webpack.common.js');
var helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, {
    // devtool specifies a bunch of presets that determine build speed and output quality
    // https://webpack.github.io/docs/configuration.html#devtool
    devtool: 'cheap-module-eval-source-map',

    output: {
        // Directory that output will be written to
        path: helpers.root('dist'),
        // Url that output will be hosted at (can be absolute or relative)
        publicPath: 'http://localhost:8080/',
        filename: '[name].js',
        chunkFilename: '[id].chunk.js'
    },

    plugins: [
        new ExtractTextPlugin('[name].css')
    ],

    devServer: {
        historyApiFallback: true,
        stats: 'minimal'
    }
});
