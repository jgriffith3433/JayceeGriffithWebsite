const CopyWebpackPlugin = require('copy-webpack-plugin');
 
module.exports = {
	context: __dirname,
    plugins: [
        new CopyWebpackPlugin({
            patterns: [
                { from: 'src/assets', to: 'assets' }
            ]
        })
    ]
};