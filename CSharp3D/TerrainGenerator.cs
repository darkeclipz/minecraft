using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.Vulkan;

namespace CSharp3D;

public static class TerrainGenerator
{
    public static void GenerateChunk(Chunk chunk, World world)
    {
        var stopwatch = Stopwatch.StartNew();
        const int seaLevel = 100;

        chunk.Blocks = new Block[Chunk.Dimensions.X, Chunk.Dimensions.Y, Chunk.Dimensions.Z];
        
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
        
                if (y < seaLevel - 3 + height)
                    chunk.Blocks[x, y, z].Type = BlockType.Stone;
                else if (y < seaLevel + height)
                    chunk.Blocks[x, y, z].Type = BlockType.Dirt;
                else if (y < seaLevel + 1 + height)
                    chunk.Blocks[x, y, z].Type = BlockType.Grass;
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
                    chunk.Blocks[x, y, z].Type = BlockType.Air;
                }
            }
            
            // Underground caverns
            // {
            //     for (int x = 0; x < Chunk.Dimensions.X; x++)
            //     for (int z = 0; z < Chunk.Dimensions.Z; z++)
            //     for (int y = Chunk.Dimensions.Y - 1; y >= 0; y--)
            //     {
            //         var worldX = x + chunk.Position.X;
            //         var worldZ = z + chunk.Position.Z;
            //
            //         var amplitude = 0.008f;
            //
            //         var height = (float)(Noise.GradientNoise(amplitude * worldX, amplitude * worldZ, Game.WorldSeed) * 0.5 + 0.5);
            //         var density = Noise.GradientNoise3D(amplitude * worldX, amplitude * y, amplitude * worldZ,
            //             Game.WorldSeed);
            //
            //         if (y < seaLevel - 30 && Math.Abs(density) < 0.2f)
            //         {
            //             chunk.Blocks[x, y, z].Type = BlockType.Air;
            //         }
            //     }
            // }
            
            // Grass pass
            for (int x = 0; x < Chunk.Dimensions.X; x++)
            for (int z = 0; z < Chunk.Dimensions.Z; z++)
            for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
            {
                if (chunk.Blocks[x, y, z].Type == BlockType.Dirt && chunk.Blocks[x, y + 1, z].Type == BlockType.Air)
                {
                    chunk.Blocks[x, y, z].Type = BlockType.Grass;
                }
            }
            
            // Trees
            {
                var rng = new Random(chunk.Position.GetHashCode());
        
                // Place random tree.
                var rx = rng.Next(0, Chunk.Dimensions.X);
                var rz = rng.Next(0, Chunk.Dimensions.Z);
                var ry = chunk.HeightAt(rx, rz);
                
                var worldX = rx + chunk.Position.X;
                var worldZ = rz + chunk.Position.Z;
                
                var amplitude = 0.02f;
                var treeGradient = Noise.GradientNoise3D(amplitude * worldX, 0.1f * ry, amplitude * worldZ,
                    Game.WorldSeed);
                
                var treeSize = rng.Next(3, 8);

                var isGrass = chunk.Blocks[rx, ry, rz].Type == BlockType.Grass;
        
                if (isGrass && treeGradient < 0.0 && ry > 0 && ry + treeSize + 1 < Chunk.Dimensions.Y)
                {
                    ry += 1;
            
                    for (var i = ry; i < ry + treeSize; i++)
                    {
                        chunk.Blocks[rx, i, rz].Type = BlockType.Tree;
                    }
            
                    chunk.Blocks[rx, ry + treeSize, rz].Type = BlockType.Leaves;

                    var treeLeafRef = chunk.GetBlockRef(rx, ry + treeSize, rz);
                    
                    List<BlockRef> extraLeaves = [];

                    foreach (var neighbour in treeLeafRef.GetNeighbours9X9())
                    {
                        if (neighbour.IsAir)
                        {
                            neighbour.SetBlockType(BlockType.Leaves);    
                        }
                        
                        extraLeaves.AddRange(neighbour.GetNeighbours9X9());
                    }

                    foreach (var leaf in extraLeaves.Where(leave => leave.IsAir))
                    {
                        if (rng.NextDouble() < 0.25 + (treeSize - 2) * 0.04)
                        {
                            leaf.SetBlockType(BlockType.Leaves);
                        }
                    }
                    
                }
            }

            // Coal ore
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp((y - 20.0) / 100.0, 0.0, 1.0);

                        if (p < 0.0025 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.CoalOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.CoalOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.08)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Iron ore
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp((140.0 - y) / 60.0, 0.0, 1.0);

                        if (p < 0.002 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.DiamondOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.IronOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.08)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Gold ore
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp((140.0 - y) / 60.0, 0.0, 1.0);

                        if (p < 0.0009 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.GoldOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.GoldOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.065)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Redstone
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp((100.0 - y) / 60.0, 0.0, 1.0);

                        if (p < 0.0007 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.RedStoneOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.RedStoneOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.095)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Diamond
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp((75.0 - y) / 75.0, 0.0, 1.0);

                        if (p < 0.00055 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.DiamondOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.DiamondOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.045)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Lapus
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp((75.0 - y) / 75.0, 0.0, 1.0);

                        if (p < 0.0007 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.LapusLazuliOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.LapusLazuliOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.075)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Emerald ore
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();
                        var c = Math.Clamp(x / 150.0, 0.0, 1.0);

                        if (p < 0.00155 * c)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.EmeraldOre;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.EmeraldOre);
                                var neighbours = blockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.015)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            
            // Dirt
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                for (int y = Chunk.Dimensions.Y - 2; y >= 0; y--)
                {
                    if (chunk.Blocks[x, y, z].Type == BlockType.Stone)
                    {
                        var p = rng.NextDouble();

                        if (p < 0.00125)
                        {
                            chunk.Blocks[x, y, z].Type = BlockType.Dirt;
                            var blockRef = chunk.GetBlockRef(x, y, z);

                            var frontier = new Queue<BlockRef>();
                            frontier.Enqueue(blockRef);

                            while (frontier.TryDequeue(out var currentBlockRef))
                            {
                                currentBlockRef.SetBlockType(BlockType.Dirt);
                                var neighbours = currentBlockRef.GetNeighbours9X9();

                                foreach (var neighbour in neighbours)
                                {
                                    if (neighbour.Block.Type == BlockType.Stone && rng.NextDouble() < 0.06)
                                    {
                                        frontier.Enqueue(neighbour);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Bedrock
            {
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                {
                    chunk.Blocks[x, 0, z].Type = BlockType.Bedrock;
                }    
            }
            
            // Plants
            {
                var rng = new Random(chunk.Position.GetHashCode());
                
                for (int x = 0; x < Chunk.Dimensions.X; x++)
                for (int z = 0; z < Chunk.Dimensions.Z; z++)
                {
                    // Place random grass.
                    var ry = chunk.HeightAt(x, z);
                
                    var isGrass = chunk.Blocks[x, ry, z].Type == BlockType.Grass;
 
                    if (ry < Chunk.Dimensions.Y - 1 && isGrass && rng.NextDouble()< 0.01)
                    {
                        var flowerType = GetRandomFlower(rng);

                        chunk.Blocks[x, ry + 1, z].Type = flowerType;

                        if (flowerType == BlockType.BigPlantBottom)
                        {
                            chunk.Blocks[x, ry + 2, z].Type = BlockType.BigPlantTop;
                        }
                    }
                }
            }
            
            // Light
            for (int x = 0; x < Chunk.Dimensions.X; x++)
            for (int z = 0; z < Chunk.Dimensions.Z; z++)
            for (int y = Chunk.Dimensions.Y - 1; y >= 0; y--)
            {
                var block = chunk.Blocks[x, y, z];

                if (!Block.IsSolid(block.Type))
                {
                    chunk.Blocks[x, y, z].LightLevel = 15; 
                }
                else
                {
                    break;
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
            
            stopwatch.Stop();
            Console.WriteLine($"Generated terrain for chunk {chunk.Position.X},{chunk.Position.Z} in {stopwatch.ElapsedMilliseconds} milliseconds.");
        }

        double Lift(double x, double exp)
        {
            return Math.Pow(x, exp) + 1.0;
        }

        double Exeggarate(double x)
        {
            return Math.Pow(x, 3.0);
        }

        BlockType GetRandomFlower(Random rng)
        {
            var p = rng.NextDouble();

            if (p < 0.15) return BlockType.Flower3;
            if (p < 0.30) return BlockType.Flower4;
            if (p < 0.40) return BlockType.Flower1;
            if (p < 0.43) return BlockType.BigPlantBottom;
            return BlockType.Flower2;
        }

    }
}

