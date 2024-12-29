using System.Reflection.Metadata.Ecma335;
using OpenTK.Graphics.Vulkan;
using OpenTK.Windowing.Common;

// Implement ImGuiNet too!

using var game = new Game(1920, 1080, "Hello World!");
game.VSync = VSyncMode.Off;
game.Run();