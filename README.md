# ASP.NET Core with Angular 2
This is a shell project that uses Angular 2 with Asp.Net core. The read me will detail all the steps that I followed (may have some redundant details from official quick starts and guides).

## Asp.Net Web API
There are multiple options when creating a new Asp.Net Web Application project.
- Using Visual Studio 2015 Tooling Preview
- Using DotNet Cli
- Using Yeomen Generator
- Writing from Scratch

Here I used Yeoman generator to generate the Asp.Net project. The steps to do that,
1. Install Node.js, npm
2. Install Yeoman
```
npm install -g yo
```
 *`-g` option for global install*
3. Install ASP.NET generator
```
npm install -g generator-aspnet
```
4. Run ASP.NET generator
```                 
npm install -g generator-aspnet
```

5. Select "Empty Web Application" from the options given.

We should now have a Web Application. If we start a terminal/command prompt and run following command inside the project folder:
```
dotnet restore

dotnet build

dotnet run
```

This will start the web application listening at port 5000 (default in asp.net core generated application)

If we browse [http://localhost:5000](http://localhost:5000), it will display,
```
Hello World!
```

## Setting up Web Application

### Reading configuration from appsettings.json
This step is probably not necessary to set up Angular 2. But I like to have this set up for any application.
Change the `Startup.cs` constructor method,
```cs
public class Startup
{
    private IConfiguration configuration;

    public Startup(IHostingEnvironment env){
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json");
        
        configuration = builder.Build();
    }
    ...
    ...
}
```

Add `appsettings.json` and `appsettings.<env>.json`, for whichever environment we are running, under project folder.

### Static File Server for wwwroot
To serve the files inside `wwwroot` folder, we need to add a middleware. Just change the, `Configure` method in `Startup.cs`.

```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddConsole();

    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseFileServer();

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync(configuration["testValue"]);
    });
}
```

### Static File Server for node_modules
By default the File Server middleware will only serve the files under `wwwroot`. However, we can add another middleware which will serve the files under `node_module` folder under project root folder.

We need this for a simple reason that, libraries like angular will be installed under `node_module` by `npm`. So our server needs to be able to resove the requests for files under `node_module` folder.

One way to add this middleware to write an extension method that extends IApplicationBuilder,
```cs
namespace Microsoft.AspNet.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseNodModules(this IApplicationBuilder app
            , IHostingEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "node_modules");
            var provider = new PhysicalFileProvider(path);
            
            var options = new StaticFileOptions();
            options.RequestPath = "/node_modules";
            options.FileProvider = provider;
            
            app.UseStaticFiles(options);
            return app;
        }

    }
}
```
This extension method is configures Static File server to serve files under `node_modules` directory when request has `/node_modules`


Now add this extension middleware to the `Configure` method in `Startup.cs`
```cs
public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
{
    loggerFactory.AddConsole();

    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseFileServer();

    app.UseNodeModules(env);

    app.Run(async (context) =>
    {
        await context.Response.WriteAsync(configuration["testValue"]);
    });
}
```

We can now run the application and it should be able to serve files under `node_modules`

## Setting up Angular
Here I am going to follow Angular Quickstart documentation as closely as possible.

Create files `package.json` and `tsconfig.json` in the project root folder.
Create file `systemjs.config.js` in the `wwwroot` folder

Copy the content of all three files form the angular Quickstart.

Modify the content of the `tsconfig.json` to,

```json
{
  "compilerOptions": {
    "target": "es5",
    "watch": true,
    "module": "commonjs",
    "moduleResolution": "node",
    "sourceMap": true,
    "emitDecoratorMetadata": true,
    "experimentalDecorators": true,
    "removeComments": false,
    "noImplicitAny": false,
    "outDir": "./wwwroot/app"
  },
  "compileOnSave": true,
  "exclude": [
    "node_modules",
    "wwwroot"
  ]
}
```
I added `outDir` to generate the compiled js file inside `wwwroot/app` folder, where it'll be used.

Also, set `compileOnSave` and run `tsc -w` on the project folder.

Exclude `wwwroot` and `node_modules` since we don't need to compile those.

For everything else follow the Angular2-quickstart guide, it should be working.

## Moving to Webpack
Is a module (ES6 modules not Angular) bundler.
It's an alternative to SystemJS.

### Removing SystemJS
Remove the code we already have in place for systemjs.

1. Remove following code from `index.cshtml` file
```html
<script>
  System.import('app').catch(function(err){ console.error(err); });
</script>
```

2. Delete `wwwroot/systemjs.config.js` file

This should break our project. Now let's get started with setting up webpack.

### Adding webpack packages to package.json
1. Add following packages to package.json
```json
"devDependencies":{
    "angular2-template-loader":"^0.6.0",
    "awesome-typescript-loader":"^2.2.4",
    "css-loader":"^0.25.0",
    "extract-text-webpack-plugin":"^1.0.1",
    "file-loader":"^0.9.0",
    "html-loader":"^0.4.4",
    "raw-loader":"^0.5.1",
    "style-loader":"^0.13.1",
    "webpack":"^1.13.3",
    "webpack-merge": "^0.15.0",
    "webpack-md5-hash": "^0.0.5",
    "copy-webpack-plugin": "^4.0.0",
    "source-map-loader": "^0.1.5",
    "script-ext-html-webpack-plugin": "^1.3.2"
}
```
These are the webpack core file and loaders that webpack will use to load different type of files.

2. Run `npm install`
### Moving source path
1. Create file `src/polyfills.ts`
```js
import 'core-js/es6';
import 'core-js/es7/reflect';
require('zone.js/dist/zone');
if (process.env.ENV === 'production') {
  // Production
} else {
  // Development
  Error['stackTraceLimit'] = Infinity;
  require('zone.js/dist/long-stack-trace-zone');
}
```

2. Create file `src/vendor.ts`
```js
// Angular
import '@angular/platform-browser';
import '@angular/platform-browser-dynamic';
import '@angular/core';
import '@angular/common';
import '@angular/http';
import '@angular/router';
// RxJS
import 'rxjs';
// Other vendors for example jQuery, Lodash or Bootstrap
// You can import js, ts, css, sass, ...
```

3. Move `main.ts` under `src` folder
4. Move following files under `src/app/` folder
    - `app.component.ts`
    - `app.component.html`
    - `app.component.css`
    - `app.module.ts`



### Configuring webpack
We are going to follow a lot of angular 2 guide on webpack, except for changes that will be specific to make it work with Asp.Net Core

1. We're no longer going to use typescript compiler to write our `js` files to `wwwroot`. So let's modify our typescript to look like:
```json
{
  "compilerOptions": {
    "target": "es5",
    "watch": true,
    "module": "commonjs",
    "moduleResolution": "node",
    "sourceMap": true,
    "emitDecoratorMetadata": true,
    "experimentalDecorators": true,
    "removeComments": false,
    "noImplicitAny": true,
    "suppressImplicitAnyIndexErrors": true
  }
}
```

2. Create Config file for webpack `webpack.config.js`
```js
module.exports = require('./config/webpack.dev.js');
```

3. Create helper file for webpack `config/helper.js`
```js
var path = require('path');
var _root = path.resolve(__dirname, '..');
function root(args) {
  args = Array.prototype.slice.call(arguments, 0);
  return path.join.apply(path, [_root].concat(args));
}
exports.root = root;
```
This uses a Nodejs module to give easy access to webpack's config files to the absolute path to different files.

4. Add common configuration `config/webpack.common.js`
```js
var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var helpers = require('./helpers');

module.exports = {
  entry: {
    'polyfills': './src/polyfills.ts',
    'vendor': './src/vendor.ts',
    'app': './src/main.ts'
  },

  resolve: {
    extensions: ['', '.ts', '.js']
  },

  module: {
    loaders: [
      {
        test: /\.ts$/,
        loaders: ['awesome-typescript-loader', 'angular2-template-loader']
      },
      {
        test: /\.html$/,
        loader: 'html'
      },
      {
        test: /\.(png|jpe?g|gif|svg|woff|woff2|ttf|eot|ico)$/,
        loader: 'file?name=../assets/[name].[hash].[ext]'
      },
      {
        test: /\.css$/,
        exclude: helpers.root('src', 'app'),
        loader: ExtractTextPlugin.extract('style', 'css?sourceMap')
      },
      {
        test: /\.css$/,
        include: helpers.root('src', 'app'),
        loader: 'raw'
      }
    ]
  },

  plugins: [
    new webpack.optimize.CommonsChunkPlugin({
      name: ['app', 'vendor', 'polyfills']
    })
  ]
};
``` 

Since we're using razor views `index.cshtml` we cannot use `HtmlwebpackPlugin`. We'll manually have to add the proper script and stylesheet reference to the file later.

5. Add dev configuration for webpack `webpack.dev.js`

```js
var webpackMerge = require('webpack-merge');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var commonConfig = require('./webpack.common.js');
var helpers = require('./helpers');

module.exports = webpackMerge(commonConfig, {
  devtool: 'cheap-module-eval-source-map',

  output: {
    path: helpers.root('wwwroot', 'app'),
    filename: '[name].js',
    chunkFilename: '[id].chunk.js'
  },

  plugins: [
    new ExtractTextPlugin('[name].css')
  ]
});
```

Since we're not hosting using webpack-dev-server, we can remove the public path attribute. Also configuration for the dev server can be removed.


6. Add production configuration for webpack `webpack.prod.js`

```js
var webpack = require('webpack');
var webpackMerge = require('webpack-merge');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var commonConfig = require('./webpack.common.js');
var helpers = require('./helpers');

const ENV = process.env.NODE_ENV = process.env.ENV = 'production';

module.exports = webpackMerge(commonConfig, {
  devtool: 'source-map',

  output: {
    path: helpers.root('wwwroot', 'app'),
    publicPath: '/',
    filename: '[name].js',
    chunkFilename: '[id].chunk.js'
  },

  htmlLoader: {
    minimize: false // workaround for ng2
  },

  plugins: [
    new webpack.NoErrorsPlugin(),
    new webpack.optimize.DedupePlugin(),
    new webpack.optimize.UglifyJsPlugin({ // https://github.com/angular/angular/issues/10618
      mangle: {
        keep_fnames: true
      }
    }),
    new ExtractTextPlugin('[name].css'),
    new webpack.DefinePlugin({
      'process.env': {
        'ENV': JSON.stringify(ENV)
      }
    })
  ]
});
```
### Run webpack
- For Debug package run `webpack -d`
- For production package run `webpack -p`

### Add file references to Index.cshtml
```html
@{
  ViewBag.Title = "Angular ASP.NET CORE";
}

<my-app>Loading...</my-app>

<link type="text/css" rel="stylesheet" href="./css/styles.css" />
<script src="./app/polyfills.js"></script>
<script src="./app/vendor.js"></script>
<script src="./app/app.js"></script>
```
# References:
- [ASP.NET Core 1.0 Fundamentals by Scott Allen on Pluralsight](https://app.pluralsight.com/library/courses/aspdotnet-core-1-0-fundamentals/)
- [ASP.NET Core 1.0 Documentation](https://docs.asp.net/en/latest/)
- [Angular 2 Quickstart](https://angular.io/docs/ts/latest/quickstart.html)
