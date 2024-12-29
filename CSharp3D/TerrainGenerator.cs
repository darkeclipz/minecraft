using System.Runtime.CompilerServices;
using OpenTK.Graphics.Vulkan;

namespace CSharp3D;

public static class TerrainGenerator
{
    public static void GenerateChunk(Chunk chunk, World world)
    {
        var seed = chunk.GetHashCode();

        // Overworld
        {
            for (int x = 0; x < Chunk.Dimensions.X; x++)
            for (int z = 0; z < Chunk.Dimensions.Z; z++)
            for (int y = Chunk.Dimensions.Y - 1; y >= 0; y--)
            {
                var worldX = x + chunk.Position.X;
                var worldZ = z + chunk.Position.Z;
        
                var frequency = 0.1f;
                var amplitude = 10f;
                var hills = amplitude * Noise.GradientNoise(frequency * worldX, frequency * worldZ, Game.WorldSeed);
                var mountainY = Noise.GradientNoise(0.003f * worldX, 0.003f * worldZ, Game.WorldSeed);
                var mountains = 100 * Exeggarate(2 * Math.Clamp(mountainY, 0.0, 1.0));
                var flatness = Math.Clamp(Noise.GradientNoise(0.001f * worldX, 0.001f * worldZ, Game.WorldSeed), -0.3, 1.0);
                var height = mountains + hills * flatness;
        
                if (y < 57 + height)
                    chunk.Blocks[x, y, z] = BlockType.Stone;
                else if (y < 60 + height)
                    chunk.Blocks[x, y, z] = BlockType.Dirt;
                else if (y < 61 + height)
                    chunk.Blocks[x, y, z] = BlockType.Grass;
                else
                    chunk.Blocks[x, y, z] = chunk.Blocks[x, y, z];
            }
        }

        // Underground tunnels
        {
            for (int x = 0; x < Chunk.Dimensions.X; x++)
            for (int z = 0; z < Chunk.Dimensions.Z; z++)
            for (int y = Chunk.Dimensions.Y - 1; y >= 0; y--)
            {
                var worldX = x + chunk.Position.X;
                var worldZ = z + chunk.Position.Z;

                var amplitude = 0.05f;

                var cavyness = Math.Clamp(Noise.GradientNoise(0.001f * worldX, 0.001f * worldZ, Game.WorldSeed), -0.3, 1.0);
                var limit = 60;
                var depthFactor = 0.5 * Math.Clamp((limit - y) / limit, 0.0, 1.0);

                var amplitudeOffsetX = 0.2f * (float)(Noise.GradientNoise(0.001f * worldX, 0.001f * worldZ, Game.WorldSeed) * amplitude * 0.5);
                var amplitudeOffsetY = 0.2f * (float)(Noise.GradientNoise(0.001f * worldX + 9347f, 0.001f * worldZ + 8126f, Game.WorldSeed) * amplitude * 0.5);

                var density = Noise.GradientNoise3D((amplitude + amplitudeOffsetX) * worldX, 0.1f * y, (amplitude + amplitudeOffsetY) * worldZ,
                    Game.WorldSeed);

                if (Math.Abs(density - 0.5) < 0.2)
                {
                    chunk.Blocks[x, y, z] = BlockType.Air;
                }
            }
            
            // Underground caverns
            {
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 1; y >= 0; y--)
                {
                    var worldX = x + chunk.Position.X;
                    var worldZ = z + chunk.Position.Z;

                    var amplitude = 0.03f;

                    var density = Noise.GradientNoise3D(amplitude * worldX, 0.1f * y, amplitude * worldZ,
                        Game.WorldSeed);

                    if (y < 50 && Math.Abs(density - 0.5) < 0.2f)
                    {
                        chunk.Blocks[x, y, z] = BlockType.Air;
                    }
                }
            }
            
            // Grass pass
            for (int x = 0; x < Chunk.Dimensions.X; x++)
            for (int z = 0; z < Chunk.Dimensions.Z; z++)
            for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
            {
                if (chunk.Blocks[x, y, z] == BlockType.Dirt && chunk.Blocks[x, y + 1, z] == BlockType.Air)
                {
                    chunk.Blocks[x, y, z] = BlockType.Grass;
                }
            }
            
            // Trees
            {
                var rng = new Random(seed);
        
                // Place random tree.
                var rx = rng.Next(0, Chunk.Dimensions.X);
                var rz = rng.Next(0, Chunk.Dimensions.Z);
                var ry = chunk.HeightAt(rx, rz);
                var treeSize = rng.Next(3, 8);
        
                if (ry > 0 && ry + treeSize + 1 < Chunk.Dimensions.Y)
                {
                    ry += 1;
            
                    for (var i = ry; i < ry + treeSize; i++)
                    {
                        chunk.Blocks[rx, i, rz] = BlockType.Tree;
                    }
            
                    chunk.Blocks[rx, ry + treeSize, rz] = BlockType.Leaves;
                }
            }
            
            // // Invert solids to see the caverns.
            // for (int x = 0; x < Chunk.Dimensions.X; x++)
            // for (int z = 0; z < Chunk.Dimensions.Z; z++)
            // for (int y = Chunk.Dimensions.Y - 1; y >= 0; y--)
            // {
            //     if (chunk.Blocks[x, y, z] == BlockType.Air)
            //     {
            //         chunk.Blocks[x, y, z] = BlockType.Grass;
            //     }
            //     else
            //     {
            //         chunk.Blocks[x, y, z] = BlockType.Air;
            //     }
            // }
        }

        double Lift(double x, double exp)
        {
            return Math.Pow(x, exp) + 1.0;
        }

        double Exeggarate(double x)
        {
            return Math.Pow(x, 3.0);
        }

    }
}

