namespace CSharp3D;

public static class TerrainGenerator
{
    public static void GenerateChunk(Chunk chunk, World world)
    {
        Console.WriteLine($"Generating chunk {chunk.Position.X}, {chunk.Position.Z}...");

        const float amplitude = 10f;
        const float frequency = 0.1f;
        
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

            float height = (float)(amplitude * Math.Sin(frequency * (x + chunk.Position.X)) * Math.Sin(frequency * (z + chunk.Position.Z)));

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
}