namespace CSharp3D;

public enum BlockType : byte
{
    Air = 0,
    Grass = 1,
    Dirt = 2,
    GrassSide = 3,
    Stone = 4,
    Brick = 5,
    Sand = 6,
    Snow = 7,
    Tree = 8,
    TreeTop = 9,
    Leaves = 10,
    SinisterLeaves = 11,
    SinisterGrass = 12,
    Bedrock = 13,
    Flower1 = 14,
    Flower2 = 15,
    Flower3 = 16,
    Flower4 = 17,
    IronOre = 18,
    CoalOre = 19,
    DiamondOre = 20,
    BigPlantTop = 21,
    BigPlantBottom = 22,
    GoldOre = 23,
    RedStoneOre = 24,
    EmeraldOre = 25,
    LapusLazuliOre = 26,
}

public partial struct Block
{
    public static bool IsSolid(BlockType type) => type switch
    {
        BlockType.Air => false,
        BlockType.Flower1 => false,
        BlockType.Flower2 => false,
        BlockType.Flower3 => false,
        BlockType.Flower4 => false,
        BlockType.BigPlantBottom => false,
        BlockType.BigPlantTop => false,
        _ => true
    };

    public static bool IsPlant(BlockType type) => type switch
    {
        BlockType.Flower1 => true,
        BlockType.Flower2 => true,
        BlockType.Flower3 => true,
        BlockType.Flower4 => true,
        BlockType.BigPlantBottom => true,
        BlockType.BigPlantTop => true,
        _ => false
    };
}