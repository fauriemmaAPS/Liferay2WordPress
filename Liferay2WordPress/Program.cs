using Liferay2WordPress;
using Liferay2WordPress.Data;
using Liferay2WordPress.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
var configuration = builder.Build();

var services = new ServiceCollection();
services.AddLogging(cfg => cfg.AddConsole());

// Config values
var liferayConn = configuration["Liferay:ConnectionString"]!;
var wpBase = configuration["WordPress:BaseUrl"]!;
var wpUser = configuration["WordPress:Username"]!;
var wpPass = configuration["WordPress:ApplicationPassword"]!;
var postType = configuration["WordPress:PostType"] ?? "posts";

services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<ILiferayRepository>(sp => new LiferayRepository(liferayConn, sp.GetRequiredService<ILogger<LiferayRepository>>()));
services.AddSingleton<ILiferayUserRepository>(sp => new LiferayUserRepository(liferayConn, sp.GetRequiredService<ILogger<LiferayUserRepository>>()));
services.AddSingleton<ILiferayDocumentRepository>(sp => new LiferayDocumentRepository(liferayConn, sp.GetRequiredService<ILogger<LiferayDocumentRepository>>()));
services.AddSingleton<ILiferayFolderRepository>(sp => new LiferayFolderRepository(liferayConn, sp.GetRequiredService<ILogger<LiferayFolderRepository>>()));
services.AddSingleton<ILiferayTemplateRepository>(sp => new LiferayTemplateRepository(liferayConn, sp.GetRequiredService<ILogger<LiferayTemplateRepository>>()));
services.AddSingleton<ILiferayStructureRepository>(sp => new LiferayStructureRepository(liferayConn, sp.GetRequiredService<ILogger<LiferayStructureRepository>>()));
services.AddSingleton<ILiferayArticleConverter, LiferayArticleConverter>();
services.AddSingleton<ITemplateScriptConverter, TemplateScriptConverter>();
services.AddHttpClient();
services.AddSingleton<IMediaMigrator, MediaMigrator>();
services.AddSingleton<IMigrationStateStore, FileMigrationStateStore>();
services.AddSingleton<IWordPressClient>(sp =>
{
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("wp");
    return new WordPressClient(http,
        sp.GetRequiredService<ILogger<WordPressClient>>(),
        new Uri(wpBase), wpUser, wpPass, postType);
});
services.AddSingleton<ICustomPostTypeGenerator>(sp => 
    new CustomPostTypeGenerator(
        sp.GetRequiredService<ILiferayStructureRepository>(),
        sp.GetRequiredService<ILogger<CustomPostTypeGenerator>>()));
services.AddSingleton<Migrator>();
services.AddSingleton<TemplateGenerator>();

var provider = services.BuildServiceProvider();

Console.WriteLine("╔═══════════════════════════════════════════════════╗");
Console.WriteLine("║  Liferay to WordPress Migration Tool              ║");
Console.WriteLine("╚═══════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"📊 Liferay Connection: {liferayConn.Split(';')[0]}...");
Console.WriteLine($"🌐 WordPress URL: {wpBase}");
Console.WriteLine();

Console.WriteLine("Choose what you want to do:");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.WriteLine("1. 🔄 Run Migration (Articles + Media)");
Console.WriteLine("2. 📄 Generate Page Templates for WordPress");
Console.WriteLine("3. 🎨 Generate Custom Post Types from Liferay Structures");
Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
Console.Write("\nEnter choice (1, 2, or 3): ");
var choice = Console.ReadLine();
Console.WriteLine();

if (choice == "1")
{
    Console.WriteLine("╔═══════════════════════════════════════╗");
    Console.WriteLine("║        Migration Mode                 ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine();
    
    var migrator = provider.GetRequiredService<Migrator>();
    var migCts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; migCts.Cancel(); };

    await migrator.RunAsync(migCts.Token);

    Console.WriteLine();
    Console.WriteLine("✓ Migration completed.");
}
else if (choice == "2")
{
    Console.WriteLine("╔═══════════════════════════════════════╗");
    Console.WriteLine("║    Template Generation Mode           ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine();
    
    var outputDir = "./generated-templates";
    var generator = provider.GetRequiredService<TemplateGenerator>();
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
    
    await generator.GenerateTemplatesAsync(outputDir, cts.Token);
    
    Console.WriteLine();
    Console.WriteLine($"✓ Templates generated in: {Path.GetFullPath(outputDir)}");
    Console.WriteLine("📝 Next step: Copy the PHP files to your WordPress theme directory.");
}
else if (choice == "3")
{
    Console.WriteLine("╔═══════════════════════════════════════╗");
    Console.WriteLine("║  Custom Post Type Generation          ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine();
    
    // Chiedi groupId
    Console.Write("Enter Liferay Group ID (default: 20143): ");
    var groupIdInput = Console.ReadLine();
    var groupId = string.IsNullOrWhiteSpace(groupIdInput) ? 20143L : long.Parse(groupIdInput);
    
    Console.WriteLine();
    Console.WriteLine($"🔍 Loading DDM Structures for Group ID: {groupId}");
    Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    Console.WriteLine();
    
    var outputDir = "./GeneratedPostTypes";
    var generator = provider.GetRequiredService<ICustomPostTypeGenerator>();
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
    
    try
    {
        await generator.GenerateFromLiferayStructuresAsync(groupId, cts.Token);
        
        Console.WriteLine();
        Console.WriteLine("╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    ✓ SUCCESS!                             ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine($"📁 Output directory: {Path.GetFullPath(outputDir)}");
        Console.WriteLine();
        Console.WriteLine("📋 Next Steps:");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("1. Install 'Advanced Custom Fields PRO' plugin in WordPress");
        Console.WriteLine($"2. Copy 'acf-json' folder to your WordPress theme");
        Console.WriteLine("3. Copy PHP files from 'post-types' folder to your theme");
        Console.WriteLine("4. Flush permalinks: Settings → Permalinks → Save");
        Console.WriteLine("5. Check individual README files for detailed instructions");
        Console.WriteLine();
        Console.WriteLine($"📖 See: {Path.GetFullPath(Path.Combine(outputDir, "INSTALLATION_GUIDE.md"))}");
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        Console.WriteLine("❌ Error during generation:");
        Console.WriteLine(ex.Message);
        Console.WriteLine();
        Console.WriteLine("Possible issues:");
        Console.WriteLine("- Invalid Group ID");
        Console.WriteLine("- Database connection error");
        Console.WriteLine("- No structures found for this group");
    }
}
else
{
    Console.WriteLine("❌ Invalid choice. Exiting.");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();



