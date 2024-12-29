using System.Runtime.CompilerServices;

namespace CSharp3D;

public static class TerrainGenerator
{
    public static void GenerateChunk(Chunk chunk, World world)
    {
        // Console.WriteLine($"Generating chunk {chunk.Position.X}, {chunk.Position.Z}...");


        
        for (int y = 0; y < Chunk.Dimensions.Y; y++)
        for (int x = 0; x < Chunk.Dimensions.X; x++)
        for (int z = 0; z < Chunk.Dimensions.Z; z++)
        {
            /*
                     * --- Chunk.Dimensions.Y
                     *  | 
                     *  |
                     *  | ...
                     *  |
                     *  |
                     * --- 0
                     */
            
            float amplitude = 20f;
            float frequency = 0.1f;

            float height = (float)(amplitude * Math.Sin(frequency * (x + chunk.Position.X)) * Math.Sin(frequency * (z + chunk.Position.Z)));

            amplitude = 2f;
            frequency = 1f;
            
            float height2 = (float)(amplitude * Math.Sin(frequency * (x + chunk.Position.X)) * Math.Sin(frequency * (z + chunk.Position.Z)));

            height += height2;

            amplitude = 30f;
            frequency = 0.02f;
            
            float height3 = (float)(amplitude * Math.Sin(frequency * (x + chunk.Position.X)) * Math.Sin(frequency * (z + chunk.Position.Z)));

            height += height3;
            
            //
            // for (int i = 0; i < 4; i++)
            // {
            //     height += (float)(amplitude * Math.Sin(frequency * (x + chunk.Position.X)) * Math.Sin(frequency * (z + chunk.Position.Z)));
            //     frequency *= 0.8f;
            //     amplitude *= 2.0f;
            // }

            if (y < 57 + height)
                chunk.Blocks[x, y, z] = BlockType.Stone;
            else if (y < 60 + height)
                chunk.Blocks[x, y, z] = BlockType.Dirt;
            else if (y < 61 + height)
                chunk.Blocks[x, y, z] = BlockType.Grass;
            else
                chunk.Blocks[x, y, z] = chunk.Blocks[x, y, z];
        }

        var seed = chunk.GetHashCode();
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

}