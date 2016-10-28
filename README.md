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

You should now have a Web Application. If you start a terminal/command prompt and run following command inside the project folder:
```
dotnet restore

dotnet build

dotnet run
```

This will start the web application listening at port 5000 (default in asp.net core generated application)

If you browse [http://localhost:5000](http://localhost:5000), it will display,
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

Add `appsettings.json` and `appsettings.<env>.json`, for whichever environment you are running, under project folder.

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

You can now run the application and it should be able to serve files under `node_modules`

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
