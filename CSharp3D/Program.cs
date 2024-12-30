using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Configuration;
using OpenTK.Graphics.Vulkan;
using OpenTK.Windowing.Common;

// Implement ImGuiNet too!

var builder = new ConfigurationBuilder();
builder.AddJsonFile("appsettings.json");
var configuration = builder.Build();

int width = int.Parse(configuration["Window:Width"]);
int height = int.Parse(configuration["Window:Height"]);

using var game = new Game(width, height, configuration);
game.VSync = VSyncMode.Off;
game.Run();